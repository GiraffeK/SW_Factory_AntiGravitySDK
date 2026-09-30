"""Software Factory Orchestrator Engine.

Implements the multi-agent assembly pipeline and the external closed-loop
verification / self-repair harness using the Antigravity Python SDK.
"""

from __future__ import annotations

import asyncio
import dataclasses
import os
import subprocess
import sys
import logging
from typing import AsyncGenerator

from dotenv import load_dotenv
from google.antigravity import (
    Agent,
    LocalAgentConfig,
    CapabilitiesConfig,
    ModelTarget,
    AgentBehavior,
)
from google.antigravity.models import ModelType, GeminiAPIEndpoint
from google.antigravity.policy import allow_all

from .roles import get_factory_subagents, get_orchestrator_instructions

# Ensure UTF-8 output on Windows
if sys.platform == "win32":
    sys.stdout.reconfigure(encoding="utf-8")

# Suppress retry warning logs
logging.getLogger("google.antigravity").setLevel(logging.ERROR)
logging.getLogger("root").setLevel(logging.ERROR)

load_dotenv()


@dataclasses.dataclass
class FactoryResult:
    """Represents the final outcome of the software factory build."""
    success: bool
    workspace: str
    task: str
    repair_rounds_used: int
    test_output: str
    summary_report: str


class SoftwareFactory:
    """Industrial-grade autonomous Software Factory driven by Antigravity SDK."""

    def __init__(
        self,
        workspace_dir: str,
        api_key: str | None = None,
        primary_model: str = "gemini-3-flash-preview",
        fallback_model: str = "gemini-3.1-flash-lite-preview",
    ):
        self.workspace_dir = os.path.abspath(workspace_dir)
        os.makedirs(self.workspace_dir, exist_ok=True)
        self.api_key = api_key or os.environ.get("GEMINI_API_KEY")
        if not self.api_key:
            raise ValueError(
                "GEMINI_API_KEY is required. Please set it in .env or pass it explicitly."
            )

        self.primary_model = primary_model
        self.fallback_model = fallback_model

    def _build_models(self) -> list[ModelTarget]:
        endpoint = GeminiAPIEndpoint(api_key=self.api_key)
        return [
            ModelTarget(name=self.primary_model, types=[ModelType.TEXT], endpoint=endpoint),
            ModelTarget(name=self.fallback_model, types=[ModelType.TEXT], endpoint=endpoint),
        ]

    def _run_external_test(self, test_cmd: str) -> tuple[bool, str]:
        """Runs the test command locally to independently verify quality gate."""
        try:
            res = subprocess.run(
                test_cmd,
                shell=True,
                cwd=self.workspace_dir,
                capture_output=True,
                text=True,
                encoding="utf-8",
                errors="replace",
                timeout=60,
            )
            output = f"Stdout:\n{res.stdout}\n\nStderr:\n{res.stderr}"
            return (res.returncode == 0), output
        except Exception as e:
            return False, f"Failed to execute verification command: {e}"

    async def _safe_chat(
        self, orchestrator: Agent, prompt: str, max_retries: int = 5
    ) -> str:
        """Executes orchestrator.chat with streaming and automatic 429 rate limit backoff."""
        accumulated_text = ""
        current_prompt = prompt

        for attempt in range(1, max_retries + 1):
            try:
                response = await orchestrator.chat(current_prompt)
                async for token in response:
                    sys.stdout.write(token)
                    sys.stdout.flush()
                    accumulated_text += token
                return accumulated_text
            except Exception as e:
                err_msg = str(e)
                if (
                    "429" in err_msg
                    or "RESOURCE_EXHAUSTED" in err_msg
                    or "Quota exceeded" in err_msg
                    or "retryDelay" in err_msg
                ):
                    import re
                    match = re.search(r'retryDelay["\']?:\s*["\']?([0-9.]+)', err_msg)
                    wait_seconds = float(match.group(1)) + 5.0 if match else 45.0
                    print(
                        f"\n⏳ [429 速率限制保護] 觸發 API 每分鐘頻率上限，自動冷卻 {wait_seconds:.1f} 秒後恢復 (重試 {attempt}/{max_retries})..."
                    )
                    await asyncio.sleep(wait_seconds)
                    current_prompt = "請繼續剛才受到頻率限制而暫停的步驟，檢視工作區狀態並完成未完成的工作。"
                else:
                    raise
        return accumulated_text

    async def build(
        self,
        task: str,
        test_cmd: str = "pytest -v",
        max_repair_rounds: int = 3,
    ) -> FactoryResult:
        """Executes the full end-to-end software factory pipeline.

        1. Spawns Lead Agent with Architect, Coder, Tester, Reviewer subagents.
        2. Executes autonomous multi-agent development.
        3. Quality Gate: Independently executes test_cmd in the workspace.
        4. Closed-Loop Self-Healing: If tests fail, feeds traceback back to Coder
           until 100% green or max_repair_rounds reached.
        """
        print("=" * 70)
        print("🏭 【ANTIGRAVITY 軟體工廠 - 自動化流水線啟動】")
        print(f"📁 目標工作區: {self.workspace_dir}")
        print(f"📋 交付任務: {task}")
        print(f"🧪 驗證指令: {test_cmd}")
        print("=" * 70 + "\n")

        models = self._build_models()
        subagents = get_factory_subagents(model=self.primary_model)
        instructions = get_orchestrator_instructions(self.workspace_dir, test_cmd)

        factory_app_data = os.path.abspath(os.path.join(self.workspace_dir, ".factory_app_data"))
        os.makedirs(factory_app_data, exist_ok=True)

        from google.antigravity import types as agy_types
        retry_config = agy_types.RetryConfig(
            api_retry=agy_types.ModelAPIRetryConfig(
                max_retries=6,
                initial_sleep_duration_ms=10000,
                exponential_multiplier=1.8,
            )
        )

        config = LocalAgentConfig(
            models=models,
            system_instructions=instructions,
            workspaces=[self.workspace_dir],
            subagents=subagents,
            policies=[allow_all()],
            skills_paths=[],  # 停用全域無關插件，減少 99% 的無效 Token 消耗
            app_data_dir=factory_app_data,  # 隔離專案應用資料夾
            retry_config=retry_config,  # 啟用自動指數退避重試，遇 429 自動等待冷卻
            capabilities=CapabilitiesConfig(
                agent_behavior=AgentBehavior.AUTONOMOUS,
            ),
            api_key=self.api_key,
        )

        repair_rounds_used = 0
        final_test_output = ""
        success = False

        async with Agent(config) as orchestrator:
            # 階段一：啟動流水線（架構 -> 實作 -> 測試）
            print("🚀 [階段 1/3] 調度多智能體流水線（Architect -> Coder -> Tester）...\n")
            initial_prompt = (
                f"【啟動流水線】\n"
                f"使用者交付軟體開發任務：\n{task}\n\n"
                f"請自主協調 architect 設計架構、coder 實作核心檔案、tester 撰寫測試，"
                f"並於工作區 [{self.workspace_dir}] 執行 `{test_cmd}` 進行初次驗證。"
            )

            await self._safe_chat(orchestrator, initial_prompt)
            print("\n")

            # 階段二：獨立品管閘門與閉環自我修復 (Closed-Loop Quality Gate)
            print("-" * 70)
            print("🔍 [階段 2/3] 啟動獨立品質驗證閘門 (Running Quality Gate)...")
            passed, test_output = self._run_external_test(test_cmd)
            final_test_output = test_output

            if passed:
                print("✅ 品管閘門初次驗證即 100% 通過！")
                success = True
            else:
                print("⚠️ 測試未完全通過，啟動閉環自我反思與修復循環 (Self-Healing Loop)...")
                for r in range(1, max_repair_rounds + 1):
                    repair_rounds_used = r
                    print(f"\n🔄 [修復循環 {r}/{max_repair_rounds}] 將錯誤日誌回傳給 Agent 進行定位與修復...")

                    repair_prompt = (
                        f"【品管閘門錯誤警報】\n"
                        f"執行驗證指令 `{test_cmd}` 失敗。\n"
                        f"以下為完整的錯誤日誌與 Traceback：\n\n"
                        f"```\n{test_output}\n```\n\n"
                        f"請調用 coder 分析上述錯誤根本原因，修正相關檔案，確保所有測試通過。"
                    )

                    await self._safe_chat(orchestrator, repair_prompt)
                    print("\n")

                    # 再次驗證
                    passed, test_output = self._run_external_test(test_cmd)
                    final_test_output = test_output
                    if passed:
                        print(f"🎉 [修復成功] 在第 {r} 次修復循環後，所有測試已 100% 通過！")
                        success = True
                        break
                    else:
                        print(f"❌ 第 {r} 次修復後測試仍未通過。")

            # 階段三：生成工廠交付報告
            print("-" * 70)
            print("📑 [階段 3/3] 結算交付報告...")
            report_prompt = (
                f"請檢查工作區根目錄，產生最終的 `FACTORY_REPORT.md`，"
                f"統整本次架構設計、核心模組清單、測試結果與使用說明。"
            )
            report_content = await self._safe_chat(orchestrator, report_prompt)

        print("\n" + "=" * 70)
        status_text = "SUCCESS ✅ (100% Passed)" if success else "FAILED ❌ (Requires Human Inspection)"
        print(f"🏭 軟體工廠生產完畢！最終狀態: {status_text}")
        print(f"📂 成果目錄: {self.workspace_dir}")
        print("=" * 70 + "\n")

        return FactoryResult(
            success=success,
            workspace=self.workspace_dir,
            task=task,
            repair_rounds_used=repair_rounds_used,
            test_output=final_test_output,
            summary_report=report_content,
        )

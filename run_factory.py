"""CLI Entrypoint for the Antigravity Software Factory.

Usage:
    python run_factory.py
    python run_factory.py --task "建立一個自訂快取與過期機制的 LRUCache，含完整測試"
"""

import argparse
import asyncio
import os
import sys

from software_factory import SoftwareFactory

DEFAULT_TASK = (
    "建立一個完整的 Python 非同步任務佇列函式庫 (Async Priority Task Queue)：\n"
    "1. 支援工作優先級 (Priority: High, Medium, Low)\n"
    "2. 支援最大並發限制 (Concurrency limit / Semaphore)\n"
    "3. 支援失敗重試機制 (Exponential Backoff Retry)\n"
    "4. 包含詳細的型別註解與完整的 pytest 單元測試檔案 (test_queue.py)，確保測試全數通過。"
)

async def main():
    parser = argparse.ArgumentParser(description="Antigravity Autonomous Software Factory")
    parser.add_argument(
        "--task",
        type=str,
        default=None,
        help="軟體開發需求描述 (預設為非同步優先級佇列範例)",
    )
    parser.add_argument(
        "--task-file",
        type=str,
        default=None,
        help="從文字檔讀取任務需求描述 (避免 PowerShell 多行跳脫問題)",
    )
    parser.add_argument(
        "--output",
        type=str,
        default="./output_project",
        help="工廠輸出的專案工作區路徑 (預設: ./output_project)",
    )
    parser.add_argument(
        "--test-cmd",
        type=str,
        default="pytest -v",
        help="閉環品質驗證指令 (預設: pytest -v)",
    )
    parser.add_argument(
        "--model",
        type=str,
        default="gemini-3.5-flash-lite",
        help="主要使用的 Gemini 模型 (預設: gemini-3.5-flash-lite，支援自動健康探測與 Fallback 輪替)",
    )
    args = parser.parse_args()

    if args.task_file and os.path.exists(args.task_file):
        with open(args.task_file, "r", encoding="utf-8") as f:
            task = f.read().strip()
    else:
        task = args.task or DEFAULT_TASK
    workspace = os.path.abspath(args.output)

    factory = SoftwareFactory(workspace_dir=workspace, primary_model=args.model)
    result = await factory.build(task=task, test_cmd=args.test_cmd)

    if result.success:
        print(f"\n[工廠報告] 專案已成功交付至: {result.workspace}")
        print(f"[品管數據] 經歷自我修復次數: {result.repair_rounds_used}")
    else:
        print(f"\n[工廠警告] 專案尚未完全達到 100% 通過標準，請檢查: {result.workspace}")

if __name__ == "__main__":
    asyncio.run(main())

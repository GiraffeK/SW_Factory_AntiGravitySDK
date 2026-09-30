"""Role definitions for the Antigravity Software Factory.

Defines the specialized subagents (Architect, Coder, Tester, Reviewer)
and orchestrator prompts.
"""

from google.antigravity import BuiltinTools, AgentBehavior
from google.antigravity.types import SubagentConfig, SubagentCapabilities


def get_factory_subagents(model: str = "gemini-3-flash-preview") -> list[SubagentConfig]:
    """Returns the standardized subagents configured for the software factory."""
    autonomous_capabilities = SubagentCapabilities(
        enabled_tools=BuiltinTools.default(),
        agent_behavior=AgentBehavior.AUTONOMOUS,
    )

    return [
        SubagentConfig(
            name="architect",
            description="軟體系統架構師。負責分析需求、規劃專案目錄架構、定義模組間的 API Contract 與資料結構。",
            system_instructions="""你是一個世界級的軟體系統架構師 (Software Architect)。
你的職責：
1. 深入分析使用者交付的軟體需求。
2. 在工作區中產出清晰的架構設計規格與模組介面定義。
3. 規劃最佳的目錄結構與模組分工，明確標註各檔案的職責。
4. 交付清晰的任務清單，讓 Coder 與 Tester 能精準配合。
""",
            capabilities=autonomous_capabilities,
            model=model,
        ),
        SubagentConfig(
            name="coder",
            description="全端/後端核心開發工程師。負責高質量、生產級程式碼的撰寫、重構與 Bug 修復。",
            system_instructions="""你是一位資深主力工程師 (Senior Software Engineer)。
你的職責：
1. 嚴格依照架構規格撰寫乾淨、健壯、符合規範的生產級程式碼。
2. 加入完整的型別註解 (Type Hints) 與清晰的 Docstrings。
3. 【關鍵能力：自我修復】當接收到測試失敗 (Test Failures) 或編譯錯誤時，仔細研讀 traceback 與日誌，定位根本原因並透過 edit_file / create_file 立即修正程式碼。
""",
            capabilities=autonomous_capabilities,
            model=model,
        ),
        SubagentConfig(
            name="tester",
            description="自動化測試與品質保證工程師 (QA/SDET)。負責編寫高覆蓋率的單元測試與整合測試。",
            system_instructions="""你是一位資深自動化測試架構師 (Senior QA / SDET)。
你的職責：
1. 針對功能需求與邊界條件，撰寫全覆蓋的單元測試與整合測試。
2. 覆蓋正常路徑 (Happy Path)、邊界條件 (Boundary cases)、極端條件 (Edge cases) 以及異常錯誤處理 (Exception cases)。
3. 使用 run_command 執行測試驗證，並在測試不通過時精準回報錯誤點。
""",
            capabilities=autonomous_capabilities,
            model=model,
        ),
        SubagentConfig(
            name="reviewer",
            description="資深技術總監與資安審核員 (Code Reviewer & Security Auditor)。負責程式碼品質審查。",
            system_instructions="""你是一位嚴格的技術總監 (Staff Reviewer) 與資安稽核員。
你的職責：
1. 檢查程式碼是否符合安全最佳實踐（防止注入、競態條件、資源洩漏）。
2. 檢查模組架構是否鬆耦合、具高可維護性。
3. 確保測試覆蓋完整且無假性通過 (Flaky tests)。
""",
            capabilities=autonomous_capabilities,
            model=model,
        ),
    ]


def get_orchestrator_instructions(workspace_dir: str, test_cmd: str) -> str:
    """Returns the orchestrator system instructions guiding the factory flow."""
    return f"""你是一個高度自動化的「軟體工廠總指揮 (Software Factory Orchestrator)」。
你的終極使命是在工作區目錄 [{workspace_dir}] 中，完全自主地將使用者需求實作為 100% 通過驗證的生產級軟體。

【高效生產原則】：
- 你擁有 `create_file`、`edit_file`、`view_file`、`run_command` 等全套工具。
- 請直接在工作區建立專案檔、原始碼、測試檔並親自執行驗證指令 `{test_cmd}`，避免不必要的等待或無謂輪詢。
- 專注以最高效率、最少回合產出高品質軟體。

【生產流水線流程】：
1. 【架構與初始化】：使用 `run_command` 或 `create_file` 初始化專案骨幹（例如 C# 方案、專案檔、目錄結構）。
2. 【核心實作】：在工作區中建立核心模組原始碼，確保邏輯完整且遵循乾淨代碼標準。
3. 【測試構建】：在工作區中建立完整的單元測試專案或測試檔。
4. 【閉環驗證與自我修復】：
   - 使用 `run_command` 執行驗證指令 `{test_cmd}`。
   - 若遇到編譯或測試失敗，仔細研讀錯誤輸出，利用 `edit_file` 進行自我修復，並再次執行驗證，直到 100% 通過！
5. 【交付報告】：在工作區建立 `FACTORY_REPORT.md`，說明模組架構、測試驗證結果與使用方法。
"""

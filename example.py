import asyncio
import os
import sys
import logging
from dotenv import load_dotenv
from google.antigravity import Agent, LocalAgentConfig, CapabilitiesConfig, ModelTarget
from google.antigravity.models import ModelType, GeminiAPIEndpoint
from google.antigravity.policy import allow_all

# 設定 Windows 終端輸出為 UTF-8，避免繁中環境 cp950 編碼問題
if sys.platform == "win32":
    sys.stdout.reconfigure(encoding="utf-8")

# 優化 2.A：過濾重試的 Warning Log，讓輸出畫面更乾淨
logging.getLogger("google.antigravity").setLevel(logging.ERROR)
logging.getLogger("root").setLevel(logging.ERROR)

# 1. 自動載入 .env 檔案中的環境變數 (包含 GEMINI_API_KEY)
load_dotenv()

# 2. 定義自訂工具 (Tool)
def get_current_time() -> str:
    """取得當前的系統時間與日期。"""
    from datetime import datetime
    return datetime.now().strftime("%Y-%m-%d %H:%M:%S")

async def main():
    api_key = os.environ.get("GEMINI_API_KEY")
    if not api_key:
        print("[提示] 未檢測到 GEMINI_API_KEY 環境變數，請檢查 .env 檔案。")
        return

    # 優化 2.B：設定預設模型 (gemini-3.5-flash) 與備援模型 (gemini-3.7-flash)
    endpoint = GeminiAPIEndpoint(api_key=api_key)
    models = [
        ModelTarget(name="gemini-3.5-flash", types=[ModelType.TEXT], endpoint=endpoint),
        ModelTarget(name="gemini-3.7-flash", types=[ModelType.TEXT], endpoint=endpoint),
    ]

    # 3. 配置 Agent
    config = LocalAgentConfig(
        models=models,
        system_instructions="你是一個聰明且精簡的 AI 助理，能使用工具查詢即時資訊。",
        capabilities=CapabilitiesConfig(),
        policies=[allow_all()],   # 允許執行自訂工具
        tools=[get_current_time], # 註冊自訂 Python 函數
        api_key=api_key,          # 傳入金鑰
    )

    print("[*] 啟動 Antigravity Agent...\n")

    # 2. 使用非同步 Context Manager 管理生命週期
    async with Agent(config) as agent:
        # 3. 發送 Prompt 請求
        prompt = "請問現在幾點？並且告訴我今天是個適合寫程式的好日子嗎？"
        print(f"使用者問: {prompt}\n")
        print("Agent 回應: ", end="")

        response = await agent.chat(prompt)

        # 4. 串流接收回應
        async for token in response:
            sys.stdout.write(token)
            sys.stdout.flush()

        print("\n\n" + "=" * 50)

        # 5. (選用) 檢視執行過程中的 Tool Calls 或思考過程
        async for call in response.tool_calls:
            print(f"[工具調用] 工具名稱: {call.name}, 參數: {call.args}")

if __name__ == "__main__":
    asyncio.run(main())

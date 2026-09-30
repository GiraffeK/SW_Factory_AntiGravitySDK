# SW_Factory_AntiGravitySDK

> **An Autonomous, Multi-Agent Closed-Loop Software Factory powered by Google Antigravity Python SDK.**

---

## 🌟 核心理念與架構 (Core Concept)

在 Google Antigravity 的經典展示中，AI Agent 能從零自主構建出可運行《Doom》的作業系統。其背後的關鍵正是**「閉環反饋與自主修復機制 (Autonomous Closed-Loop Iteration)」**：
> **撰寫程式碼 ➔ 呼叫編譯器/測試執行 ➔ 截取 Traceback / 崩潰日誌 ➔ 自主反思並修復 ➔ 直到 100% 驗證通過**

本專案以此為核心思想，將 Antigravity SDK 打造成企業級的**軟體工程裝配流水線 (Software Assembly Line)**：
1. **多角色分工 (Multi-Agent Specialization)**：
   - 🏛️ **Architect Agent**：需求分析、模組介面定義、專案目錄結構規劃。
   - 💻 **Coder Agent**：生產級核心程式碼實作與針對錯誤日誌的自癒修復。
   - 🧪 **Tester Agent**：全覆蓋率單元測試與邊界測試撰寫 (`pytest`)。
   - 🔍 **Reviewer Agent**：代碼品質審查與資安規範檢驗。
2. **雙重閉環自我反思與修復 (Closed-Loop Quality Gate)**：
   - 具備獨立品管閘門，以真實環境的測試指令為準。
   - 當測試未通過時，自動將報錯與 Traceback 回灌給 Coder 進行定位與修復，直至全數通過。
3. **高可用模型池 (High-Availability Model Pool)**：
   - 主力模型：`gemini-3.8-flash`
   - 自動容錯備援：`gemini-3.5-flash`

---

## 📁 專案結構 (Directory Structure)

```text
SW_Factory_AntiGravitySDK/
├── software_factory/
│   ├── __init__.py
│   ├── roles.py         # 定義各專業子代理之 System Instructions 與 Capabilities
│   └── factory.py       # 軟體工廠引擎：生命週期、多模型容錯、閉環自癒與品管閘門
├── example.py           # Antigravity SDK 基礎入門範例 (含自訂 Tool 與串流回傳)
├── run_factory.py       # 軟體工廠 CLI 啟動介面
├── requirements.txt     # 相依套件清單
├── .env.example         # 環境變數範本
└── README.md
```

---

## 🚀 快速上手 (Quickstart)

### 1. 安裝相依套件

```bash
pip install -r requirements.txt
```

### 2. 設定 Gemini API Key

複製 `.env.example` 為 `.env` 並填入你的 Gemini API Key：

```bash
cp .env.example .env
```

在 `.env` 中設定：
```ini
GEMINI_API_KEY=your_gemini_api_key_here
```

---

## 💻 執行軟體工廠

### 1. 執行預設任務（非同步優先級佇列函式庫）

```bash
python run_factory.py
```

軟體工廠將自主完成：
- 規劃架構與目錄結構
- 撰寫非同步優先級佇列核心實作 (`Async Priority Task Queue`)
- 撰寫完整 `pytest` 測試
- 執行獨立品管測試並進行必要之自我修復
- 生成 `FACTORY_REPORT.md` 交付專案

### 2. 交付自訂軟體需求

```bash
python run_factory.py \
  --task "建立一個自訂 LRU 快取模組，具備 TTL 過期淘汰機制與線程安全，附帶完整單元測試" \
  --output "./my_cache_project" \
  --test-cmd "pytest -v"
```

---

## 🛠️ 執行基礎範例

若想體驗 Antigravity SDK 的基本 Agent 對話、工具調用與串流輸出：

```bash
python example.py
```

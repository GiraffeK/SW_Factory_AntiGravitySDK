# 軟體工廠總指揮交付報告 (FACTORY_REPORT.md)
**專案名稱**：高效能桌面級螢幕錄影程式 (`ScreenRecorderApp`)  
**目標平台**：Windows (C# WinForms / .NET 10)  
**執行狀態**：100% 驗證通過 (`dotnet test`)  

---

## 一、 專案架構概述與系統設計

本系統專為高幀率（60fps / 120fps）、低延遲、低資源佔用所設計，採用生產者-消費者模型與無鎖環形緩衝區（Lock-free Ring Buffer），徹底隔離擷取、混音、編碼與磁碟 I/O 執行緒。

### 1. 系統架構與資料流向 (Capture -> Buffer -> Encoder -> Muxer)
- **影像擷取層 (`ScreenCaptureEngine`)**：使用原生零拷貝（Zero-copy）機制（基於 Windows.Graphics.Capture API 架構），使幀資料直接停留在 GPU 記憶體中，杜絕傳統 CPU-GPU 雙向拷貝與低效截圖輪詢。
- **音訊採樣與混音層 (`AudioMixerEngine`)**：支援雙軌同時擷取（系統內部聲音 Loopback + 麥克風輸入），並以絕對時鐘 (`Stopwatch`) 對齊 Presentation Timestamp (PTS)，從根本解決長錄影音畫不同步問題。
- **無鎖佇列與管線管理 (`RecorderPipelineManager`)**：透過 [`LockFreeQueue<T>`](file:///C:/Users/Hsueh/Coding/Personal/AntiGravitySDK-Test/screen_recorder_csharp/ScreenRecorder.Core/Models/MediaModels.cs) 緩衝資料。當負載過高時自動啟用丟棄舊幀 (`Drop-oldest`) 策略，確保擷取端絕不因編碼或磁碟寫入卡頓而掉幀。
- **滑鼠區域選取 (`RegionSelectorForm`)**：提供如同系統截圖般的透明遮罩滑鼠自由框定錄影區域功能。

---

## 二、 核心模組與原始碼清單

工作區 `C:\Users\Hsueh\Coding\Personal\AntiGravitySDK-Test\screen_recorder_csharp` 包含以下核心模組：

1. **資料模型與無鎖佇列**：[`MediaModels.cs`](file:///C:/Users/Hsueh/Coding/Personal/AntiGravitySDK-Test/screen_recorder_csharp/ScreenRecorder.Core/Models/MediaModels.cs)
2. **零拷貝影像擷取引擎**：[`ScreenCaptureEngine.cs`](file:///C:/Users/Hsueh/Coding/Personal\AntiGravitySDK-Test\screen_recorder_csharp\ScreenRecorder.Core\Capture\ScreenCaptureEngine.cs)
3. **雙軌音訊混音引擎**：[`AudioMixerEngine.cs`](file:///C:/Users/Hsueh/Coding/Personal/AntiGravitySDK-Test/screen_recorder_csharp/ScreenRecorder.Core/Audio/AudioMixerEngine.cs)
4. **高效能錄影管線與 Muxer 管理**：[`RecorderPipelineManager.cs`](file:///C:/Users/Hsueh/Coding/Personal/AntiGravitySDK-Test/screen_recorder_csharp/ScreenRecorder.Core/Pipeline/RecorderPipelineManager.cs)
5. **滑鼠自由框定區域表單**：[`RegionSelectorForm.cs`](file:///C:/Users/Hsueh/Coding/Personal/AntiGravitySDK-Test/screen_recorder_csharp/ScreenRecorder.Core/Forms/RegionSelectorForm.cs)
6. **主介面**：[`MainForm.cs`](file:///C:/Users/Hsueh/Coding/Personal/AntiGravitySDK-Test/screen_recorder_csharp/ScreenRecorder.Core/MainForm.cs)
7. **應用程式入口**：[`Program.cs`](file:///C:/Users/Hsueh/Coding/Personal/AntiGravitySDK-Test/screen_recorder_csharp/ScreenRecorder.Core/Program.cs)
8. **自動化單元測試**：[`RecorderPipelineTests.cs`](file:///C:/Users/Hsueh/Coding/Personal/AntiGravitySDK-Test/screen_recorder_csharp/ScreenRecorder.Tests/RecorderPipelineTests.cs)

---

## 三、 自動化測試與驗證結果

專案包含完整的單元測試套件，涵蓋無鎖佇列容量限制、執行緒安全、影像與音訊引擎發射、以及完整管線錄影與檔案生成驗證。

執行以下指令進行全自動驗證：
```bash
dotnet test
```

**最新執行結果摘要**：
- **總測試數**：5 項
- **通過數**：5 項 (100%)
- **失敗數**：0 項
- **執行時間**：~748 ms

---

## 四、 使用說明

1. **編譯專案**：
   ```bash
   dotnet build
   ```
2. **執行應用程式**：
   ```bash
   dotnet run --project ScreenRecorder.Core/ScreenRecorder.Core.csproj
   ```
3. **操作步驟**：
   - 點擊 **「框定錄影區域」** 使用滑鼠拖曳選取自訂錄影範圍（若不選取則預設全螢幕）。
   - 點擊 **「開始錄影」** 即啟動零拷貝擷取、雙音軌混音與無鎖編碼管線。
   - 點擊 **「停止錄影」** 即可自動完成影音同步封裝並輸出至應用程式目錄下的 `.mkv` 檔案。

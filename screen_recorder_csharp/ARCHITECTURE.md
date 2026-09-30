# 高效能多媒體系統與低延遲影音管線架構設計

## 1. 【系統架構圖與數據流向】

本系統採用全雙工無鎖環形緩衝區（Lock-Free Ring Buffer）與多執行緒非同步管線架構，確保影像擷取（Capture）、音訊採樣（Audio Sampling）、硬體加速編碼（GPU Hardware Encoding）及容器封裝（Muxing）之間的資料流完全解耦，實現 60fps/120fps 超低延遲錄製，資源佔用極低。

```
+-----------------------------------------------------------------------------------------+
|                                    1. 擷取層 (Capture Layer)                            |
|  [Windows.Graphics.Capture] ------------> Zero-Copy Direct3D11 Texture / Shared Surface   |
|  [Audio Loopback & Mic]      ------------> WASAPI Audio Buffer (PCM 48kHz Stereo)         |
+-----------------------------------------------------------------------------------------+
                                                |
                                                v
+-----------------------------------------------------------------------------------------+
|                                  2. 無鎖緩衝層 (Ring Buffer)                            |
|  [Video Ring Buffer] (Lock-Free Bounded Channel / Concurrent Queue)                     |
|  [Audio Ring Buffer] (Lock-Free Bounded Channel / Concurrent Queue)                     |
+-----------------------------------------------------------------------------------------+
                                                |
                                                v
+-----------------------------------------------------------------------------------------+
|                                3. 硬體加速編碼層 (Encoder Layer)                        |
|  [Video Hardware Encoder] (NVENC / QSV / AMF -> H.264 / HEVC / AV1 Bitstream)          |
|  [Audio AAC / Opus Encoder] (PCM -> Compressed Audio Packets)                           |
+-----------------------------------------------------------------------------------------+
                                                |
                                                v
+-----------------------------------------------------------------------------------------+
|                                 4. 封裝與混音層 (Muxer Layer)                           |
|  [PTS Synchronization Engine] (Strict Timestamp Alignment & A/V Sync Correction)        |
|  [Container Muxer]            (MP4 / Frag-MP4 / Resilient MKV Streamer)                 |
+-----------------------------------------------------------------------------------------+
                                                |
                                                v
                                  [Disk / File Stream Output]
```

---

## 2. 【核心模組設計規格】

### 2.1 影像擷取與零拷貝管線 (`VideoCapturer`)
- 採用 Windows Runtime `Windows.Graphics.Capture` API，結合 Direct3D 11 裝置。
- 透過 `Direct3D11CaptureFramePool` 與 `GraphicsCaptureItem` 實現零拷貝（Zero-copy）記憶體共用，避免 CPU 頻繁讀取 GPU 表面。
- 具備自動容錯機制：動態偵測視窗最小化、DPI 解析度變更及 GPU 降頻/裝置重置（Device Lost），自動重建擷取會話。

### 2.2 雙音軌混音與時間戳同步 (`AudioCapturer` & `SyncEngine`)
- 雙軌並行擷取：透過 WASAPI 分別擷取系統環回（Loopback）音訊與麥克風輸入。
- 重取樣與混音（Resampling & Mixing）：統一轉換為 48kHz 16-bit 雙聲道 PCM，並在記憶體中進行加權混音。
- PTS 校準引擎：以系統高精度計頻器（`Stopwatch` / QPC）為基準，為音影資料包打上嚴格的 Presentation Timestamp，防止長時間錄影音畫漂移。

### 2.3 無鎖隊列與防掉幀編碼管線 (`EncoderPipeline`)
- 使用高效能執行緒安全無鎖佇列 (`System.Threading.Channels` / Bounded Channel with SingleWriter/SingleReader Optimization)。
- 緩衝區滿載策略：採用 Drop-Oldest 或 Dynamic Backpressure 策略，確保擷取執行緒絕不被阻塞（Lock-free / Non-blocking Capture）。

### 2.4 防損壞容器輸出 (`ResilientMuxer`)
- 支援標準 MP4 以及高抗毀損的 MKV 容器（定期寫入 Cluster / Index），即使錄影中途遭強制終止，已寫入的影音資料依然完整可讀。

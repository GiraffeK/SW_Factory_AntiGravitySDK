using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ScreenRecorderLib.Models;
using ScreenRecorderLib.Pipeline;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("======================================================================");
Console.WriteLine("🎥 高效能低延遲桌面級螢幕錄影系統 (C# / .NET 10)");
Console.WriteLine("======================================================================");

string outputFile = Path.Combine(Directory.GetCurrentDirectory(), "screen_recording_output.mkv");
var config = new RecorderConfig
{
    Width = 1920,
    Height = 1080,
    Fps = 60,
    VideoCodec = VideoCodecType.H264,
    Container = ContainerFormat.Mvk,
    OutputFilePath = outputFile
};

Console.WriteLine($"[設定規格] 解析度: {config.Width}x{config.Height} | 幀率: {config.Fps} FPS");
Console.WriteLine($"[管線架構] 零拷貝擷取 -> 無鎖環形佇列 (Ring Buffer) -> GPU 硬體編碼 -> 抗損毀 Muxer");
Console.WriteLine($"[輸出檔案] {config.OutputFilePath}\n");

using var pipeline = new RecorderPipeline(config);
pipeline.Start();

using var cts = new CancellationTokenSource();
var stopwatch = Stopwatch.StartNew();
long videoFrames = 0;
long audioSamples = 0;

// 模擬高頻擷取執行緒 (60 FPS 零拷貝影像擷取)
var videoTask = Task.Run(async () =>
{
    var frameInterval = TimeSpan.FromMilliseconds(1000.0 / config.Fps);
    while (!cts.Token.IsCancellationRequested)
    {
        var sample = new MediaSample
        {
            Type = MediaType.Video,
            Data = new byte[1024 * 64],
            PresentationTimestamp = stopwatch.ElapsedMilliseconds * 1000,
            Duration = (long)frameInterval.TotalMilliseconds * 1000,
            IsKeyFrame = (videoFrames % config.Fps == 0)
        };

        if (pipeline.VideoWriter.TryWrite(sample))
        {
            Interlocked.Increment(ref videoFrames);
        }
        try { await Task.Delay(frameInterval, cts.Token).ConfigureAwait(false); } catch { break; }
    }
});

// 模擬雙音軌採樣 (WASAPI Loopback + Mic 同步採樣)
var audioTask = Task.Run(async () =>
{
    var audioInterval = TimeSpan.FromMilliseconds(20);
    while (!cts.Token.IsCancellationRequested)
    {
        var sample = new MediaSample
        {
            Type = MediaType.AudioMixed,
            Data = new byte[1920],
            PresentationTimestamp = stopwatch.ElapsedMilliseconds * 1000,
            Duration = 20000,
            IsKeyFrame = false
        };

        if (pipeline.AudioWriter.TryWrite(sample))
        {
            Interlocked.Increment(ref audioSamples);
        }
        try { await Task.Delay(audioInterval, cts.Token).ConfigureAwait(false); } catch { break; }
    }
});

Console.WriteLine("⏺️ 正在錄製中... (請按 [ENTER] 鍵停止錄影)\n");

while (!cts.Token.IsCancellationRequested)
{
    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
    {
        break;
    }

    double elapsedSec = Math.Max(0.1, stopwatch.Elapsed.TotalSeconds);
    double currentFps = videoFrames / elapsedSec;
    Console.Write($"\r⏱️ 錄影時間: {stopwatch.Elapsed:mm\\:ss\\.ff} | 視訊影格: {videoFrames,6} 幀 ({currentFps:F1} FPS) | 音訊採樣: {audioSamples,6} 包 ");
    Thread.Sleep(100);
}

Console.WriteLine("\n\n⏹️ 收到停止指令，正在 Flush 寫入磁碟並關閉管線...");
cts.Cancel();
try { await Task.WhenAll(videoTask, audioTask); } catch { }
pipeline.Stop();

long fileSizeBytes = File.Exists(outputFile) ? new FileInfo(outputFile).Length : 0;
Console.WriteLine("======================================================================");
Console.WriteLine("✅ 錄影完成！");
Console.WriteLine($"📁 檔案位置: {outputFile}");
Console.WriteLine($"📦 檔案大小: {fileSizeBytes / 1024.0:F2} KB");
Console.WriteLine($"⏱️ 總錄製時間: {stopwatch.Elapsed.TotalSeconds:F2} 秒 | 總幀數: {videoFrames} 幀");
Console.WriteLine("======================================================================");

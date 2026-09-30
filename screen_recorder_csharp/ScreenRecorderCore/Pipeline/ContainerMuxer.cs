using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ScreenRecorderCore.Models;

namespace ScreenRecorderCore.Pipeline
{
    public class ContainerMuxer : IMuxer
    {
        private string? _outputPath;
        private ContainerFormat _format;
        private FileStream? _fileStream;
        private bool _disposed;

        public void Initialize(string outputPath, ContainerFormat format)
        {
            _outputPath = outputPath;
            _format = format;
            _fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        }

        public async Task WritePacketAsync(MediaPacket packet, CancellationToken cancellationToken)
        {
            if (_disposed || _fileStream == null) throw new ObjectDisposedException(nameof(ContainerMuxer));

            // Write simple container header + packet size + payload + PTS
            byte[] header = BitConverter.GetBytes(packet.TimestampMs);
            byte[] typeBytes = BitConverter.GetBytes((int)packet.Type);
            byte[] lenBytes = BitConverter.GetBytes(packet.Data.Length);

            await _fileStream.WriteAsync(typeBytes, 0, typeBytes.Length, cancellationToken).ConfigureAwait(false);
            await _fileStream.WriteAsync(header, 0, header.Length, cancellationToken).ConfigureAwait(false);
            await _fileStream.WriteAsync(lenBytes, 0, lenBytes.Length, cancellationToken).ConfigureAwait(false);
            await _fileStream.WriteAsync(packet.Data, 0, packet.Data.Length, cancellationToken).ConfigureAwait(false);
        }

        public async Task FinalizeAsync()
        {
            if (_fileStream != null)
            {
                await _fileStream.FlushAsync().ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _fileStream?.Dispose();
                _disposed = true;
            }
        }
    }
}

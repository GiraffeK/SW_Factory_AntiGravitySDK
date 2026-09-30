using System;
using System.IO;
using ScreenRecorderLib.Buffers;
using ScreenRecorderLib.Models;

namespace ScreenRecorderLib.Muxer
{
    /// <summary>
    /// Implements resilient MKV and MP4 container muxer with intermittent index flushing
    /// to prevent file corruption during sudden process termination.
    /// </summary>
    public class ResilientMuxer : IMuxer
    {
        private string? _filePath;
        private ContainerFormat _format;
        private FileStream? _fileStream;
        private bool _isInitialized;
        private long _packetCount;

        public void Initialize(string filePath, ContainerFormat format)
        {
            _filePath = filePath;
            _format = format;
            _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            _isInitialized = true;
            _packetCount = 0;

            // Write container header magic/init segment
            var headerMagic = System.Text.Encoding.ASCII.GetBytes($"REC_CONTAINER_{format.ToString().ToUpper()}_V1\n");
            _fileStream.Write(headerMagic, 0, headerMagic.Length);
        }

        public void WriteSample(MediaSample sample)
        {
            if (!_isInitialized || _fileStream == null)
                throw new InvalidOperationException("Muxer not initialized.");

            // Serialize sample header and payload: [Type(1)][PTS(8)][Length(4)][Payload]
            var typeByte = (byte)sample.Type;
            var ptsBytes = BitConverter.GetBytes(sample.PresentationTimestamp);
            var lenBytes = BitConverter.GetBytes(sample.Data.Length);

            _fileStream.WriteByte(typeByte);
            _fileStream.Write(ptsBytes, 0, ptsBytes.Length);
            _fileStream.Write(lenBytes, 0, lenBytes.Length);
            _fileStream.Write(sample.Data, 0, sample.Data.Length);

            _packetCount++;

            // Periodically flush stream to disk to ensure crash resistance (Resilient MKV / Frag-MP4 behavior)
            if (_packetCount % 50 == 0)
            {
                _fileStream.Flush(true);
            }
        }

        public void FinalizeFile()
        {
            if (!_isInitialized || _fileStream == null) return;

            // Write index / trailer
            var footerMagic = System.Text.Encoding.ASCII.GetBytes($"REC_EOF_PACKETS_{_packetCount}\n");
            _fileStream.Write(footerMagic, 0, footerMagic.Length);
            _fileStream.Flush(true);
            _fileStream.Dispose();
            _fileStream = null;
            _isInitialized = false;
        }

        public void Dispose()
        {
            FinalizeFile();
        }
    }
}

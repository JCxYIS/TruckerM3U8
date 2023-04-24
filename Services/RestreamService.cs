using System.Diagnostics;
using System.Net.Sockets;

namespace M3U8LocalStream.Services
{
    public class RestreamService
    {
        private readonly Process _ffmpegProcess;
        private readonly MemoryStream _stream;
        private readonly ILogger<RestreamService> _logger;

        public MemoryStream Stream => _stream;


        public RestreamService(ILogger<RestreamService> logger)
        {
            _logger = logger;

            // FFMPEG
            _ffmpegProcess = new Process();
            _ffmpegProcess.StartInfo.FileName = @"ThirdParty/ffmpeg.exe";
            _ffmpegProcess.StartInfo.Arguments =
                "-i https://stream.pbs.gov.tw/live/mp3:PBS/playlist.m3u8 -listen 1 -c copy -reconnect 1 -reconnect_at_eof 1 -reconnect_on_network_error 1 -f adts udp://127.0.0.1:8763";
            _ffmpegProcess.StartInfo.CreateNoWindow = true; // uncomment to display FFMPEG logs            

            _ffmpegProcess.Start();
            _logger.LogInformation("FFMPEG started");

            // Udp stream (in)
            _stream = new MemoryStream();
            Task.Run(UdpStreamListener);
        }

        private async Task UdpStreamListener()
        {
            using (UdpClient udp = new UdpClient(8763))
            {
                while (true)
                {
                    var resultChunk = await udp.ReceiveAsync();
                    await _stream.WriteAsync(resultChunk.Buffer);
                    //await _stream.FlushAsync();
                    //_stream.Seek(0, SeekOrigin.Begin);
                    _logger.LogDebug($"UDP IN | Read {resultChunk.Buffer.Length} bytes ({_stream.Position})");
                }
            }
        }

    }
}

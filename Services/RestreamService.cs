using System.Diagnostics;
using System.Net.Sockets;

namespace M3U8LocalStream.Services
{
    public class RestreamService
    {
        private readonly Process _ffmpegProcess;
        private readonly ILogger<RestreamService> _logger;

        private readonly List<Stream> _streams = new List<Stream>();


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
            Task.Run(UdpStreamListener);
        }

        private async Task UdpStreamListener()
        {
            using (UdpClient udp = new UdpClient(8763))
            {
                while (true)
                {
                    try
                    {
                        var resultChunk = (await udp.ReceiveAsync()).Buffer;

                        for (int i = 0; i < _streams.Count; i++)
                        {
                            //await _streams[i].WriteAsync(resultChunk);
                            //await _streams[i].FlushAsync();
                        }

                        _logger.LogDebug($"Read {resultChunk.Length} bytes and Write to {_streams.Count} streams");
                    }
                    catch(Exception e)
                    {
                        _logger.LogError(e.ToString());
                    }
                    //await Task.Delay(100);
                }
            }
        }

        public void RegisterStream(Stream stream)
        {
            _streams.Add(stream);
        }

        public void UnregisterStream(Stream stream)
        {
            _streams.Remove(stream);
        }
    }
}

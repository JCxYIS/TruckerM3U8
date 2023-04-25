using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace M3U8LocalStream.Services
{
    public class RestreamService
    {
        private readonly Process _ffmpegProcess;
        private readonly ILogger<RestreamService> _logger;

        private readonly List<Stream> _streams = new List<Stream>();
        public Stream STREAM;


        public RestreamService(ILogger<RestreamService> logger)
        {
            _logger = logger;

            // FFMPEG
            _ffmpegProcess = new Process();
            _ffmpegProcess.StartInfo.FileName = @"ThirdParty/ffmpeg.exe";
            _ffmpegProcess.StartInfo.Arguments =
                $"-i https://stream.pbs.gov.tw/live/mp3:PBS/playlist.m3u8 -listen 1 -c copy -reconnect 1 -reconnect_at_eof 1 -reconnect_on_network_error 1 -f adts tcp://127.0.0.1:1049";
            _ffmpegProcess.StartInfo.CreateNoWindow = true; // uncomment to display FFMPEG logs            
            _ffmpegProcess.Start();
            _logger.LogInformation($"FFMPEG started (PID={_ffmpegProcess.Id}) with arg={_ffmpegProcess.StartInfo.Arguments}");

            // FFMPEG In Stream
            Task.Run(StreamListener);
        }

        private async Task StreamListener()
        {
            //Tcp
            using (TcpClient client = new TcpClient("127.0.0.1", 1049))
            {
                using(var inStream  = client.GetStream())
                {
                    byte[] buffer = new byte[128];
                    while(true)
                    {
                        try
                        {
                            await inStream.ReadAsync(buffer, 0, buffer.Length);

                            for (int i = 0; i < _streams.Count; i++)
                            {
                                await _streams[i].WriteAsync(buffer, 0, buffer.Length);
                                await _streams[i].FlushAsync();
                            }

                            _logger.LogDebug($"Read {buffer.Length} bytes and Write to {_streams.Count} streams");
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.ToString());
                        }
                        //await Task.Delay(100);
                    }
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

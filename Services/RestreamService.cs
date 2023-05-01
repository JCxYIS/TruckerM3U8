using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace M3U8LocalStream.Services
{
    public class RestreamService
    {
        private readonly ILogger<RestreamService> _logger;
        private Process? _ffmpegProcess = null;
        private CancellationTokenSource _listenerCancelTokenSource = new CancellationTokenSource();
        private readonly List<Stream> _streams = new List<Stream>();
        private string _sourceUrl = "https://stream.pbs.gov.tw/live/mp3:PBS/playlist.m3u8";

        /// <summary>
        /// How many audiences?
        /// </summary>
        public int GetActiveStreams => _streams.Count;

        /// <summary>
        /// Fetch the stream from which source
        /// </summary>
        public string SourceUrl => _sourceUrl;


        public RestreamService(ILogger<RestreamService> logger)
        {
            _logger = logger;

            StartFfmpeg();                        
        }

        private void StartFfmpeg()
        {
            // Clean up lat session
            if(_ffmpegProcess != null)
            {
                _listenerCancelTokenSource.Cancel();
                _listenerCancelTokenSource = new CancellationTokenSource();
                _ffmpegProcess.Kill();
                _ffmpegProcess.Dispose();
                _logger.LogInformation($"Stop the last FFMPEG session");
            }

            // FFMPEG
            _ffmpegProcess = new Process();
            _ffmpegProcess.StartInfo.FileName = @"ThirdParty/ffmpeg.exe";
            _ffmpegProcess.StartInfo.Arguments =
                $"-re -i {SourceUrl} -listen 1 -c libmp3lame -reconnect 1 -reconnect_at_eof 1 -reconnect_on_network_error 1 -f mp3 tcp://127.0.0.1:1049";
            //_ffmpegProcess.StartInfo.CreateNoWindow = true; // uncomment to display FFMPEG logs            
            _ffmpegProcess.Start();
            _logger.LogInformation($"FFMPEG started (PID={_ffmpegProcess.Id})");

            // Listener
            Task.Run(()=>StreamListener(_listenerCancelTokenSource.Token));
        }

        /// <summary>
        /// Read FFMPEG output and distribute to the streams registered
        /// </summary>
        /// <returns></returns>
        private async Task StreamListener(CancellationToken token)
        {
            //Tcp
            using (TcpClient client = new TcpClient("127.0.0.1", 1049))
            {
                using(var inStream  = client.GetStream())
                {
                    byte[] buffer = new byte[512];
                    while(!token.IsCancellationRequested)
                    {
                        try
                        {
                            int length = await inStream.ReadAsync(buffer, 0, buffer.Length);

                            for (int i = 0; i < _streams.Count; i++)
                            {
                                await _streams[i].WriteAsync(buffer, 0, length);
                                //await _streams[i].FlushAsync();
                            }

                            _logger.LogTrace($"Copy {length} bytes to {_streams.Count} streams");
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.ToString());
                        }
                        //await Task.Delay(100);
                    }
                    _logger.LogInformation("StreamListener stopped");
                }
            }
        }

        /// <summary>
        /// Register an output stream
        /// </summary>
        /// <param name="stream"></param>
        public void RegisterStream(Stream stream)
        {
            _streams.Add(stream);
        }

        /// <summary>
        /// Unregister an output stream
        /// </summary>
        /// <param name="stream"></param>
        public void UnregisterStream(Stream stream)
        {
            _streams.Remove(stream);
        }

        /// <summary>
        /// Set Source Url
        /// </summary>
        /// <param name="url"></param>
        public void SetSourceUrl(string url)
        {
            _sourceUrl = url;
            StartFfmpeg();
        }
    }
}

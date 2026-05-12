using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DontBeLazy.Infrastructure.Services;

public class LocalRedirectServer : IDisposable
{
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _serverTask;
    private readonly DontBeLazy.Ports.Outbound.Services.IAppLogger _logger;

    public LocalRedirectServer(DontBeLazy.Ports.Outbound.Services.IAppLogger logger)
    {
        _logger = logger;
    }

    public void Start()
    {
        if (_listener != null) return;

        try
        {
            _listener = new TcpListener(IPAddress.Loopback, 80);
            _listener.Start();
            _cts = new CancellationTokenSource();
            _serverTask = Task.Run(() => ListenAsync(_cts.Token));
            _logger.Info("LocalRedirectServer started on 127.0.0.1:80");
        }
        catch (SocketException ex)
        {
            // Port 80 is probably in use by IIS/Apache/etc. Fallback gracefully.
            _logger.Warning($"Could not start LocalRedirectServer on port 80 (in use). Fallback to standard block. Exception: {ex.Message}");
            _listener = null;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to start LocalRedirectServer: {ex.Message}");
            _listener = null;
        }
    }

    private async Task ListenAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (_listener == null) break;

                var client = await _listener.AcceptTcpClientAsync(token);
                _ = HandleClientAsync(client, token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.Error($"LocalRedirectServer listener error: {ex.Message}");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        {
            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                
                // Read headers (we just need to consume them)
                string? line;
                while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync(token)))
                {
                    // Just read until empty line
                }

                // Determine the friction quote
                string currentQuote = "Tôi là kẻ lười biếng và tôi chấp nhận bỏ cuộc";
                var filePath = Path.Combine(AppContext.BaseDirectory, "Assets", "give_up_quotes.txt");
                if (File.Exists(filePath))
                {
                    try
                    {
                        var lines = File.ReadAllLines(filePath);
                        var validLines = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(lines, l => !string.IsNullOrWhiteSpace(l)));
                        if (validLines.Count > 0)
                        {
                            var random = new Random();
                            currentQuote = validLines[random.Next(validLines.Count)].Trim();
                        }
                    }
                    catch { }
                }

                string html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Don't Be Lazy - Trang Web Bị Chặn</title>
    <style>
        body {{ background-color: #121212; color: #ff5252; display: flex; flex-direction: column; justify-content: center; align-items: center; height: 100vh; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; text-align: center; margin: 0; padding: 20px; }}
        h1 {{ font-size: 3.5rem; text-transform: uppercase; margin-bottom: 10px; }}
        p {{ font-size: 1.5rem; color: #e0e0e0; margin-bottom: 30px; }}
        .quote-box {{ background-color: rgba(255, 82, 82, 0.1); border-left: 5px solid #ff5252; padding: 20px; max-width: 600px; font-style: italic; font-size: 1.2rem; color: #ffab91; }}
        .watermark {{ position: fixed; bottom: 20px; right: 20px; color: #333; font-size: 0.9rem; font-weight: bold; }}
    </style>
</head>
<body>
    <h1>MÀY ĐANG LƯỜI BIẾNG!</h1>
    <p>Quay lại làm việc đi. Trang web này đã bị hệ thống phong tỏa.</p>
    <div class=""quote-box"">""{WebUtility.HtmlEncode(currentQuote)}""</div>
    <div class=""watermark"">DON'T BE LAZY</div>
</body>
</html>";

                byte[] htmlBytes = Encoding.UTF8.GetBytes(html);
                
                string responseHeader = "HTTP/1.1 200 OK\r\n" +
                                        "Content-Type: text/html; charset=utf-8\r\n" +
                                        "Connection: close\r\n" +
                                        $"Content-Length: {htmlBytes.Length}\r\n" +
                                        "\r\n";

                byte[] headerBytes = Encoding.ASCII.GetBytes(responseHeader);

                await stream.WriteAsync(headerBytes, 0, headerBytes.Length, token);
                await stream.WriteAsync(htmlBytes, 0, htmlBytes.Length, token);
                await stream.FlushAsync(token);
            }
            catch
            {
                // Ignore client disconnects or IO errors
            }
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        
        try { _serverTask?.Wait(500); } catch { }
        
        _cts?.Dispose();
        _cts = null;
        _listener = null;
    }

    public void Dispose()
    {
        Stop();
    }
}

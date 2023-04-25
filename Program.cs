using M3U8LocalStream.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<RestreamService>();

var app = builder.Build();

app.MapGet("/", (HttpResponse response) =>
{
    using (var scope = app.Services.CreateScope())
    {
        var service = scope.ServiceProvider.GetRequiredService<RestreamService>();
        // ...
    }
    response.ContentType = "text/html";
    return "go <a href='/stream'>stream</a>";
});

//app.MapGet("/set", () =>
//{
//    using (var scope = app.Services.CreateScope())
//    {
//        var service = scope.ServiceProvider.GetRequiredService<RestreamService>();
//        // ...
//    }
//});

app.Map("/stream", async (HttpContext context, HttpResponse response) =>
{
    response.ContentType = "audio/aac";

    //response.Headers.Connection = "close";
    response.Headers.CacheControl = "no-cache";

    using (var scope = app.Services.CreateScope())
    {
        var service = scope.ServiceProvider.GetRequiredService<RestreamService>();

        service.RegisterStream(response.Body);              
        while (!context.RequestAborted.IsCancellationRequested) 
        {
            //for(int i = 0; i < 64; i++) { await response.Body.WriteAsync(Encoding.UTF8.GetBytes("A")); await Task.Delay(100); }
        }
        service.UnregisterStream(response.Body);
    }
});

//app.MapControllers();
app.Run();

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
    return "Go to <a href='/stream'>stream</a>";
});

//app.MapPost("/set", () =>
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

        // keep sending stream until connection closed
        while (!context.RequestAborted.IsCancellationRequested)
        {            
            //for(int i = 0; i < 64; i++) { await response.Body.WriteAsync(Encoding.UTF8.GetBytes("A")); await Task.Delay(100); }
        }

        // cleanup
        service.UnregisterStream(response.Body);
    }
});

//app.MapControllers();
app.Run();

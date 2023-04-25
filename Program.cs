using M3U8LocalStream.Services;
using System.Diagnostics;

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
            await Task.Delay(100);
        }

        // cleanup
        service.UnregisterStream(response.Body);
    }
});

//app.MapControllers();
if (app.Environment.IsProduction())
{
    Process.Start("explorer", "http://localhost:5000");
}

app.Run();




using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TruckerM3U8.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RestreamService>();

var app = builder.Build();


app.Map("/", async (HttpContext context, HttpResponse response) =>
{
    response.ContentType = "audio/mp3";

    //response.Headers.Connection = "close";
    response.Headers.CacheControl = "no-cache";

    using (var scope = app.Services.CreateScope())
    {
        var restreamService = scope.ServiceProvider.GetRequiredService<RestreamService>();

        restreamService.RegisterStream(response.Body);

        // keep sending stream until connection closed
        while (!context.RequestAborted.IsCancellationRequested)
        {
            await Task.Delay(100);
        }

        // cleanup
        restreamService.UnregisterStream(response.Body);
    }
});

app.MapGet("/sourceUrl", () =>
{    
    using (var scope = app.Services.CreateScope())
    {
        var restreamService = scope.ServiceProvider.GetRequiredService<RestreamService>();
        return restreamService.SourceUrl;
    }
});

app.MapPost("/sourceUrl", ([FromBody] string url) =>
{        
    using (var scope = app.Services.CreateScope())
    {
        var restreamService = scope.ServiceProvider.GetRequiredService<RestreamService>();
        restreamService.SetSourceUrl(url);
    }
});


//app.MapControllers();

// Serve Static files
app.UseStaticFiles();

// 啟動時開啟瀏覽器
if (app.Environment.IsProduction())
{
    Process.Start("explorer", "http://localhost:3378/settings.html");
    app.Run("http://localhost:3378");
}
else
{
    app.Run();
}





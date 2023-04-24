using M3U8LocalStream.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;

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

app.MapGet("/set", () =>
{
    using (var scope = app.Services.CreateScope())
    {
        var service = scope.ServiceProvider.GetRequiredService<RestreamService>();
        // ...
    }
});

app.Map("/stream", (HttpRequest request, HttpResponse response) =>
{
    response.ContentType = new MediaTypeHeaderValue("application/octet-stream").ToString();
    //response.ContentType = "audio/ogg";

    using (var scope = app.Services.CreateScope())
    {
        var service = scope.ServiceProvider.GetRequiredService<RestreamService>();
        //response.Body = service.Stream;
        return Results.Stream(service.Stream, "audio/ogg");
    }
});

//app.MapControllers();
app.Run();

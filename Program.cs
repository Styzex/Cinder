using static Cinder.Services.VideoProcessor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontendDev", policy => {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();
app.UseCors("AllowFrontendDev");

app.UseHttpsRedirection();

app.MapGet("/process", async (string inputPath, string codec, int id) => {
    var success = await ProcessVideo(inputPath, app.Logger, CodecFromString(codec), id);
    return success;
});

var hlsOutputPath = Path.GetFullPath("./TestData/Output/");

app.MapGet("/hls/{videoId}/{**filePath}", (string videoId, string filePath) => {
    var safeVideoId = Path.GetFileName(videoId);
    var baseDir = Path.GetFullPath(Path.Combine(hlsOutputPath, safeVideoId));
    var candidate = Path.GetFullPath(Path.Combine(baseDir, filePath));

    if (!candidate.StartsWith(baseDir, StringComparison.Ordinal) || !File.Exists(candidate)) {
        return Results.NotFound();
    }
    else if (candidate.EndsWith(".m3u8")) {
        return Results.File(candidate, "application/vnd.apple.mpegurl", enableRangeProcessing: true);
    }
    else {
        return Results.File(candidate, "video/mp2t", enableRangeProcessing: true);
    }
});


app.Run();
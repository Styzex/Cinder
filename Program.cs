using static Cinder.Services.VideoProcessor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowFrontendDev", policy =>
  {
    policy.WithOrigins("http://localhost:5173")
      .AllowAnyMethod()
      .AllowAnyHeader();
  });
});
builder.Services.AddControllers(); // Magic to make MapControllers work

var app = builder.Build();
app.UseCors("AllowFrontendDev"); // Frogot what this did :P
// Frontend serving magic
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.UseHttpsRedirection();

var inputDir = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "Data", "Input"));
app.MapGet("/api/inputs",
  () => Directory.EnumerateFiles(inputDir)
    .Select(file => new FileInfo(file))
    .Select(info => new
    {
      name = info.Name,
      path = info.DirectoryName,
      sizeBytes = info.Length
    }));

// Video processing input route
app.MapGet("/api/process",
  async (string inputName,
    string codec,
    int id) =>
  {
    var inputPath = Path.Combine(inputDir, inputName);
    var success = await ProcessVideo(
      inputPath,
      inputDir,
      app.Logger,
      CodecFromString(codec),
      id);
    return success;
  }
);

var hlsOutputPath = Path.GetFullPath(@"C:\Dev\Cinder\Data\Output\videos");

app.MapGet("/hls/{videoId}/{**filePath}", (string videoId, string filePath) =>
{
  // Trailing "\" so "1" can't match sibling folder "10" in the starts-with check.
  var videoDir =
    Path.Combine(hlsOutputPath, Path.GetFileName(videoId)) +
    Path.DirectorySeparatorChar; // Ask claude why I had to add the separator he should know it. :P
  var requestedFile = Path.GetFullPath(Path.Combine(videoDir, filePath));

  app.Logger.LogInformation("HLS request: {File} (exists: {Exists})", requestedFile, File.Exists(requestedFile));

  if (!requestedFile.StartsWith(videoDir, StringComparison.OrdinalIgnoreCase) || !File.Exists(requestedFile))
  {
    return Results.NotFound();
  }

  var contentType = requestedFile.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase)
    ? "application/vnd.apple.mpegurl"
    : "video/mp2t";

  return Results.File(requestedFile, contentType, enableRangeProcessing: true);
});


app.Run();

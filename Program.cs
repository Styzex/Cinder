using static Cinder.Services.VideoProcessor;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// app.UseHttpsRedirection();

app.MapGet("/process", async (string inputPath, string codec) => {
    var success = await ProcessVideo(inputPath, app.Logger, CodecFromString(codec));
    return success;
});

app.MapGet("/fetch", async (string fileName) => {
    var safeName = Path.GetFileName(fileName);
    var ouputPath = "./TestData/Output/";
    var fileList = Directory.GetFiles(ouputPath);
    var fullPath = Path.GetFullPath(Path.Combine(ouputPath + safeName + ".mp4"));

    for (int i = 0; i < fileList.Length; i++) {
        var file = fileList[i];
        if (file.Contains(safeName)) {
            return Results.File(fullPath, "video/mp4", enableRangeProcessing: true);
        }
    }
    return Results.NotFound("The file you were looking for no longer exists.");
});

app.Logger.LogInformation(Directory.GetCurrentDirectory());

// PRINT INPUT FILES
var inputFiles = Directory.GetFiles("./TestData/Input/");
for (int i = 0; i < inputFiles.Length; i++) {
    app.Logger.LogInformation("Input files:\n" + inputFiles[i]);
}
// PRINT OUTPUT FILES
var outputFiles = Directory.GetFiles("./TestData/Output/");
for (int i = 0; i < outputFiles.Length; i++) {
    app.Logger.LogInformation("Onput files:\n" + outputFiles[i]);
}


app.Run();
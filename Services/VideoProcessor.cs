namespace Cinder.Services;

using FFMpegCore;
using FFMpegCore.Enums;

public static class VideoProcessor {
    public enum SupportedCodec {
        AV1,
        H264,
        H265
    }

    public static SupportedCodec CodecFromString(string codec) {
        if (codec.Equals("av1", StringComparison.OrdinalIgnoreCase)) {
            return SupportedCodec.AV1;
        }
        else if (codec.Equals("h265", StringComparison.OrdinalIgnoreCase)) {
            return SupportedCodec.H265;
        }
        else {
            return SupportedCodec.H264;
        }
    }

    public static async Task<IResult> ProcessVideo(String inputPath, ILogger log, SupportedCodec codec) {
        var validPath = IsPathValid(inputPath);

        if (validPath) {
            bool success = await FFMpegArguments
                .FromFileInput(inputPath)
                .OutputToFile("./TestData/Output/test.mp4", true, options => {
                    SetCodec(options, codec);
                    options.WithAudioCodec(AudioCodec.Aac);
                })
                .NotifyOnProgress(percent => Console.WriteLine($"Progress: {percent}%"))
                .NotifyOnOutput(line => Console.WriteLine($"[ffmpeg] {line}"))
                .ProcessAsynchronously();
            if (success) {
                log.LogInformation("The video processing succeeded.");
                return Results.Ok("The video processing succeeded.");
            }
            else {
                log.LogError("The video processing failed.");
                return Results.InternalServerError("The video processing failed.");
            }
        }
        return Results.Problem("Invalid file path");
    }

    private static void SetCodec(FFMpegArgumentOptions options, SupportedCodec codec) {
        switch (codec) {
            case SupportedCodec.AV1:
                options.WithCustomArgument("-c:v libsvtav1 -preset 8");
                break;

            case SupportedCodec.H265:
                options.WithVideoCodec(VideoCodec.LibX265);
                break;

            default:
                options.WithVideoCodec(VideoCodec.LibX264);
                break;
        }
    }

    private static bool IsPathValid(String path) {
        var inputFiles = Directory.GetFiles("./TestData/Input/");

        for (int i = 0; i < inputFiles.Length; i++) {
            if (inputFiles[i] == path) {
                return true;
            }
        }
        return false;
    }
}
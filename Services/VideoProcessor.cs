namespace Cinder.Services;

using System.Text;
using FFMpegCore;
using FFMpegCore.Enums;

public static class VideoProcessor {
    public enum SupportedCodec {
        AV1,
        H264,
        H265
    }

    public record VideoQuality(
        string Name,
        int Width,
        int Height,
        int VideoBitrate,
        int AudioBitrate
    );

    public static readonly List<VideoQuality> Qualities = new() {
        new("1440p", 2560, 1440, 10000, 384),
        new("1080p", 1920, 1080, 7500, 384),
        new("720p",  1280, 720,  5000, 192),
        new("480p",  854, 480,  3500, 128),
    };

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

    public static async Task<IResult> ProcessVideo(String inputPath, ILogger log, SupportedCodec codec, int videoId) {
        var validPath = IsPathValid(inputPath);
        var outputDir = Path.Combine("./TestData/Output/", videoId.ToString());

        if (validPath) {
            foreach (var quality in Qualities) {
                var dir = Path.Combine(outputDir, quality.Name);
                Directory.CreateDirectory(dir);

                var playlistPath = Path.Combine(dir, "playlist.m3u8");
                var segmentPath = Path.Combine(dir, "segment_%03d.ts");

                bool success = await FFMpegArguments
                    .FromFileInput(inputPath)
                    .OutputToFile(playlistPath, true, options => {
                        SetCodec(options, codec);
                        options.WithAudioCodec(AudioCodec.Aac)
                        .WithVideoBitrate(quality.VideoBitrate)
                        .WithAudioBitrate(quality.AudioBitrate)
                        .WithCustomArgument(
                            $"-vf scale=-2:{quality.Height} " +
                            $"-f hls -hls_time 6 -hls_playlist_type vod " +
                            $"-hls_segment_filename \"{segmentPath}\""
                        );
                    })
                    .NotifyOnProgress(percent => Console.WriteLine($"Progress: {percent}%"))
                    .NotifyOnOutput(line => Console.WriteLine($"[ffmpeg:{quality.Name}] {line}"))
                    .ProcessAsynchronously();
                if (!success) {
                    log.LogError("The video processing failed.");
                    return Results.InternalServerError("The video processing failed.");
                }
            }
            GenerateMasterPlaylist(outputDir);
            log.LogInformation("The video processing succeeded.");
            return Results.Ok("The video processing succeeded.");
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

    private static void GenerateMasterPlaylist(string baseOutputDir) {
        var sb = new StringBuilder();
        sb.AppendLine("#EXTM3U");

        foreach (var quality in Qualities) {
            // BANDWIDTH is in bits per second, not kbps — this is a common gotcha
            var bandwidthBps = (quality.VideoBitrate + quality.AudioBitrate) * 1000;

            sb.AppendLine($"#EXT-X-STREAM-INF:BANDWIDTH={bandwidthBps},RESOLUTION={quality.Width}x{quality.Height}");
            sb.AppendLine($"{quality.Name}/playlist.m3u8");
        }

        File.WriteAllText(Path.Combine(baseOutputDir, "master.m3u8"), sb.ToString());
    }
}
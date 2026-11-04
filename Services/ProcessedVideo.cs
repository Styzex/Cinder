using FFMpegCore;
using Cinder.Models;

namespace Cinder.Services;

public class ProcessedVideo(PathString path)
{
  private readonly IMediaAnalysis analysis = FFProbe.Analyse(path);
  public readonly PathString Path = path;
  public string Name = SetName(path);
  public readonly SupportedCodec? Codec = DetectCodec();

  // I know that this sound really smart but it's actually stupidly simple
  // Get's the highest quality and all of the lower ones using a predicate
  public readonly List<Video.Quality> Qualities = Video.Qualities
    .Where(quality => quality.Height <= OriginalQuality()).ToList();

  private static int? OriginalQuality()
  {
    int? height = analysis.PrimaryVideoStream!.Height;
    return height;
  }

  // Should remove everything before '/' or '\' so with "Test/Something/my_thing.file" the result would be "my_thing"
  private static string SetName(PathString path)
  {
    var p = path.ToString().Trim().Replace('\\', '/');
    return System.IO.Path.GetFileNameWithoutExtension(p);
  }

  private static SupportedCodec? DetectCodec()
  {
    // Source - https://stackoverflow.com/a/33700000
    // Posted by timgavin
    // Retrieved 2026-11-13, License - CC BY-SA 3.0
    /*
     codec = exec(
     "ffprobe -v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 {$input['filename']}");
    */
    var codec = analysis.PrimaryVideoStream?.CodecName;


    // We use string.Equals instead of codec.Equals because for some magical reason it fixes the bug where if codec is null the ifs blow my foot off with a shotgun.
    if (string.Equals(codec, "av1", StringComparison.OrdinalIgnoreCase))
      return SupportedCodec.Av1;
    if (string.Equals(codec, "h264", StringComparison.OrdinalIgnoreCase))
      return SupportedCodec.H264;
    if (string.Equals(codec, "h265", StringComparison.OrdinalIgnoreCase))
      return SupportedCodec.H265;
    return null;
  }
}

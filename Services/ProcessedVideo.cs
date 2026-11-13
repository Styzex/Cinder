using FFMpegCore;
using Cinder.Models;

namespace Cinder.Services;

public class ProcessedVideo
{
  public PathString Path { get; }
  public string Name { get; }
  public SupportedCodec? Codec { get; }
  public IReadOnlyList<Video.Quality> Qualities { get; }

  // Moved to a regular constructor from a primary constructor because it was causing me issues
  public ProcessedVideo(PathString path)
  {
    var analysis = FFProbe.Analyse(path);

    Path = path;
    // Should remove everything before '/' or '\' so with "Test/Something/my_thing.file" the result would be "my_thing"
    Name = System.IO.Path.GetFileNameWithoutExtension(path.ToString().Replace('\\', '/'));
    Codec = DetectCodec(analysis.PrimaryVideoStream?.CodecName);
    // I know that this sound really smart but it's actually stupidly simple
    // Get's the highest quality and all of the lower ones using a predicate
    Qualities = Video.Qualities
      .Where(quality =>
        quality.Height <=
        (analysis.PrimaryVideoStream?.Height ??
         0)) // I just moved the GetOriginalQuality methods body into the paranthesis
      .ToList();
  }

  private static SupportedCodec? DetectCodec(string? codec)
  {
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

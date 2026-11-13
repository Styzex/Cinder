using FFMpegCore;
using Cinder.Models;

namespace Cinder.Services;

public class ProcessedVideo
{
  public PathString Path { get; }
  public string Name { get; }
  public SupportedCodec? Codec { get; }
  public IReadOnlyList<Video.Quality> Qualities { get; }

  public ProcessedVideo(PathString path)
  {
    var analysis = FFProbe.Analyse(path);

    if (analysis.PrimaryVideoStream == null)
    {
      throw new InvalidOperationException("Primary video stream is null.");
    }

    Path = path;
    Name = System.IO.Path.GetFileNameWithoutExtension(path.ToString().Replace('\\', '/'));
    Codec = DetectCodec(analysis.PrimaryVideoStream?.CodecName);
    Qualities = Video.Qualities
      .Where(quality =>
        quality.Height <=
        (analysis.PrimaryVideoStream?.Height ??
         0))
      .ToList();
  }

  private static SupportedCodec? DetectCodec(string? codec)
  {
    // We use string.Equals instead of codec.Equals because for some magical reason it makes audio-only files have no codec name.
    if (string.Equals(codec, "av1", StringComparison.OrdinalIgnoreCase))
      return SupportedCodec.Av1;
    if (string.Equals(codec, "h264", StringComparison.OrdinalIgnoreCase))
      return SupportedCodec.H264;
    if (string.Equals(codec, "h265", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(codec, "hevc", StringComparison.OrdinalIgnoreCase))
      return SupportedCodec.H265;
    return null;
  }
}

using FFMpegCore;

namespace Cinder.Services;

public static class Video
{
  public record Quality(
    string Name,
    int Width,
    int Height,
    int VideoBitrate,
    int AudioBitrate
  );
  public static readonly List<Quality> Qualities = new()
  {
    new Quality("1440p", 2560, 1440, 10000, 384),
    new Quality("1080p", 1920, 1080, 7500, 384),
    new Quality("720p", 1280, 720, 5000, 192),
    new Quality("480p", 854, 480, 3500, 128)
  };
}

public class ProcessedVideo(PathString path)
{
  public PathString Path = path;
  public string Name = "";

  // Get's the highest quality and all of the lower ones using a predicate
  public readonly List<Video.Quality> qualities = Video.Qualities
    .Where(quality => quality.Height <= OriginalQuality(path)).ToList();

  private static int? OriginalQuality(PathString path)
  {
    Nullable<int> height = FFProbe.Analyse(path).PrimaryVideoStream.Height;
    return height;
  }

  private void SetName()
  {
    Name = Path.ToString().Trim().Concat();
  }
}

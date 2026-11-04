namespace Cinder.Models;

public static class Video
{
  public record Quality(
    string Name,
    int Width,
    int Height,
    int VideoBitrate,
    int AudioBitrate
  );

  public static readonly List<Quality> Qualities =
  [
    new("1440p", 2560, 1440, 10000, 384),
    new("1080p", 1920, 1080, 7500, 384),
    new("720p", 1280, 720, 5000, 192),
    new("480p", 854, 480, 3500, 128)
  ];
}

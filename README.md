# Cinder

## Overview

Cinder is a C# backend written with ASP.NET Core. It's main use case is for transcoding videos to the HLS format which can then be served to the Scoria frontend which also works as a video "streaming" platfrom.

## To-Do

- [x] Video processing endpoint
- [x] Object store-like data structure
- [ ] Filter the quality ladder by source resolution with `ProcessedVideo`, so inputs aren't upscaled
- [ ] Video upload endpoint
- [ ] Video download endpoint, the backend will send the user an mp4 version of the video in the quality selected by the user

## Architecture

The videos actually live on disk in a folder structure that's made to resemble an S3 like object store.
A compiled version of Scoria lives in `wwwroot/` so that the frontend can be served from the same process as the backend.

### Components

| Path                         | Role                                                                                                                                      |
| ---------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| `Program.cs`                 | Minimal API host: CORS for the Scoria dev server, static file + SPA fallback, and the `/api/inputs`, `/api/process` and `/hls` endpoints. |
| `Services/VideoProcessor.cs` | The transcoding pipeline: path validation, per-quality FFmpeg/HLS runs, master playlist generation.                                       |
| `Services/Video.cs`          | The quality ladder (name, resolution, video/audio bitrate) used for encoding and the master playlist.                                     |
| `Services/SupportedCodec.cs` | Video codecs the processor can emit: AV1, H.264, H.265.                                                                                   |
| `Services/ProcessedVideo.cs` | Probes an input with FFprobe to get its resolution and derives the qualities at or below it (not wired into the pipeline yet).            |
| `Data/Input`, `Data/Output`  | Source videos and the generated HLS tree. Replaces what would be an object store in production.                                           |

# Scoria

## Overview

As stated above this is the frontend part of Cidner, it's main purpose is fetchign the HLS videos and serving them to the user in a easy to use way, it is written in Vue and depends on hls.js. It's a work in progress.

## To-Do

- [x] Support HLS video fetching
- [ ] Video quality switching
- [x] ;3

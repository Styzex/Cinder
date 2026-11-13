# Cinder

## Overview

Cinder is a C# backend written with ASP.NET Core. It's main use case is for transcoding videos to the HLS format which can then be served to the Scoria frontend which also works as a video "streaming" platfrom.

## To-Do

- [x] Video processing endpoint
- [x] Object store-like data structure
- [ ] Filter the quality ladder by source resolution with `ProcessedVideo`, so inputs aren't upscaled
- [ ] Video upload endpoint
- [ ] Video download endpoint, the backend will send the user an mp4 version of the video in the quality selected by the user

### Important To-Do I guess :3

- [ ] Fix where files go — right now one part of the app writes encoded videos to a relative folder and the /hls part reads from a hardcoded C:\... path. They need to point at the same place. (`Program.cs:55` vs `Services/VideoProcessor.cs:41`)
- [ ] Make the Data/Input and Data/Output folders at startup, so a fresh clone doesn't break with a 500 error. (`Program.cs:27`, `Services/VideoProcessor.cs:90`)
- [ ] Encode into a temporary folder and only move it into place when it all succeeds. That way a failed or repeated encode doesn't leave a mix of old and new files. (`Services/VideoProcessor.cs:41-86`)
- [ ] Reject a second encode with the same id while one is already running. (`Program.cs:39-53`)
- [ ] Actually use ProcessedVideo inside ProcessVideo, so a 480p video doesn't get blown up into a fake 1440p one. (`Services/VideoProcessor.cs:49` and `:102`)
- [x] Fix H.265 detection — FFprobe calls it hevc, not h265, so the check never matches. (`Services/ProcessedVideo.cs:39`)
- [ ] Reject videos that have no video stream, instead of handing back an empty playlist (`Services/ProcessedVideo.cs:21-29`)
- [ ] Make /api/process a POST instead of a GET, and pass a CancellationToken so a cancelled encode actually stops (`Program.cs:39`, `Services/VideoProcessor.cs:57`, `Cinder.http:3`)
- [ ] Add a small test project covering three things: path validation, the master playlist output, and the codec mapping
- [ ] Stop `/api/inputs` from returning the absolute server path (`Program.cs:34`)
- [ ] Return 404 for unknown routes instead of `index.html` with a 200 (`Program.cs:23`)

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

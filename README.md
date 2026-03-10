# ConvertVideo2GIF - FFmpeg Video Processing Tool

A powerful .NET 6 console application for video processing operations using FFmpeg.

[![.NET Version](https://img.shields.io/badge/.NET-6.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/6.0)

## Features

- **Video Cutting** - Cut videos with precise timestamp control (HH:MM:SS format)
- **Video Merging** - Merge multiple video files with validation
- **Volume Adjustment** - Adjust audio volume by percentage (50%, 150%, etc.)
- **Audio & Video Combination** - Combine separate audio and video files
- **Unicode Support** - Full support for Chinese, Japanese, and other Unicode characters in filenames

## Quick Start

### Prerequisites

1. **.NET 6.0 SDK or Runtime**
   - Download from: https://dotnet.microsoft.com/download/dotnet/6.0

2. **FFmpeg**
   - Download from: https://ffmpeg.org/download.html
   - Or Windows builds: https://www.gyan.dev/ffmpeg/builds/
   - Or Unzip `ffpmeg.zip` in the `Resource` folder

### Installation

1. **Clone the repository**

2. **Setup FFmpeg**
   - Extract `ffmpeg.exe` from the downloaded FFmpeg package
   - Place it in the `ConvertVideo2GIF/Resources/` folder
   
   ```
   ConvertVideo2GIF/
      Resources/
         ffmpeg.exe  <- Place here
   ```

3. **Configure Working Directory**
   ```bash
   # Copy the sample configuration file
   cd ConvertVideo2GIF
   cp appsettings.sample.json appsettings.json
   ```
   
   Edit `appsettings.json` and set your video working directory:
   ```json
   {
     "AppSettings": {
       "WorkingDirectory": "the folder where your video is in"
     }
   }
   ```

4. **Build and Run**
   ```bash
   dotnet build
   dotnet run --project ConvertVideo2GIF
   ```

## Usage

### Feature Details

#### 1. Cut Video
Extract a portion of a video by specifying start and end times.

**Example:**
- Input file: `movie.mp4` (without extension)
- Output file: `movie_cut` (optional, auto-generated if empty)
- Start time: `00:01:30` (1 minute 30 seconds)
- End time: `00:03:45` (3 minutes 45 seconds)

#### 2. Merge Videos
Combine multiple video files into one. The tool validates each file before merging.

**Example:**
- Output file: `merged_video`
- Input files: `video1`, `video2`, `video3` (enter one by one)

#### 3. Adjust Volume
Change the audio volume by percentage.

**Example:**
- Input file: `movie`
- Volume: `150` (for 150% volume, or `50` for 50% volume)
- Output file: `movie_volume150` (auto-generated)

#### 4. Combine Audio and Video
Merge a separate audio file with a video file.

**Example:**
- Audio file: `audio.mp3` (with extension)
- Video file: `video` (without extension)
- Output file: `video_output` (optional)

## Configuration

### appsettings.json

```json
{
  "AppSettings": {
    "WorkingDirectory": "D:\\Videos\\"
  }
}
```

| Setting | Description | Required | Default |
|---------|-------------|----------|---------|
| WorkingDirectory | Directory where videos are stored and processed | No | %UserProfile%\Downloads\ |


## Notes

- **File Naming:** Input filenames should be without extension (.mp4 is assumed)
- **Conflict Resolution:** Output files are automatically renamed if conflicts exist
- **Unicode Support:** Full support for Chinese, Japanese, and other Unicode characters
- **Validation:** Video files are validated before merging operations

## Important

- **Do not commit** `appsettings.json` to version control (it may contain your personal directory paths)
- **Do not commit** video/audio files (they are large and may contain personal content)
- **Do not commit** FFmpeg binaries (users should download them separately)

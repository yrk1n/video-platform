# Video Processing Platform

A full-stack application that provides video upload, processing, and playback capabilities. The system automatically processes uploaded videos into multiple formats while maintaining the original quality.

## Features

- Video upload with metadata (name and genre)
- Automatic video processing to multiple formats:
  - Native resolution copy
  - 720p version
- Real-time processing status tracking
- Support for various video formats (including MKV conversion)
- Concurrent video processing with CPU core optimization
- RESTful API endpoints for video management

## Tech Stack

### Frontend
- React with TypeScript
- Vite build tool
- Modern CSS

### Backend
- ASP.NET Core
- FFmpeg for video processing
- Channel-based background processing
- File-based metadata storage

## Prerequisites

- .NET 6.0 or later
- Node.js and npm
- FFmpeg installed via Chocolatey
- Sufficient storage space for video processing

## Installation

1. **Clone the repository**
   ```bash
   git clone [repository-url]
   ```

2. **Backend Setup**
   ```bash
   cd VideoUploadService
   dotnet restore
   dotnet build
   ```

3. **Frontend Setup**
   ```bash
   cd client
   npm install
   ```

4. **FFmpeg Installation (Windows)**
   ```bash
   choco install ffmpeg
   ```

## Running the Application

1. **Start the Backend**
   ```bash
   cd VideoUploadService
   dotnet run
   ```

2. **Start the Frontend**
   ```bash
   cd client
   npm run dev
   ```

## API Endpoints

### Video Controller

- `POST /api/video/upload` - Upload a new video with metadata
- `GET /api/video/list` - Get list of all uploaded videos
- `GET /api/video/versions/{fileName}` - Get available versions of a specific video



## Video Processing

The application processes videos in the following ways:
- Creates a copy of the original file
- Converts MKV files to MP4 if necessary
- Generates a native resolution version
- Creates a 720p version with optimized encoding

Processing is handled asynchronously with a queue system to manage system resources effectively.

## Configuration

The application uses default paths for storage:
- Uploaded videos: `wwwroot/uploads/`
- Processed videos: `wwwroot/processed/`
- Metadata: `wwwroot/metadata.json`

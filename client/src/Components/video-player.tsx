import React, { useState, useEffect } from "react";
import VideoInfo from "../types";

interface VideoPlayerProps {
  video: VideoInfo;
}

interface VideoVersion {
  resolution: string;
  url: string;
  isNative: boolean;
}

interface VideoVersionsResponse {
  versions: VideoVersion[];
}

const VideoPlayer: React.FC<VideoPlayerProps> = ({ video }) => {
  const [videoVersions, setVideoVersions] =
    useState<VideoVersionsResponse | null>(null);
  const [currentResolution, setCurrentResolution] = useState<string>("");

  useEffect(() => {
    const fetchVideoVersions = async () => {
      try {
        const versions = video.processedVersions.map((version) => {
          const isNative = version === "native";

          const url = `/processed/${video.fileName.split(".")[0]}/${version}.mp4`;

          // Debug logging
          console.log("Constructing URL:", {
            isNative,
            fileName: video.fileName,
            version,
            finalUrl: url,
          });

          return {
            resolution: isNative ? "native" : version,
            url,
            isNative,
          };
        });

        setVideoVersions({ versions });
        setCurrentResolution(versions[0].resolution);
      } catch (error) {
        console.error("Error setting video versions:", error);
      }
    };

    if (video) {
      fetchVideoVersions();
    }
  }, [video]);

  const handleResolutionChange = (resolution: string) => {
    setCurrentResolution(resolution);
    const video = document.getElementById(
      "videoPlayer",
    ) as HTMLVideoElement | null;
    if (video && videoVersions) {
      const currentTime = video.currentTime;
      const version = videoVersions.versions.find(
        (v) => v.resolution === resolution,
      );
      if (version) {
        video.src = `http://localhost:5253${version.url}`;
        video.currentTime = currentTime;
        video.play();
      }
    }
  };

  if (!videoVersions || !currentResolution) return null;

  return (
    <div className="bg-slate-700 rounded-lg shadow-lg p-6">
      <h2 className="text-xl font-bold mb-4">Now Playing: {video.fileName}</h2>
      <div className="w-full max-w-3xl mx-auto">
        <video
          id="videoPlayer"
          className="w-full rounded-lg shadow-md"
          controls
          src={`http://localhost:5253${videoVersions.versions.find((v) => v.resolution === currentResolution)?.url}`}
        />
        <div className="mt-4 flex items-center gap-2">
          <span className="text-gray-700">Quality:</span>
          <select
            value={currentResolution}
            onChange={(e) => handleResolutionChange(e.target.value)}
            className="px-4 py-2 border rounded bg-slate-700 shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            {videoVersions.versions.map((version) => (
              <option key={version.resolution} value={version.resolution}>
                {version.resolution}
              </option>
            ))}
          </select>
        </div>
      </div>
    </div>
  );
};

export default VideoPlayer;

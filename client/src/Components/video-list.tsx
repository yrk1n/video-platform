import { useState, useEffect } from "react";
import axios from "axios";
import VideoInfo from "../types";

interface VideoListProps {
  onVideoSelect: (video: VideoInfo) => void;
  selectedVideo: VideoInfo | null;
}

const VideoList: React.FC<VideoListProps> = ({
  onVideoSelect,
  selectedVideo,
}) => {
  const [videoList, setVideoList] = useState<VideoInfo[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchVideos = async () => {
    try {
      setError(null);
      const response = await axios.get<VideoInfo[]>(
        "http://localhost:5253/api/video/list",
      );
      setVideoList(response.data);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An unknown error occurred";
      setError(`Failed to fetch videos: ${message}`);
      console.error("Error fetching videos:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchVideos();
    const interval = setInterval(fetchVideos, 5000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div>
      <h2 className="text-xl font-bold mb-4">Uploaded Videos</h2>
      {isLoading && <p className="text-gray-400">Loading videos...</p>}
      {error && <p className="text-red-500">{error}</p>}
      <div className="space-y-4">
        {videoList.map((video, index) => (
          <div
            key={index}
            className={`border p-4 rounded cursor-pointer transition-colors
              ${selectedVideo?.fileName === video.fileName ? "bg-slate-700 border-blue-500" : "hover:bg-slate-600"}`}
            onClick={() => !video.isProcessing && onVideoSelect(video)}
          >
            <h3 className="font-medium">{video.name || video.fileName}</h3>
            {video.genre && (
              <p className="text-sm text-gray-400">Genre: {video.genre}</p>
            )}
            <div className="mt-2">
              {video.isProcessing ? (
                <span className="text-yellow-600">Processing...</span>
              ) : (
                <div>
                  <p className="text-green-600">Available versions:</p>
                  <ul className="list-disc list-inside">
                    {video.processedVersions.map((version) => (
                      <li key={version}>{version}</li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default VideoList;

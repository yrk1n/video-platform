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

  const fetchVideos = async () => {
    try {
      const response = await axios.get<VideoInfo[]>(
        "http://localhost:5253/api/video/list",
      );
      setVideoList(response.data);
    } catch (error) {
      console.error("Error fetching videos:", error);
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
      <div className="space-y-4">
        {videoList.map((video, index) => (
          <div
            key={index}
            className={`border p-4 rounded cursor-pointer transition-colors
              ${selectedVideo?.fileName === video.fileName ? "bg-slate-700 border-blue-500" : "hover:bg-slate-600"}`}
            onClick={() => !video.isProcessing && onVideoSelect(video)}
          >
            <h3 className="font-medium">{video.fileName}</h3>
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

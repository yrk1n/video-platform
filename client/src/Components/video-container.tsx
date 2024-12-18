import { useState } from "react";
import VideoList from "./video-list";
import VideoPlayer from "./video-player";
import VideoInfo from "../types";

const VideoContainer = () => {
  const [selectedVideo, setSelectedVideo] = useState<VideoInfo | null>(null);

  return (
    <div className="container mx-auto p-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="md:col-span-1">
          <VideoList
            onVideoSelect={setSelectedVideo}
            selectedVideo={selectedVideo}
          />
        </div>
        <div className="md:col-span-2">
          {selectedVideo && !selectedVideo.isProcessing && (
            <VideoPlayer video={selectedVideo} />
          )}
        </div>
      </div>
    </div>
  );
};

export default VideoContainer;

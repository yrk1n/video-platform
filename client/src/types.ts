interface VideoInfo {
  fileName: string;
  name?: string;
  genre?: string;
  isProcessing: boolean;
  processedVersions: string[];
}

export default VideoInfo;

import { useState, useEffect } from "react";
import axios from "axios";

const VideoUpload = () => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [videoList, setVideoList] = useState<string[]>([]);

  // Fetch list of uploaded videos
  const fetchVideos = async () => {
    try {
      const response = await axios.get("http://localhost:5253/api/video/list");
      setVideoList(response.data);
    } catch (error) {
      console.error("Error fetching videos:", error);
    }
  };

  useEffect(() => {
    fetchVideos();
  }, []);

  // Handle file selection
  // And update the handleFileChange function
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setSelectedFile(e.target.files[0]);
    }
  };

  // Handle file upload
  const handleUpload = async () => {
    if (!selectedFile) {
      alert("Please select a file first!");
      return;
    }

    const formData = new FormData();
    formData.append("file", selectedFile);

    try {
      await axios.post("http://localhost:5253/api/video/upload", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });
      alert("File uploaded successfully!");
      setSelectedFile(null);
      fetchVideos();
    } catch (error) {
      console.error("Error uploading file:", error);
      alert("Failed to upload file.");
    }
  };

  return (
    <div>
      <h1>Video Upload</h1>
      <input type="file" onChange={handleFileChange} />
      <button onClick={handleUpload}>Upload</button>

      <h2>Uploaded Videos</h2>
      <ul>
        {videoList.map((video, index) => (
          <li key={index}>{video}</li>
        ))}
      </ul>
    </div>
  );
};

export default VideoUpload;

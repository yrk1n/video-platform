import { useState } from "react";
import axios from "axios";

interface UploadProps {
  onFileUpload: (fileName: string | null) => void;
}

const Upload: React.FC<UploadProps> = ({ onFileUpload }) => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [name, setName] = useState("");
  const [genre, setGenre] = useState("");
  const [isUploading, setIsUploading] = useState(false);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const file = e.target.files[0];
      setSelectedFile(e.target.files[0]);

      setName(file.name.replace(/\.[^/.]+$/, ""));
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      alert("Please select a file first!");
      return;
    }

    if (!name.trim()) {
      alert("Please enter a name for the video!");
      return;
    }

    setIsUploading(true);
    try {
      const formData = new FormData();
      formData.append("file", selectedFile);
      formData.append("name", name);
      formData.append("genre", genre);

      const response = await axios.post(
        "http://localhost:5253/api/video/upload",
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
          maxContentLength: Infinity,
          maxBodyLength: Infinity,
          timeout: 600000,
          onUploadProgress: (progressEvent) => {
            const percentCompleted = Math.round(
              (progressEvent.loaded * 100) / progressEvent.total!,
            );
            console.log(`Upload Progress: ${percentCompleted}%`);
          },
        },
      );
      alert("File uploaded successfully!");
      onFileUpload(response.data.fileName);
      setSelectedFile(null);
      setName("");
      setGenre("");
    } catch (error) {
      console.error("Error uploading file:", error);
      alert("Failed to upload file.");
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">Video Upload</h1>
      <div className="mb-6 space-y-4">
        <div>
          <input
            type="file"
            onChange={handleFileChange}
            className="mb-2"
            accept="video/*, .mkv"
          />
        </div>
        <div>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Video Name"
            className="px-3 py-2 border rounded w-full max-w-md"
          />
        </div>
        <div>
          <input
            type="text"
            value={genre}
            onChange={(e) => setGenre(e.target.value)}
            placeholder="Genre"
            className="px-3 py-2 border rounded w-full max-w-md"
          />
        </div>
        <button
          onClick={handleUpload}
          disabled={isUploading}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:bg-gray-400"
        >
          {isUploading ? "Uploading..." : "Upload"}
        </button>
      </div>
    </div>
  );
};

export default Upload;

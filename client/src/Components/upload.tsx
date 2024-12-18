import { useState } from "react";
import axios from "axios";

interface UploadProps {
  onFileUpload: (fileName: string | null) => void;
}

const Upload: React.FC<UploadProps> = ({ onFileUpload }) => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setSelectedFile(e.target.files[0]);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      alert("Please select a file first!");
      return;
    }

    const formData = new FormData();
    formData.append("file", selectedFile);

    try {
      const response = await axios.post(
        "http://localhost:5253/api/video/upload",
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        },
      );
      alert("File uploaded successfully!");
      onFileUpload(response.data.fileName);
      setSelectedFile(null);
    } catch (error) {
      console.error("Error uploading file:", error);
      alert("Failed to upload file.");
    }
  };

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">Video Upload</h1>
      <div className="mb-6">
        <input
          type="file"
          onChange={handleFileChange}
          className="mb-2"
          accept="video/*"
        />
        <button
          onClick={handleUpload}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          Upload
        </button>
      </div>
    </div>
  );
};

export default Upload;

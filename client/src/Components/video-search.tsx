interface VideoSearchProps {
  onSearch: (searchTerm: string) => void;
}

const VideoSearch: React.FC<VideoSearchProps> = ({ onSearch }) => {
  return (
    <div className="mb-4">
      <input
        type="text"
        placeholder="Search videos by name..."
        className="w-full p-2 rounded bg-slate-700 border border-slate-600 focus:border-blue-500 focus:outline-none"
        onChange={(e) => onSearch(e.target.value)}
      />
    </div>
  );
};

export default VideoSearch;

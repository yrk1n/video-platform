import "./App.css";
import Upload from "./Components/upload";
import VideoContainer from "./Components/video-container";

function App() {
  return (
    <>
      <Upload onFileUpload={() => {}} />
      <VideoContainer />
    </>
  );
}

export default App;

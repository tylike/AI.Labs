
namespace TranscribeCS
{
    public class NewSegmentArgs
    {
        public List<NewSegment> Segments { get; } = new List<NewSegment>();
        public int NewCount { get; set; }
    }

    public class NewSegment
    {
        public string Text { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan StartTimeSpan { get; set; }
        public TimeSpan EndTimeSpan { get; set; }
    }
    public interface IWhisperEngine
    {
        string ModelFile { get; }

        event EventHandler<NewSegmentArgs> NewSegments;

        void Dispose();
        string GetTextFromWavData(byte[] wavFileData, string prompts = null);
        void Setup();
        void StartRealTimeSpeechRecognition();
        void StopRealTimeSpeechRecognition();
        void Unload();
    }
}
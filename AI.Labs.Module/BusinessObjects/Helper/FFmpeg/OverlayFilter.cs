//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects
{
    public class OverlayFilter : FFMpegFilter
    {
        public string BackgroundVideoLabel { get; }
        public string OverlayVideoLabel { get; }
        public string OutputLabel { get; }

        public OverlayFilter(string backgroundVideoLabel, string overlayVideoLabel, string outputLabel)
        {
            BackgroundVideoLabel = backgroundVideoLabel;
            OverlayVideoLabel = overlayVideoLabel;
            OutputLabel = outputLabel;
        }

        public override string ToString()
        {
            return $"[{BackgroundVideoLabel}][{OverlayVideoLabel}]overlay[overlayed{OutputLabel}]";
        }
    }
}

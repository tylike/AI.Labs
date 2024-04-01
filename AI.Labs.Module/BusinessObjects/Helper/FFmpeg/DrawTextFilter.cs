//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects
{
    public class DrawTextFilter : FFMpegFilter
    {
        public string Text { get; }
        public string FontFile { get; }
        public int FontSize { get; }
        public string FontColor { get; }

        public DrawTextFilter(string text, string fontFile, int fontSize, string fontColor = "white")
        {
            Text = text.Replace("'", "\\'");
            FontFile = fontFile;
            FontSize = fontSize;
            FontColor = fontColor;
        }

        public override string ToString()
        {
            return $"drawtext=text='{Text}':fontfile={FontFile}:fontsize={FontSize}:fontcolor={FontColor}:x=(w-text_w)/2:y=(h-text_h)/2";
        }
    }
}

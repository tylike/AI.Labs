//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects
{
    public abstract class FFMpegFilter
    {
        public string Label { get; set; }
        public abstract override string ToString();
    }
}

//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects
{
    // Concat filter to concatenate multiple video streams
    public class ConcatFilter : FFMpegFilter
    {
        public int StreamCount { get; }

        public ConcatFilter(int streamCount)
        {
            StreamCount = streamCount;
        }

        public override string ToString()
        {
            return $"concat=n={StreamCount}:v=1:a=0";
        }
    }
}

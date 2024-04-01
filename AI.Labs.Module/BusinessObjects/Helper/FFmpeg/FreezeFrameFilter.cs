//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects
{
    public class FreezeFrameFilter : FFMpegFilter
    {
        public TimeSpan Duration { get; }

        public FreezeFrameFilter(TimeSpan duration)
        {
            Duration = duration;
        }

        public override string ToString()
        {
            return $"trim=duration={Duration.TotalSeconds},setpts=PTS-STARTPTS";
        }
    }
}

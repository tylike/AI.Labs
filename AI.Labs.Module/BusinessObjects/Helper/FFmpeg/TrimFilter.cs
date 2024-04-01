//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects
{
    public class TrimFilter : FFMpegFilter
    {
        public TimeSpan Start { get; set; }
        public TimeSpan? Duration { get; set; }

        public TrimFilter(TimeSpan start, TimeSpan? duration = null)
        {
            Start = start;
            Duration = duration;
        }

        public override string ToString()
        {
            string filter = $"trim=start={Start.TotalSeconds}";
            if (Duration.HasValue)
            {
                filter += $":duration={Duration.Value.TotalSeconds}";
            }
            filter += ",setpts=PTS-STARTPTS";
            return filter;
        }
    }
}

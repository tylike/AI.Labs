//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
using DevExpress.Spreadsheet;
namespace AI.Labs.Module.BusinessObjects
{
    public class SegmentMuxerOptions
    {
        public string InputFile { get; set; }
        /// <summary>
        /// output%03d.mp4
        /// </summary>
        public string OutputFile { get; set; }
        public int? SegmentTime { get; set; }
        public IEnumerable<double> SegmentTimes { get; set; }
        public string SegmentFormat { get; set; }
        public string SegmentList { get; set; }

        // 其他参数...

        public string ToArgumentString()
        {
            var times = SegmentTimes.Select(t => t.ToFFmpegFormat()).Join(",");

            var args = $"-i {InputFile}  -copyts -avoid_negative_ts 1  -c:v libx264 -crf 23 -map 0 -force_key_frames {times} -x264-params keyint=25:scenecut=0 -f segment";

            if (SegmentTime.HasValue)
                args += $" -segment_time {SegmentTime.Value}";

            if (SegmentTimes?.Any() == true)
                args += $" -segment_times {times}";

            if (!string.IsNullOrEmpty(SegmentFormat))
                args += $" -segment_format {SegmentFormat}";

            if (!string.IsNullOrEmpty(SegmentList))
                args += $" -segment_list {SegmentList}";

            // 其他参数...

            args += $" {OutputFile}";

            return args;
        }
    }
}

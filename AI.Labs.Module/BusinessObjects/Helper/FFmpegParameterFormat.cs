using System.Drawing;
using System.Text;

namespace AI.Labs.Module.BusinessObjects
{
    public static class FFmpegParameterFormat
    {
        public static string ToFFmpegColorString(this Color color)
        {
            return $"&H{255 - color.A:X2}{color.B:X2}{color.G:X2}{color.R:X2}";
        }
        public static void AppendNotEmptyOrNull(this StringBuilder sb, string text, bool appendLine = false)
        {
            if (!string.IsNullOrEmpty(text))
            {
                sb.Append(text);
                if (appendLine)
                    sb.AppendLine();
            }
        }
        public static string Join(this IEnumerable<string> values, string separator = "")
        {
            return string.Join(separator, values);
        }
        public static string ToFFmpegSeconds(this TimeSpan time)
        {
            return time.TotalSeconds.ToString("0.0#####");
        }
        public static TimeSpan AddMilliseconds(this TimeSpan time, double milliseconds)
        {
            return time.Add(TimeSpan.FromMilliseconds(milliseconds));
        }
        public static TimeSpan Subtract(this TimeSpan time, double substract)
        {
            return time.Subtract(TimeSpan.FromMilliseconds(substract));
        }
        public static string GetTimeString(this TimeSpan time)
        {
            return time.ToString(@"hh\:mm\:ss\.fff");
        }
        public static string ToFFmpegString(this double value)
        {
            return value.ToString();// ("0.0#####");
        }
        public static string ToFFmpegString(this float value)
        {
            return value.ToString();// ("0.0#####");
        }

        // 假设视频是50fps，每帧的持续时间是20毫秒
        private const int FrameDuration = 100; // 毫秒

        public static TimeSpan AdjustTime(this TimeSpan self,bool plus)
        {
            // 计算最近的帧边界毫秒数
            int milliseconds = self.Milliseconds;
            int adjustedMilliseconds = (milliseconds / FrameDuration) * FrameDuration;
            var s = self.Seconds;
            if (milliseconds > 0 && plus)
            {
                s++;
            }
            // 创建修正后的TimeSpan
            return new TimeSpan(self.Days,self.Hours, self.Minutes, self.Seconds,adjustedMilliseconds);
        }
    }
}

using System.Text;

namespace AI.Labs.Module.BusinessObjects
{
    public static class FFmpegParameterFormat
    {
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
            return value.ToString("0.0#####");
        }
        public static string ToFFmpegString(this float value)
        {
            return value.ToString("0.0#####");
        }
    }
}

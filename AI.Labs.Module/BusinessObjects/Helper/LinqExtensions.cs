//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
using DevExpress.XtraSpreadsheet.Commands;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AI.Labs.Module.BusinessObjects
{
    public static class LinqExtensions
    {
        public static bool TryParseToTimeSpan(this string str, out TimeSpan timespan)
        {
            // Input formats:
            // 0:00
            // 00:00
            // 00:00:00

            if (str.Length == 4) // m:ss
            {
                var parts = str.Split(':');
                timespan = new TimeSpan(
                    0, // hours
                    int.Parse(parts[0]), // minutes
                    int.Parse(parts[1])); // seconds
                return true;
            }
            else if (str.Length == 5) // 00:00
            {
                var parts = str.Split(':');
                timespan = new TimeSpan(
                    0, // hours
                    int.Parse(parts[0]), // minutes
                    int.Parse(parts[1])); // seconds
                return true;
            }

            return TimeSpan.TryParse(str, out timespan);
        }


        public static TimeSpan ClearMilliseconds(this TimeSpan time)
        {
            return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        }
        public static bool ContainsTimespan(this string str, out TimeSpan timespan)
        {
            var parts = str.Split();

            foreach (var part in parts)
            {
                // Add a regular expression that matches a timestamp preceded by a number and a period, with or without a space
                var regex = new Regex(@"(\d+\.\s*)?(\d{1,2}:\d{2}(:\d{2})?)");
                var match = regex.Match(part);

                if (match.Success)
                {
                    var cleaned = match.Groups[2].Value;  // The second group in the regex is the timestamp

                    if (cleaned.TryParseToTimeSpan(out timespan))
                        return true;
                }
            }

            timespan = TimeSpan.MaxValue;
            return false;
        }

        public static string RemoveTimespan(this string str)
        {
            var stringBuilder = new StringBuilder();
            var parts = str.Split();

            foreach (var part in parts)
                if (part.ContainsTimespan(out _) == false)
                    stringBuilder.Append(part + " ");

            return stringBuilder.ToString().Trim();
        }

        public static string RemoveIllegalCharacters(this string str)
        {
            var invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + "\"~|";
            return invalid.Aggregate(str, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    
        public static void Log(this StringBuilder log,string text,bool isError = false,bool showDebug = false)
        {
            if (showDebug)
            {
                Debug.Write(text);
            }
            else
            {
                Debug.Write(".");
            }

            if(string.IsNullOrEmpty(text))
            {
                return;
            }
            if(isError)
            {
                text = $"!!!: {text}";
            }

            log.AppendLine(text);
        }

        public static IEnumerable<IEnumerable<T>> GroupWhile<T>(
            this IEnumerable<T> source, Func<T, T, bool> predicate)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break;
                }

                List<T> currentGroup = new List<T> { iterator.Current };

                while (iterator.MoveNext())
                {
                    if (predicate(currentGroup.Last(), iterator.Current))
                    {
                        // 如果当前元素满足条件，添加到当前分组
                        currentGroup.Add(iterator.Current);
                    }
                    else
                    {
                        // 如果不满足条件，返回当前分组并开始新的分组
                        yield return currentGroup;
                        currentGroup = new List<T> { iterator.Current };
                    }
                }

                // 返回最后一个分组
                yield return currentGroup;
            }
        }
    }
}

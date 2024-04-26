using System.Text;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using System.Text.RegularExpressions;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class SrtChecker
    {
        public static string CheckSrtFile(string filePath)
        {
            var sb = new StringBuilder();
            var lines = File.ReadAllLines(filePath);
            var regex = new Regex(@"^\d+$");
            var timeRegex = new Regex(@"\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{2},\d{3}");

            for (int i = 0; i < lines.Length; i++)
            {
                if (i % 4 == 0 && !regex.IsMatch(lines[i]))
                {
                    sb.AppendLine($"错误行号: {i + 1}: {lines[i]} 不是有效的行号.");
                }
                else if (i % 4 == 1 && !timeRegex.IsMatch(lines[i]))
                {
                    sb.AppendLine($"错误行号: {i + 1}: {lines[i]} 不是有效的时间格式.");
                }
            }
            return sb.ToString();
        }
        public static string CheckSrt(string input)
        {
            var sb = new StringBuilder();
            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var regex = new Regex(@"^\d+$");
            var timeRegex = new Regex(@"\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{2},\d{3}");

            for (int i = 0; i < lines.Length; i++)
            {
                if (i % 4 == 0 && !regex.IsMatch(lines[i]))
                {
                    sb.AppendLine($"错误行号: {i + 1}: {lines[i]} 不是有效的行号.");
                }
                else if (i % 4 == 1 && !timeRegex.IsMatch(lines[i]))
                {
                    sb.AppendLine($"错误行号: {i + 1}: {lines[i]} 不是有效的时间格式.");
                }
            }
            return sb.ToString();
        }
    }

}

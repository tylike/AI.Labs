using System.Text;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using System.Text.RegularExpressions;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class SrtFixer
    {
        public static string AutoFixSrtFormat(string input)
        {
            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0 && IsNumeric(lines[i]) && !string.IsNullOrWhiteSpace(lines[i - 1]))
                {
                    sb.AppendLine();
                }
                sb.AppendLine(lines[i]);
            }
            return sb.ToString();
        }

        public static void AutoFixSrtFileFormat(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            using (var sw = new StreamWriter(filePath))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i > 0 && IsNumeric(lines[i]) && !string.IsNullOrWhiteSpace(lines[i - 1]))
                    {
                        sw.WriteLine();
                    }
                    sw.WriteLine(lines[i]);
                }
            }
        }

        private static bool IsNumeric(string line)
        {
            return Regex.IsMatch(line, @"^\d+$");
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class GoogleTranslateFix
    {
        public static void FixFile(string inputFilePath,string outputFilePath)
        {
            // 读取SRT文件内容
            string[] lines = File.ReadAllLines(inputFilePath);

            // 正则表达式用于匹配错误的编号格式和正确的行号
            string chapterPattern = @"^第(\d+)章$";
            string lineNumberPattern = @"^\d+$";

            using (StreamWriter sw = new StreamWriter(outputFilePath))
            {
                foreach (string line in lines)
                {
                    // 检查当前行是否匹配错误的编号格式
                    Match chapterMatch = Regex.Match(line, chapterPattern);
                    if (chapterMatch.Success)
                    {
                        // 如果匹配成功，则替换为正确的编号格式
                        sw.WriteLine(chapterMatch.Groups[1].Value);
                    }
                    else
                    {
                        // 检查当前行是否为正确的行号
                        Match lineNumberMatch = Regex.Match(line, lineNumberPattern);
                        if (lineNumberMatch.Success || line.Trim() == "")
                        {
                            // 如果是正确的行号或者是空行（字幕段落之间的分隔），则写入行
                            sw.WriteLine(line);
                        }
                        else
                        {
                            // 如果不是行号，也不是空行，则检查内容是否不违反SRT格式规则
                            // 如果确定是有效的字幕内容，则写入行
                            if (IsValidSrtContent(line))
                            {
                                sw.WriteLine(line);
                            }
                            else
                            {
                                // 这里可以根据需要处理无效内容，例如输出日志、忽略等
                                Debug.WriteLine("Invalid SRT content detected: " + line);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("SRT文件修复完成。");
        }

        // 检查是否为有效的SRT内容（时间轴或对话）
        static bool IsValidSrtContent(string line)
        {
            // 正则表达式用于匹配SRT文件的时间轴
            string timecodePattern = @"^\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{2},\d{3}$";
            Match timecodeMatch = Regex.Match(line, timecodePattern);
            if (timecodeMatch.Success)
            {
                return true;
            }

            // 如果不是时间轴，那么它应该是对话文本
            // 这里可以根据实际需求添加更多的检查
            return !string.IsNullOrWhiteSpace(line);
        }
    }




}

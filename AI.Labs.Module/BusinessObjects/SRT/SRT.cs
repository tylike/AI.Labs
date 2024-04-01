//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
using AI.Labs.Module.BusinessObjects.Helper;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using System.Text;

namespace AI.Labs.Module
{
    public interface ISRT
    {
        public int Index { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Text { get; set; }
    }

    /// <summary>
    /// 简单的字幕
    /// </summary>
    public class SRT : ISRT
    {
        public int Index { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Text { get; set; }
    }

    public class SRTFile
    {
        public string FileName { get; set; }
        public bool UseIndex { get; set; }
        public List<SRT> Texts { get; } = new List<SRT>();
        
        public void Load()
        {
            Texts.AddRange(new SRTParser().ParseStream<SRT>(new FileStream(FileName, FileMode.Open), Encoding.UTF8, true, () => new SRT()));
        }
        public void Save()
        {
            // 移除所有内容为空的字幕项
            //subtitleItems = subtitleItems.Where(s => s.Lines.Any(line => !string.IsNullOrWhiteSpace(line))).ToList();

            // 保存到新的SRT文件中
            using (var fileStream = new FileStream(FileName, FileMode.Create))
            {
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    int l = 0;
                    for (int i = 0; i < Texts.Count; i++)
                    {

                        var item = Texts[i];
                        var text = item.Text;

                        if (text.Length > 0)
                        {
                            var endTime = item.EndTime;
                            //这样就是没有使用当前的结束时间，而是使用了下一个字幕的开始时间，中间是没有间隔的
                            //if (i + 1 < Texts.Count)
                            //{
                            //    var next = Texts[i + 1];
                            //    endTime = next.StartTime;
                            //}
                            writer.WriteLine(l++);
                            writer.WriteLine($"{item.StartTime.ToString(@"hh\:mm\:ss\,fff")} --> {endTime.ToString(@"hh\:mm\:ss\,fff")}");

                            //如果字幕是多行的,当前未处理
                            //foreach (var line in item.Lines)
                            //{
                            //    writer.WriteLine(line);
                            //}

                            writer.WriteLine(text);
                            writer.WriteLine();
                        }
                    }
                }
            }
        }
    }

}

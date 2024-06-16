//using SubtitlesParser.Classes; 
// 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;

using System.Text.RegularExpressions;

namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class VideoChapter
    {
        public VideoChapter()
        {
        }

        public VideoChapter(string original, string name, TimeSpan timespan)
        {
            Original = original;
            Name = name;
            StartTimespan = timespan;
        }

        public string Original { get; set; }

        public int Index { get; set; }

        public string Name { get; set; }

        public string UniqueName { get; set; }

        public TimeSpan StartTimespan { get; set; }

        public TimeSpan? EndTimespan { get; set; }

        public TimeSpan? Duration => EndTimespan - StartTimespan;
    }

    public class VideoDescription
    {
        // ReSharper disable once InconsistentNaming
        private readonly string[] FirstChapterVariants =
        {
        "0:00",
        "00:00",
        "00:00:00"
    };

        public VideoDescription(string description)
        {
            Description = description;
        }

        private string Description { get; }
        const string pattern = @"^\s*(\d{1,2}:\d{1,2}).*";
        string FindFirstLine(IEnumerable< string> lines)
        {
            return lines.FirstOrDefault(t => Regex.Match(t, pattern).Success);
        }
        public IReadOnlyCollection<VideoChapter> ParseChapters()
        {
            if (Description == null)
                return null;
                //throw new Exception("Could not parse any chapter");

            var chapters = new List<VideoChapter>();

            var lines = Description.Split(Environment.NewLine).ToList();
            lines = lines.Count == 1 ? Description.Split(@"\n").ToList() : lines;

            var firstLine = FindFirstLine(lines); // lines.FirstOrDefault(line => FirstChapterVariants.Any(line.Contains));
            var index = lines.IndexOf(firstLine);

            if (index == -1 || lines.Count == 0)
                return null ;
                //throw new Exception("Could not parse any chapter");

            var chapterIndex = -1;
            while (lines.Count > index)
            {
                var line = lines[index].Trim();
                index++;

                try
                {
                    if (line.ContainsTimespan(out var timespan) == false)
                        continue;

                    var chapter = new VideoChapter
                    {
                        Index = ++chapterIndex,
                        Original = line,
                        StartTimespan = timespan,
                        Name = line.RemoveTimespan().RemoveIllegalCharacters()
                    };

                    chapter.UniqueName = chapter.Name;

                    var duplicatesCount = chapters.Count(s => s.Name == chapter.Name);
                    if (duplicatesCount > 0)
                    {
                        chapter.UniqueName += $" ({duplicatesCount + 1})";
                    }

                    chapters.Add(chapter);
                }
                catch
                {
                    // ignored
                }
            }

            for (var i = 0; i < chapters.Count; i++)
            {
                var currentChapter = chapters.ElementAt(i);
                currentChapter.EndTimespan = i == chapters.Count - 1
                    ? null
                    : chapters.ElementAt(i + 1).StartTimespan;
            }

            return chapters;
        }
    }
}

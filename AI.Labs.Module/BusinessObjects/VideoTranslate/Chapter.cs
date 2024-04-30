using DevExpress.Xpo;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using System.Drawing;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class Chapter : XPObject
    {
        public Chapter(Session s) : base(s)
        {

        }

        [Association]
        public VideoInfo Video
        {
            get { return GetPropertyValue<VideoInfo>(nameof(Video)); }
            set { SetPropertyValue(nameof(Video), value); }
        }

        public int BoxX
        {
            get { return GetPropertyValue<int>(nameof(BoxX)); }
            set { SetPropertyValue(nameof(BoxX), value); }
        }

        public int BoxW
        {
            get { return GetPropertyValue<int>(nameof(BoxW)); }
            set { SetPropertyValue(nameof(BoxW), value); }
        }



        public TimeSpan FixedStartTime
        {
            get =>Subtitles.Min(t=>t.FixedStartTime);
        }

        public TimeSpan FixedEndTime
        {
            get => Subtitles.Max(t => t.FixedEndTime);
        }


        public TimeSpan StartTime
        {
            get { return GetPropertyValue<TimeSpan>(nameof(StartTime)); }
            set { SetPropertyValue(nameof(StartTime), value); }
        }

        public TimeSpan EndTime
        {
            get { return GetPropertyValue<TimeSpan>(nameof(EndTime)); }
            set { SetPropertyValue(nameof(EndTime), value); }
        }

        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        [Size(-1)]
        public string Memo
        {
            get { return GetPropertyValue<string>(nameof(Memo)); }
            set { SetPropertyValue(nameof(Memo), value); }
        }


        [Association, DevExpress.Xpo.Aggregated]
        public XPCollection<SubtitleItem> Subtitles
        {
            get
            {
                return GetCollection<SubtitleItem>(nameof(Subtitles));
            }
        }


        public bool IsInChapter(SubtitleItem item)
        {
            if (item.StartTime >= StartTime && item.StartTime <= EndTime)
            {
                return true;
            }
            return false;
        }

    }

}

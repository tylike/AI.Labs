using DevExpress.ExpressApp.DC;
using DevExpress.Xpo;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using DevExpress.ExpressApp.Model;
using System.Drawing;
using System.ComponentModel;
using DevExpress.Persistent.Base;
using AI.Labs.Module.BusinessObjects.AudioBooks;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    [XafDisplayName("字幕")]
    [XafDefaultProperty(nameof(Index))]
    public class SubtitleItem : SimpleXPObject,ISRT //,IClip
    {
        public SubtitleItem(Session s) : base(s)
        {

        }

        [XafDisplayName("所属章节")]
        [Association]
        public Chapter Chapter
        {
            get { return GetPropertyValue<Chapter>(nameof(Chapter)); }
            set { SetPropertyValue(nameof(Chapter), value); }
        }


        [Association]
        [XafDisplayName("所属视频")]
        public VideoInfo Video
        {
            get { return GetPropertyValue<VideoInfo>(nameof(Video)); }
            set { SetPropertyValue(nameof(Video), value); }
        }

        [XafDisplayName("序号")]
        public int Index
        {
            get { return GetPropertyValue<int>(nameof(Index)); }
            set { SetPropertyValue(nameof(Index), value); }
        }
        
        [XafDisplayName("开始时间")]
        [ModelDefault("DisplayFormat", "{0:HH:mm:ss.fff}")]
        [ToolTip("指在原视频中的开始显示时间")]
        public TimeSpan StartTime
        {
            get { return GetPropertyValue<TimeSpan>(nameof(StartTime)); }
            set { SetPropertyValue(nameof(StartTime), value); }
        }

        [XafDisplayName("结束时间")]
        [ModelDefault("DisplayFormat", "{0:HH:mm:ss.fff}")]
        public TimeSpan EndTime
        {
            get { return GetPropertyValue<TimeSpan>(nameof(EndTime)); }
            set { SetPropertyValue(nameof(EndTime), value); }
        }

        [XafDisplayName("开始时间.实际")]
        public TimeSpan FixedStartTime
        {
            get { return GetPropertyValue<TimeSpan>(nameof(FixedStartTime)); }
            set 
            {
                if (!IsLoading)
                {
                    if (value == TimeSpan.Zero)
                    {

                    }
                }
                SetPropertyValue(nameof(FixedStartTime), value); 
            }
        }

        [XafDisplayName("时长.实际")]
        public int FixedDuration
        {
            get { return (int)(FixedEndTime - FixedStartTime).TotalMilliseconds; }
        }

        [XafDisplayName("结束时间.实际")]
        public TimeSpan FixedEndTime
        {
            get => GetPropertyValue<TimeSpan>(nameof(FixedEndTime));
            set => SetPropertyValue(nameof(FixedEndTime), value);
        }


        [XafDisplayName("时长")]
        public int Duration
        {
            get { return (int)(EndTime - StartTime).TotalMilliseconds; }
        }

        [XafDisplayName("文本.V2")]
        [Size(-1)]
        public string Lines
        {
            get { return GetPropertyValue<string>(nameof(Lines)); }
            set { SetPropertyValue(nameof(Lines), value); }
        }
        string[] words;
        public string[] Words
        {
            get
            {
                if (words == null)
                {
                    words = (Lines + "").Replace("\n", " ").Replace("\r", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(t => !string.IsNullOrEmpty(t)).ToArray();
                }
                return words;
            }
        }

        [Size(-1)]
        [XafDisplayName("文本.V1")]
        public string PlainText
        {
            get { return GetPropertyValue<string>(nameof(PlainText)); }
            set { SetPropertyValue(nameof(PlainText), value); }
        }
        [XafDisplayName("中文文本")]
        [Size(-1)]
        public string CnText
        {
            get { return GetPropertyValue<string>(nameof(CnText)); }
            set { SetPropertyValue(nameof(CnText), value); }
        }
        [XafDisplayName("中文文本.V2")]
        [Size(-1)]
        public string CnTextV2
        {
            get { return GetPropertyValue<string>(nameof(CnTextV2)); }
            set { SetPropertyValue(nameof(CnTextV2), value); }
        }

        string ISRT.Text { get => PlainText; set => PlainText = value; }


        public AudioBookTextAudioItem Audio
        {
            get { return GetPropertyValue<AudioBookTextAudioItem>(nameof(Audio)); }
            set { SetPropertyValue(nameof(Audio), value); }
        }


    }

}

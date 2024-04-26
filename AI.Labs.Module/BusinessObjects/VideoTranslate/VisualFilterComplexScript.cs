using DevExpress.Xpo;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using DevExpress.ExpressApp.Model;
using System.Drawing;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class VisualFilterComplexScript : SimpleXPObject
    {
        public VisualFilterComplexScript(Session s) : base(s)
        {

        }


        public VideoInfo Video
        {
            get { return GetPropertyValue<VideoInfo>(nameof(Video)); }
            set { SetPropertyValue(nameof(Video), value); }
        }


        [Size(-1)]
        [ModelDefault("RowCount","0")]
        public string StartCommand
        {
            get { return GetPropertyValue<string>(nameof(StartCommand)); }
            set { SetPropertyValue(nameof(StartCommand), value); }
        }

        [Size(-1)]
        public string FilterComplexText
        {
            get { return GetPropertyValue<string>(nameof(FilterComplexText)); }
            set { SetPropertyValue(nameof(FilterComplexText), value); }
        }

        [Size(-1)]
        public string Output
        {
            get { return GetPropertyValue<string>(nameof(Output)); }
            set { SetPropertyValue(nameof(Output), value); }
        }
    }

}

using DevExpress.Xpo;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using System.Drawing;
using DevExpress.Persistent.Validation;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class YoutubeChannel : SimpleXPObject
    {
        public YoutubeChannel(Session s) : base(s)
        {

        }
        [RuleRequiredField]
        public string ChannelID
        {
            get { return GetPropertyValue<string>(nameof(ChannelID)); }
            set { SetPropertyValue(nameof(ChannelID), value); }
        }

        public string ChannelName
        {
            get { return GetPropertyValue<string>(nameof(ChannelName)); }
            set { SetPropertyValue(nameof(ChannelName), value); }
        }
        [RuleRequiredField]
        [Size(-1)]
        public string ChannelUrl
        {
            get { return GetPropertyValue<string>(nameof(ChannelUrl)); }
            set { SetPropertyValue(nameof(ChannelUrl), value); }
        }
    }

}

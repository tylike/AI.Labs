//using SubtitlesParser.Classes; 
// 引入SubtitlesParser的命名空间
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
//using SubtitlesParser.Classes.Parsers;

namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class SubtitleViewController : ObjectViewController<ObjectView, SubtitleItem>
    {
        public SubtitleViewController()
        {
            var fixSrt = new SimpleAction(this, "FixSrt", null);
            fixSrt.Caption = "修复字幕";
            fixSrt.Execute += FixSrt_Execute;
            var translateSubtitles = new SimpleAction(this, "TranslateSubtitleItem", null);
            translateSubtitles.Caption = "翻译字幕";
            translateSubtitles.Execute += TranslateSubtitles_Execute;

            var translateSubtitlesV2 = new SimpleAction(this, "TranslateSubtitleItemV2", null);
            translateSubtitlesV2.Caption = "翻译字幕.V2";
            translateSubtitlesV2.Execute += TranslateSubtitles_Execute1;
        }

        private async void FixSrt_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var t = ViewCurrentObject.Video;
            if (t.Model == null)
            {
                throw new UserFriendlyException("请选择模型!");
            }
            var subtitles = ViewCurrentObject.Video.Subtitles.OrderBy(t => t.Index).ToArray();
            foreach (SubtitleItem item in e.SelectedObjects)
            {
                await VideoInfoViewController.FixV1EnglishSRT(t, subtitles, item, this, ObjectSpace);
            }
        }

        private async void TranslateSubtitles_Execute1(object sender, SimpleActionExecuteEventArgs e)
        {
            var t = ViewCurrentObject.Video;
            if (t.Model == null)
            {
                throw new UserFriendlyException("请选择模型!");
            }
            var subtitles = ViewCurrentObject.Video.Subtitles.OrderBy(t => t.Index).ToArray();
            foreach (SubtitleItem item in e.SelectedObjects)
            {
                await VideoInfoViewController.TranslateSubtitle(t, subtitles, item, this, ObjectSpace, false);
            }
        }

        private async void TranslateSubtitles_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var t = ViewCurrentObject.Video;
            if (t.Model == null)
            {
                throw new UserFriendlyException("请选择模型!");
            }
            var subtitles = ViewCurrentObject.Video.Subtitles.OrderBy(t => t.Index).ToArray();
            foreach (SubtitleItem item in e.SelectedObjects)
            {
                await VideoInfoViewController.TranslateSubtitle(t, subtitles, item, this, ObjectSpace, true);
            }
        }
    }
}

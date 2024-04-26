//using SubtitlesParser.Classes; 
// 引入SubtitlesParser的命名空间
using DevExpress.ExpressApp;
using System.Diagnostics;
using DevExpress.ExpressApp.Actions;
using YoutubeExplode.Demo.Cli;
//using SubtitlesParser.Classes.Parsers;

namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class YoutubeVideoViewController : ObjectViewController<ObjectView, YoutubeVideoInfo>
    {
        public YoutubeVideoViewController()
        {
            var download = new SimpleAction(this, "Youtube.Download", null);
            download.Caption = "下载";
            download.SelectionDependencyType = SelectionDependencyType.RequireSingleObject;
            download.Execute += Download_Execute;
        }

        private async void Download_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var p = ViewCurrentObject.VideoInfo;
            await YE.DownloadForUrl(p.VideoURL, p.ProjectPath, t =>
            {
                Debug.WriteLine(t);
            }, ViewCurrentObject);
        }
    }
}

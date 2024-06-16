using DevExpress.Xpo;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using System.Drawing;
using DevExpress.Persistent.Validation;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using System.Collections.Concurrent;
using DevExpress.ExpressApp.Xpo;
using YoutubeExplode.Playlists;
using YoutubeExplode.Demo.Cli;
using System.Diagnostics;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    [NavigationItem("视频翻译")]
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


        public string SiteUserName
        {
            get { return GetPropertyValue<string>(nameof(SiteUserName)); }
            set { SetPropertyValue(nameof(SiteUserName), value); }
        }


        [RuleRequiredField]
        [Size(-1)]
        public string ChannelUrl
        {
            get { return GetPropertyValue<string>(nameof(ChannelUrl)); }
            set { SetPropertyValue(nameof(ChannelUrl), value); }
        }

        [Association, Aggregated]
        public XPCollection<VideoInfo> Videos
        {
            get => GetCollection<VideoInfo>(nameof(Videos));
        }
    }


    public class YoutubeChannelVieController : ObjectViewController<ObjectView, YoutubeChannel>
    {
        public YoutubeChannelVieController()
        {
            var getVideos = new SimpleAction(this, "获取视频", PredefinedCategory.ObjectsCreation);
            getVideos.Execute += GetVideos_Execute;

            var downloadAudios = new SimpleAction(this, "下载音频", PredefinedCategory.ObjectsCreation);
            downloadAudios.Execute += DownloadAudios_Execute;
        }

        private async void DownloadAudios_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var youtube = new YoutubeClient();
            var webVideos = new ConcurrentBag<Video>();

            var existingVideosTitles = ViewCurrentObject.Videos.Select(v => v.Title).ToHashSet();

            var downloadTasks = new List<Task>();

            var list = new List<PlaylistVideo>();

            await foreach (var item in youtube.Channels.GetUploadsAsync(ViewCurrentObject.ChannelUrl))
            {
                list.Add(item);
                if (!existingVideosTitles.Contains(item.Title))
                {
                    downloadTasks.Add(Task.Run(async () =>
                    {
                        var videoId = VideoId.Parse(item.Url);
                        var video = await youtube.Videos.GetAsync(videoId);
                        webVideos.Add(video);
                        await YE.DownloadClosedCaption(video.Url, @"d:\Youtube.小约翰", p => Debug.WriteLine(p), item.Title);
                        await YE.DownloadAudio(video.Url, @"d:\Youtube.小约翰", p => Debug.WriteLine(p), item.Title);
                    }));
                }
            }
            await Task.WhenAll(downloadTasks);

        }

        private async void GetVideos_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var youtube = new YoutubeClient();
            var webVideos = new ConcurrentBag<Video>();

            var existingVideosTitles = ViewCurrentObject.Videos.Select(v => v.Title).ToHashSet();

            var downloadTasks = new List<Task>();

            var list = new List<PlaylistVideo>();

            await foreach (var item in youtube.Channels.GetUploadsAsync(ViewCurrentObject.ChannelUrl))
            {
                list.Add(item);
                if (!existingVideosTitles.Contains(item.Title))
                {
                    downloadTasks.Add(Task.Run(async () =>
                    {
                        var videoId = VideoId.Parse(item.Url);
                        var video = await youtube.Videos.GetAsync(videoId);
                        webVideos.Add(video);
                    }));
                }
            }
            // 等待所有任务完成
            await Task.WhenAll(downloadTasks);

            foreach (var item in webVideos)
            {
                var t = ViewCurrentObject.Videos.FirstOrDefault(t => t.VideoURL == item.Url);
                if (t == null) {
                    var nv = ObjectSpace.CreateObject<VideoInfo>();
                    nv.VideoURL = item.Url;
                    
                    nv.UpdateSourceVideoInfo(item);
                }
            }
            ObjectSpace.CommitChanges();
            
        }
    }
}

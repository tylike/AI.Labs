using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    partial class VideoInfo
    {
        /// <summary>
        /// 获取youtube中的信息
        /// 解析视频章节信息
        /// </summary>
        public void UpdateSourceVideoInfo(YoutubeExplode.Videos.Video mvideo)
        {
            Title = mvideo.Title;
            Description = (mvideo.Description + "").Replace("\n", Environment.NewLine);
            Duration = mvideo.Duration ?? TimeSpan.Zero;
            
            Keywords = string.Join("\n", mvideo.Keywords);
            CnVideoDuration = mvideo.Duration ?? TimeSpan.Zero;

            Like = (int)mvideo.Engagement.LikeCount;
            DisLike = (int)mvideo.Engagement.DislikeCount;
            ViewCount = (int)mvideo.Engagement.ViewCount;
            AverageRating = (decimal)mvideo.Engagement.AverageRating;
            UploadDate = mvideo.UploadDate.LocalDateTime;
            ImageTitle = mvideo.Thumbnails.OrderByDescending(t => t.Resolution.Width).FirstOrDefault()?.Url;

            #region 作者
            var findAuthor = Session.Query<YoutubeChannel>().FirstOrDefault(t => t.ChannelUrl == mvideo.Author.ChannelUrl);
            if (findAuthor == null)
            {
                findAuthor = new YoutubeChannel(Session);
                findAuthor.ChannelID = mvideo.Author.ChannelId;
                findAuthor.ChannelUrl = mvideo.Author.ChannelUrl;
                findAuthor.ChannelName = mvideo.Author.ChannelTitle;
            }
            Channel = findAuthor;
            #endregion

            //ViewCurrentObject.VideoFile = $"{videoId}.{mvideo.Duration.ToString()}.mp4";
            CreateChapters();
        }

        public void CreateChapters()
        {
            var descs = new VideoDescription(Description).ParseChapters();
            if (descs != null && !Chapters.Any())
            {
                foreach (var item in descs)
                {
                    var c = new Chapter(Session);
                    c.Title = item.Name;
                    c.StartTime = item.StartTimespan;
                    c.EndTime = item.EndTimespan ?? Duration;
                    Chapters.Add(c);
                }
            }
        }
    }
}

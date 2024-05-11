//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using System.IO;
using System.Net.Http;

//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public static class YouTubeDownloader
    {
        public static async Task DownloadForProgress(string videoUrl,string outputPath,Action<string> progressChanged = null)
        {
            //string uri = "https://www.youtube.com/watch?v=vPto6XpRq-U";
            var youTube = YouTube.Default;
            //var video = youTube.GetVideo(videoUrl);
            //YouTube 为每个 URL 公开多个视频 -例如，当您更改视频的分辨率时，您实际上是在观看不同的视频。libvideo 支持下载其中的多个：
            var videos = youTube.GetAllVideos(videoUrl);
            //视频的一些信息

            var videoInfos = Client.For(YouTube.Default).GetAllVideosAsync(videoUrl).GetAwaiter().GetResult();
            var resolutions = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.Video).Select(j => j.Resolution);
            var bitRates = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.Audio).Select(j => j.AudioBitrate);
            var unknownFormats = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.None).Select(j => j.Resolution);
            //获取特定的分辨率、比特率、格式

            //var youTube = YouTube.Default; 
            // starting point for YouTube actions
            //var videoInfos = youTube.GetAllVideosAsync(link).GetAwaiter().GetResult();
            var video = videoInfos.First(i => i.Format == VideoFormat.Mp4 && i.Resolution == videoInfos.Max(j => j.Resolution));
            //var minBitrate = videoInfos.First(i => i.AudioBitrate == videoInfos.Min(j => j.AudioBitrate));
            //var audioFormat = videoInfos.First(i => i.AudioFormat == AudioFormat.Aac);
            //var videoFormat = videoInfos.First(i => i.Format == VideoFormat.Mp4);
            //var adaptive = videoInfos.First(i => i.IsAdaptive);

            var outputFileName = Path.Combine(outputPath, video.FullName);
            //File.WriteAllBytes(Path.Combine(outputPath, maxResolution.FullName), await maxResolution.GetBytesAsync());

            var client = new HttpClient();
            long? totalByte = 0;

            using (Stream output = File.OpenWrite(outputFileName))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, video.Uri))
                {
                    totalByte = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result.Content.Headers.ContentLength;
                }
                using (var input = await client.GetStreamAsync(video.Uri))
                {
                    byte[] buffer = new byte[512 * 1024];
                    int read;
                    int totalRead = 0;
                    Console.WriteLine("Download Started");
                    progressChanged?.Invoke("开始下载");
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                        totalRead += read;
                        Console.Write($"\rDownloading {totalRead}/{totalByte} ...");
                        progressChanged?.Invoke($"下载中 {totalRead}/{totalByte}");
                    }
                    progressChanged?.Invoke("下载完成");
                    Console.WriteLine("Download Complete");
                }
            }
            Console.ReadLine();
        }

        public static async Task DownloadVideo2(string uri,string outputPath)
        {
            //string uri = "https://www.youtube.com/watch?v=vPto6XpRq-U";
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(uri);

            //string title = video.Title;
            //var info = video.Info; // (Title,Author,LengthSeconds)
            //string fileExtension = video.FileExtension;
            //string fullName = video.FullName; // same thing as title + fileExtension
            //int resolution = video.Resolution;

            //// etc.
            ////您可以像这样下载它：
            //byte[] bytes = video.GetBytes();
            //var stream = video.Stream();
            ////并将其保存到一个文件中：
            //File.WriteAllBytes(@"C:\" + fullName, bytes);

            //YouTube 为每个 URL 公开多个视频 -例如，当您更改视频的分辨率时，您实际上是在观看不同的视频。libvideo 支持下载其中的多个：
            var videos = youTube.GetAllVideos(uri);
            //视频的一些信息

            var videoInfos = Client.For(YouTube.Default).GetAllVideosAsync(uri).GetAwaiter().GetResult();
            var resolutions = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.Video).Select(j => j.Resolution);
            var bitRates = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.Audio).Select(j => j.AudioBitrate);
            var unknownFormats = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.None).Select(j => j.Resolution);
            //获取特定的分辨率、比特率、格式

            //var youTube = YouTube.Default; 
            // starting point for YouTube actions
            //var videoInfos = youTube.GetAllVideosAsync(link).GetAwaiter().GetResult();
            var maxResolution = videoInfos.First(i => i.Format == VideoFormat.Mp4 && i.Resolution == videoInfos.Max(j => j.Resolution));
            //var minBitrate = videoInfos.First(i => i.AudioBitrate == videoInfos.Min(j => j.AudioBitrate));
            //var audioFormat = videoInfos.First(i => i.AudioFormat == AudioFormat.Aac);
            //var videoFormat = videoInfos.First(i => i.Format == VideoFormat.Mp4);
            //var adaptive = videoInfos.First(i => i.IsAdaptive);
            File.WriteAllBytes(Path.Combine(outputPath, video.FullName), await maxResolution.GetBytesAsync());

        }
        public static async Task DownloadVideoAsync(string videoUrl, string outputPath)
        {
            try
            {
                var youTube = YouTube.Default; // Starting point for YouTube actions
                var video = await youTube.GetVideoAsync(videoUrl); // Gets a Video object with info about the video
                File.WriteAllBytes(Path.Combine(outputPath, video.FullName), await video.GetBytesAsync());
                Console.WriteLine("Video downloaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
            }
            //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            //var youtube = new YoutubeClient();
            //// Get the video ID
            //var videoId = VideoId.TryParse(videoIdOrUrl);// YoutubeClient.ParseVideoId(videoIdOrUrl);
            //if (videoId.HasValue)
            //{
            //    var video = await youtube.Videos.GetAsync(videoId.Value);

            //    // Get the stream manifest
            //    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId.Value);
            //    var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            //    if (streamInfo != null)
            //    {
            //        // Download the video
            //        Console.WriteLine($"Downloading {video.Title}...");
            //        await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{outputPath}{video.Title}.{streamInfo.Container}");

            //        Console.WriteLine("Download complete!");
            //    }
            //    else
            //    {
            //        Console.WriteLine("No suitable video stream found.");
            //    }
            //}
            //else
            //{
            //    throw new Exception("没有解析成功!");
            //}

        }
    }

}

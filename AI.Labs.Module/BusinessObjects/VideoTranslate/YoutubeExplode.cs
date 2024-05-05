using AI.Labs.Module.BusinessObjects.VideoTranslate;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode.Demo.Cli.Utils;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Demo.Cli;

// This demo prompts for video ID and downloads one media stream.
// It's intended to be very simple and straight to the point.
// For a more involved example - check out the WPF demo.
public static class YE
{
    public static async Task<string> DownloadForUrl(string url, string outputPath, Action<double> progressBar,YoutubeVideoInfo info)
    {
        //Console.Title = "YoutubeExplode Demo";

        var youtube = new YoutubeClient();

        // Get the video ID
        //Console.Write("Enter YouTube video ID or URL: ");
        var videoId = VideoId.Parse(url);

        // Get available streams and choose the best muxed (audio + video) stream
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
        var streamInfos = streamManifest.Streams.Where(t=> t is AudioOnlyStreamInfo && t.Container == Container.Mp4);
        if (streamInfos is null)
        {
            // Available streams vary depending on the video and it's possible
            // there may not be any muxed streams at all.
            // See the readme to learn how to handle adaptive streams.
            Console.Error.WriteLine("没有找到指定的子级文件！");
            return string.Empty;
        }
        int i = 1;
        foreach (var s in streamInfos)
        {
            // Download the stream
            var fileName = Path.Combine(outputPath, $"{videoId + "_" + i.ToString()}.{s.Container.Name}");


            using (var progress = new ProgressReporter(progressBar))
                await youtube.Videos.Streams.DownloadAsync(s, fileName, progress);
            i++;
        }

        return "";
        //Console.WriteLine("Done");
        //Console.WriteLine($"Video saved to '{fileName}'");
    }

    public static async Task Download(VideoInfo video)
    {
        video.VideoFile = await Download(video.VideoURL, video.ProjectPath, t =>
        {
            video.DownloadProgress = t;
        },video.Oid.ToString());

        await video.GetVideoScreenSize();

    }

    public static async Task<string> Download(string url, string outputPath,Action<double> progressBar,string mainFileName)
    {
        //Console.Title = "YoutubeExplode Demo";

        var youtube = new YoutubeClient();

        // Get the video ID
        //Console.Write("Enter YouTube video ID or URL: ");
        var videoId = VideoId.Parse(url);

        // Get available streams and choose the best muxed (audio + video) stream
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
        var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
        if (streamInfo is null)
        {
            // Available streams vary depending on the video and it's possible
            // there may not be any muxed streams at all.
            // See the readme to learn how to handle adaptive streams.
            Console.Error.WriteLine("This video has no muxed streams.");
            return string.Empty;
        }
        mainFileName = mainFileName ?? videoId;
        // Download the stream
        var fileName = Path.Combine(outputPath, $"{mainFileName}.{streamInfo.Container.Name}");

        Console.Write(
            $"Downloading stream: {streamInfo.VideoQuality.Label} / {streamInfo.Container.Name}... "
        );

        using (var progress = new ProgressReporter(progressBar))
            await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName, progress);

        return fileName;
        //Console.WriteLine("Done");
        //Console.WriteLine($"Video saved to '{fileName}'");
    }

    public static async Task<string> Download(IVideoStreamInfo streamInfo, string outputPath, string oid)
    {
        //Console.Title = "YoutubeExplode Demo";

        var youtube = new YoutubeClient();
        // Get available streams and choose the best muxed (audio + video) stream
        if (streamInfo is null)
        {
            // Available streams vary depending on the video and it's possible
            // there may not be any muxed streams at all.
            // See the readme to learn how to handle adaptive streams.
            throw new Exception("This video has no streams.");
        }

        // Download the stream
        var fileName = Path.Combine(outputPath, $"{oid}.{streamInfo.Container.Name}");

        Console.Write(
            $"Downloading stream: {streamInfo.VideoQuality.Label} / {streamInfo.Container.Name}... "
        );

        using (var progress = new ProgressReporter(t => { Debug.Write(t); }))
            await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName, progress);

        return fileName;
    }
}

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
    public static async Task<string> Download(string url,string outputPath)
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

        // Download the stream
        var fileName = Path.Combine( outputPath, $"{videoId}.{streamInfo.Container.Name}");

        Console.Write(
            $"Downloading stream: {streamInfo.VideoQuality.Label} / {streamInfo.Container.Name}... "
        );

        using (var progress = new ProgressReporter())
            await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName, progress);

        return fileName;
        //Console.WriteLine("Done");
        //Console.WriteLine($"Video saved to '{fileName}'");
    }

    public static async Task<string> Download(IVideoStreamInfo streamInfo, string outputPath,string oid)
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

        using (var progress = new ProgressReporter())
            await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName, progress);

        return fileName;
    }
}

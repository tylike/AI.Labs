using AI.Labs.Module.BusinessObjects;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace RuntimePlugin;

public class VideoProject
{
    public VideoProject()
    {
        MainVideoTrack = new VideoTrack(nameof(MainVideoTrack));
        VideoTracks.Add(MainVideoTrack);

        MainAudioTrack = new AudioTrack(nameof(MainAudioTrack));
        AudioTracks.Add(MainAudioTrack);

        BackgroundAudioTrack = new AudioTrack(nameof(BackgroundAudioTrack));
        AudioTracks.Add(BackgroundAudioTrack);

        EnSubtitleTrack = new SubtitleTrack(nameof(EnSubtitleTrack));
        SubtitleTracks.Add(EnSubtitleTrack);

        CnSubtitleTrack = new SubtitleTrack(nameof(CnSubtitleTrack));
        SubtitleTracks.Add(CnSubtitleTrack);

        DefaultTextLayerTrack = new TextLayerTrack(nameof(DefaultTextLayerTrack));
        TextLayers.Add(DefaultTextLayerTrack);
    }

    #region tracks
    public VideoTrack MainVideoTrack { get; protected set; }
    public AudioTrack MainAudioTrack { get; protected set; }
    public AudioTrack BackgroundAudioTrack { get; protected set; }
    public SubtitleTrack EnSubtitleTrack { get; protected set; }
    public SubtitleTrack CnSubtitleTrack { get; protected set; }
    public TextLayerTrack DefaultTextLayerTrack { get; protected set; }
    #endregion

    public VideoFile ImportVideo(string video)
    {
        var videoObject = new VideoFile(video, FileUsage.Input);
        Sources.Add(videoObject);
        return videoObject;
    }

    public VideoSegment AddToMainVideoTrack(string videoFile)
    {
        var vo = ImportVideo(videoFile);
        var seg = new VideoSegment(vo);
        MainVideoTrack.AddSegment(seg);
        return seg;
    }

    public AudioFile ImportAudio(string audioFile)
    {
        //[1]adelay=1000|1000[a];[2]adelay=5000|5000[b];[0][a]amix=inputs=2:duration=first[va];[va][b]amix=inputs=2:duration=first
        var audioObject = new AudioFile(audioFile);
        Sources.Add(audioObject);
        return audioObject;
    }
    public AudioSegment AddToMainAudioTrack(string audioFile, TimeSpan time, int durationMS)
    {
        var audio = ImportAudio(audioFile);
        var segment = new AudioSegment(audio);
        MainAudioTrack.Segments.Add(segment);
        segment.Start = time;
        segment.End = time.Add(TimeSpan.FromMilliseconds(durationMS));

        //导入时,直接加入到主音频轨道的指定时间点
        return segment;
    }

    public SubtitleTrack ImportSubtitle(string subtitleFile, bool isCn)
    {
        if (isCn)
        {
            CnSubtitleTrack.AddSubtitle(subtitleFile);
            return CnSubtitleTrack;
        }
        else
        {
            EnSubtitleTrack.AddSubtitle(subtitleFile);
            return EnSubtitleTrack;
        }
    }
    public void ImportImage(string image)
    {

    }
    public List<MediaFile> Sources { get; } = new List<MediaFile>();
    public List<AudioTrack> AudioTracks { get; } = new List<AudioTrack>();
    public List<VideoTrack> VideoTracks { get; } = new List<VideoTrack>();
    public List<SubtitleTrack> SubtitleTracks { get; } = new List<SubtitleTrack>();
    public List<TextLayerTrack> TextLayers { get; } = new List<TextLayerTrack>();

    public void Export(string output, bool overrideExits = true)
    {
        var videos = Sources.OfType<VideoFile>().ToArray();
        var inputVideos = string.Join(" ", videos.Select(t => $" -i {t.FileName}"));
        var audios = Sources.OfType<AudioFile>().ToArray();
        var inputAudios = string.Join(" ", audios.Select(t => $" -i {t.FileName}"));

        //输入的视频文件、音频、输出文件

        var videoLables = new List<MediaSegment>();

        // string.Join(";", MainVideoTrack.Segments.Select(t => t.GetCommand(0)));
        var videoFilterComplex = MainVideoTrack.GetCommand(0, videoLables);
        var videoLastSegments = new MediaSegmentList(videoLables);
        videoLastSegments.VideoLabel = "[vout]";
        videoLastSegments.OutputVideo = true;
        var videoLastCommand = videoLastSegments.GetConcatCommand();
        
        var audioLables = new List<MediaSegment>();
        var audioFilterComplex = MainAudioTrack.GetCommand(0, audioLables);
        var audioLastSegments = new MediaSegmentList(audioLables);
        audioLastSegments.AudioLabel = "[aout]";
        audioLastSegments.OutputAudio = true;
        var audioLastCommand = audioLastSegments.GetConcatCommand();
        
        var filterComplex = $"{videoFilterComplex};\n{audioFilterComplex};\n{videoLastCommand};\n{audioLastCommand};";

        //MainVideoTrack.Segments
        //var videoSegment = new VideoSegment();
        var lines = filterComplex.SplitLines();
        var finalFilterComplex = string.Join("\n", lines.Where(t => !t.Trim().StartsWith("#")));
                
        var args = $"{inputVideos} {inputAudios} -map \"{videoLastSegments.VideoLabel}\" -map \"{audioLastSegments.AudioLabel}\" {output} -progress pipe:1";
        var basePath = Path.GetDirectoryName(output);
        //var task = RunHttp();
        //FFmpegHelper.ExecuteCommand(args, finalFilterComplex, overrideExits,basePath: basePath);
        //https://github.com/cmxl/FFmpeg.NET/blob/master/src/FFmpeg.NET/RegexEngine.cs#L44
        // FFmpeg 完成后，取消所有待处理和后续的操作
        //cancellationTokenSource.Cancel();
        //Task.Delay(10000).Wait();
        // 等待 HttpListener 处理当前的连接请求
        //task.Stop();
        //task.Close();
    }

    //static HttpListener listener;
    //static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    //static HttpListener RunHttp()
    //{
    //    // 创建和配置HttpListener
    //    listener = new HttpListener();
    //    listener.Prefixes.Add("http://127.0.0.1:19012/");
    //    listener.Start();
    //    Console.WriteLine("HTTP Listener started.");
    //    // 在后台线程中处理连接
    //    Task.Run(() => HandleIncomingConnections(cancellationTokenSource.Token));

    //    return listener;
    //}

    //static bool runServer = true;

    //private static void HandleIncomingConnections(CancellationToken token)
    //{
    //    try
    //    {
    //        while (!token.IsCancellationRequested)
    //        {
    //            if (listener.IsListening)
    //            {
    //                IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
    //                // 等待请求到达或取消请求
    //                if (WaitHandle.WaitAny(new[] { result.AsyncWaitHandle, token.WaitHandle }) == 1)
    //                {
    //                    // Cancel was signaled
    //                    break;
    //                }
    //                listener.EndGetContext(result);
    //            }
    //        }
    //    }
    //    catch (HttpListenerException)
    //    {
    //        // HttpListenerException 可能在调用 Close() 时抛出
    //    }
    //}

    //private static void ListenerCallback(IAsyncResult result)
    //{
    //    if (result.AsyncState is HttpListener listener)
    //    {
    //        try
    //        {
    //            // 调用 EndGetContext 来完成异步操作
    //            HttpListenerContext context = listener.EndGetContext(result);
    //            HttpListenerRequest request = context.Request;
    //            HttpListenerResponse response = context.Response;

    //            // 从请求中读取 POST 数据
    //            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
    //            {
    //                string postData = reader.ReadToEnd();
    //                Debug.WriteLine($"http:{postData}");
    //                //Console.WriteLine(postData);
    //            }

    //            // 发送响应
    //            string responseString = "Progress received.";
    //            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
    //            response.ContentLength64 = buffer.Length;
    //            response.OutputStream.Write(buffer, 0, buffer.Length);
    //            response.OutputStream.Close();
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex.Message);
    //        }
    //    }
    //}
}
public class MediaSegmentList : IVideoSegmentSource
{
    public MediaSegmentList(IEnumerable<ISegmentSource> segments)
    {
        this.segments = segments;
        this.Label = string.Join("", segments.Select(t => t.Label));
    }
    IEnumerable<ISegmentSource> segments;
    public string Label { get; }

    public ISegmentSource CreateChildSegment()
    {
        throw new NotImplementedException();
    }

    public bool OutputVideo { get; set; }
    public bool OutputAudio { get; set; }
    public string AudioLabel { get; set; }
    public string VideoLabel { get; set; }

    public string GetConcatCommand()
    {
        if (!this.segments.Any())
        {
            throw new Exception("错误，没有来源segment!");
        }

        var input = string.Join("", segments.Select(t => t.Label));
        var output = "";
        if (OutputVideo)
        {
            output = VideoLabel;
        }

        if (OutputAudio)
        {
            output += AudioLabel;
        }
        return $"{input}concat=n={this.segments.Count()}{output}";
    }

}



using AI.Labs.Module.BusinessObjects;

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
        var args = $"{inputVideos} {inputAudios} {output}";
        if (overrideExits)
        {
            args += " -y";
        }
        // string.Join(";", MainVideoTrack.Segments.Select(t => t.GetCommand(0)));
        var filterComplex = MainVideoTrack.GetCommand(0);
        var audioCommands = MainAudioTrack.GetCommand(0);

        //MainVideoTrack.Segments

        var basePath = Path.GetDirectoryName(output);
        FFmpegHelper.ExecuteCommand(args, filterComplex, basePath: basePath);
    }
}

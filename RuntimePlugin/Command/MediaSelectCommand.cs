namespace RuntimePlugin;

public class MediaSelectCommand : FilterCommand
{
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    //在FFmpeg中，`t`是时间（time）的缩写，它通常用于时间选择或时间基点操作。
    //`select`滤镜基于时间`t`来选择视频流的帧，
    //`between(t, 0, 3)`表示选择0到3秒的时间段。
    public override string GetCommand(int ident)
    {
        return $"{ident.GetIdent()}select='between(t,{Start.TotalSeconds},{End.TotalSeconds})'";
    }
}

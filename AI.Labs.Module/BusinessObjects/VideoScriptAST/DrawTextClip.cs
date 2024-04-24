using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects;

public class DrawTextClip : ClipBase<DrawTextClip>
{
    public DrawTextClip(Session s) : base(s)
    {
    }

    public override string GetClipType()
    {
        return "文字";
    }

    public override string GetOutputLabel()
    {
        throw new NotImplementedException();
    }


    public TextOption Option
    {
        get { return GetPropertyValue<TextOption>(nameof(Option)); }
        set { SetPropertyValue(nameof(Option), value); }
    }
    public string Left { get; set; } = "10";
    public string Top { get; set; } = "10";

    public string Text { get; set; }

    public void SetText(string text)
    {
        Text = FixText(text);
    }
    public void SetDisplayCurrentVideoTime(TimeSpan videoTimeSpan)
    {
        Text = "%{pts\\:hms} / " + FixText(videoTimeSpan.ToString(@"hh\:mm\:ss"));
    }
    public static string FixText(string txt)
    {
        if (txt == null)
            return "";
        return txt.Replace("\\", "\\\\").Replace(":", "\\:").Replace("|","\n");
    }

    public string GetScript()
    {
        var fontSize = Option?.FontSize ?? 24;
        var hasBorder = Option?.HasBoxBorder ?? false;
        var command = $"drawtext=font='微软雅黑': text='{Text}': fontcolor='white':x={Left}: y={Top}: fontsize={fontSize}";
        if (StartTime!= TimeSpan.Zero && EndTime != TimeSpan.Zero)
        {
            command += $": enable='between(t,{StartTime.TotalSeconds},{EndTime.TotalSeconds})'";
        }
        if (hasBorder)
        {
            command += $": box=1";//:boxborderw={BoxBorderWidth}:boxborderh={BoxBorderHeight}:boxbordera={BoxBorderAlpha}";//:color={BoxBorderColor}
        }
        //command += $"";//color={FontColor}:
        return command;
    }
}

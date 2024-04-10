using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects;

public class DrawTextOptions : TextOptionBase
{
    public DrawTextOptions(Session s) : base(s)
    {

    }
    public int X { get; set; } = 10;
    public int Y { get; set; } = 10;
    public string Text { get; set; }

    public TimeSpan? Start { get; set; }
    public TimeSpan? End { get; set; }

    //public DrawTextOptions(string text, int x, int y, TimeSpan start, TimeSpan end)
    //{
    //    Text = text;
    //    this.X = x;
    //    this.Y = y;
    //    Start = start;
    //    End = end;
    //}
    public void SetText(string text)
    {
        Text = FixText(text);
    }
    public void SetDisplayCurrentVideoTime(TimeSpan videoTimeSpan)
    {
        Text = "%{pts\\:hms} / " + FixText( videoTimeSpan.ToString(@"hh\:mm\:ss"));
    }
    string FixText(string txt)
    {
        return txt.Replace("\\", "\\\\").Replace(":", "\\:");
    }

    public override string GetScript()
    {
        var command = $"drawtext=font='微软雅黑': text='{Text}': fontcolor='white':x={X}: y={Y}: fontsize={FontSize}";
        if (Start.HasValue && End.HasValue)
        {
            command += $": enable='between(t,{Start.Value.TotalSeconds},{End.Value.TotalSeconds})'";
        }
        if (HasBoxBorder)
        {
            command += $": box=1";//:boxborderw={BoxBorderWidth}:boxborderh={BoxBorderHeight}:boxbordera={BoxBorderAlpha}";//:color={BoxBorderColor}
        }
        //command += $"";//color={FontColor}:
        return command;
    }

    //public override string GetCommand(int ident)
    //{
    //    var command = $"drawtext=font='微软雅黑': text='{FixText(Text)}': x={X}: y={Y}: fontsize={FontSize}: enable='between(t,{Start.TotalSeconds},{End.TotalSeconds})'";
    //    if (BoxStyle != SubtitleBorderStyle.None)
    //    {
    //        command += $": box=1";//:boxborderw={BoxBorderWidth}:boxborderh={BoxBorderHeight}:boxbordera={BoxBorderAlpha}";//:color={BoxBorderColor}
    //    }
    //    //command += $"";//color={FontColor}:
    //    return command;
    //}
}

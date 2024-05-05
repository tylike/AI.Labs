using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects;


public class DrawTextOption : XPObject //ClipBase<DrawTextClip>
{
    public DrawTextOption(Session s) : base(s)
    {
    }


    public TimeSpan StartTime
    {
        get { return GetPropertyValue<TimeSpan>(nameof(StartTime)); }
        set { SetPropertyValue(nameof(StartTime), value); }
    }
    public TimeSpan EndTime
    {
        get { return GetPropertyValue<TimeSpan>(nameof(EndTime)); }
        set { SetPropertyValue(nameof(EndTime), value); }
    }

    public TextOption Option
    {
        get { return GetPropertyValue<TextOption>(nameof(Option)); }
        set { SetPropertyValue(nameof(Option), value); }
    }

    public string Left
    {
        get { return GetPropertyValue<string>(nameof(Left)); }
        set { SetPropertyValue(nameof(Left), value); }
    } 

    public string Top
    {
        get { return GetPropertyValue<string>(nameof(Top)); }
        set { SetPropertyValue(nameof(Top), value); }
    } 

    [Size(-1)]
    public string Text
    {
        get { return GetPropertyValue<string>(nameof(Text)); }
        set { SetPropertyValue(nameof(Text), value); }
    }


    public void SetText(string text)
    {
        Text = FixText(text);
    }

    public void SetDisplayCurrentVideoTime(TimeSpan videoTimeSpan)
    {
        //%{pts\\:gmtime\\:0\\:%M\\\:%S}
        //%{pts\\:hms}
        var durationText = "";
        if((int)videoTimeSpan.TotalHours>0)
        {
            durationText = $"{FixText(EndTime.ToString(@"hh\:mm\:ss"))}";
        }
        else
        {
            durationText = $"{FixText(EndTime.ToString(@"mm\:ss"))}";
        }
        Text = @$"%{{pts\:gmtime\:0\:%M\\\:%S}} / {durationText}";
    }

    public static string FixText(string txt)
    {
        if (txt == null)
            return "";
        return txt.Replace("\\", "\\\\")
            .Replace(":", "\\:")
            .Replace(",", "，")
            .Replace("'", "’")
            .Replace("|","\n");
    }

    public string GetScript()
    {
        var fontSize = Option?.FontSize ?? 24;
        var hasBorder = Option?.HasBorder ?? false;
        var command = $"drawtext=font='微软雅黑': text='{Text}': fontcolor='{Option?.FontColor.Name.ToLower() ?? "white"}':x={Left}: y={Top}: fontsize={fontSize}";
        if (StartTime!= TimeSpan.Zero && EndTime != TimeSpan.Zero)
        {
            command += $": enable='between(t,{StartTime.TotalSeconds},{EndTime.TotalSeconds})'";
        }
        if (hasBorder)
        {
            command += $": borderw=1";//:boxborderw={BoxBorderWidth}:boxborderh={BoxBorderHeight}:boxbordera={BoxBorderAlpha}";//:color={BoxBorderColor}
            if(Option?.BorderColor != null)
                command += $": bordercolor={Option.BorderColor.Name.ToLower()}";//:color={BoxBorderColor}
        }
        //command += $"";//color={FontColor}:
        return command;
    }
}

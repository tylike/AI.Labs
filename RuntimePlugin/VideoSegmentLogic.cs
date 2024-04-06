using sun.font;

namespace RuntimePlugin;

public static class VideoSegmentLogic
{
    public static MediaSegment ChangeSpeed(this MediaSegment segment, double speed)
    {
        var mc = new MediaAdjustSpeedCommand() { Speed = speed };
        //segment.End = 
        segment.End = TimeSpan.FromSeconds(segment.End.TotalSeconds / speed);
        segment.AddCommand(mc);
        return segment;
    }

    public static MediaSegment DrawText(this MediaSegment seg, DrawTextOptions options)
    {
        seg.AddCommand(options);
        return seg;
    }

    public static MediaSegment DrawText(this MediaSegment seg, string text, int x, int y, TimeSpan? start = null, TimeSpan? end = null)
    {
        var cmd = new DrawTextOptions(text, x, y, start.HasValue ? start.Value : seg.Start, end.HasValue ? end.Value : seg.End);
        seg.AddCommand(cmd);
        return seg;
    }

    

    //public static MediaSegment CreateSelectSegment(this MediaSegment sourceSegment, params FilterCommand[] segments)
    //{
    //    //1.创建了media select command对象，返回，可以修改这个对象
    //    //2.segment，的子级segment
    //    var ms = new MediaSegment();
    //    ms.SourceCommands.AddRange(segments);
    //    return ms;
    //}

    /// <summary>
    /// 为一个源片断创建一个选中片断(其实是子级片断)
    /// </summary>
    /// <param name="sourceSegment"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static MediaSegment CreateSegmentWithSelect(this ISegmentSource sourceSegment, TimeSpan start,TimeSpan end)
    {        
        //创建选中片断命令
        var ms = new MediaSelectCommand();
        ms.Start = start;
        ms.End = end;

        MediaSegment seg = (MediaSegment)sourceSegment.CreateChildSegment();
        seg.Start = start;
        seg.End = end;
        seg.AddCommand(ms);
        return seg;
    }

    public static MediaSegment Delay(this MediaSegment segment, float delay)
    {
        var md = new MediaDelayCommand();
        md.Delay = delay;
        //md.Inputs.Add(segment);
        segment.AddCommand(md);
        return segment;
    }

    public static AddSubtitleCommand AddSubtitles(this MediaSegment segment, string srtFile)
    {
        var asb = new AddSubtitleCommand(srtFile);
        //asb.Inputs.Add(segment);
        return asb;
    }
}

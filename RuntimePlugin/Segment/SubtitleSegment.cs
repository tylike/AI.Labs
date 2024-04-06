namespace RuntimePlugin;

public class SubtitleSegment : MediaSegment 
{
    public SubtitleSegment():base(null)
    {
#warning 没有完成
        //字幕段不需要源
        //这个类应该重新设计。
        //throw new NotImplementedException();
    }

    protected override MediaSegment CreateChildSegment()
    {
        throw new NotImplementedException();
    }
}

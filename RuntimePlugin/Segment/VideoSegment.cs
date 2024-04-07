using javax.xml.transform;

namespace RuntimePlugin;

public class VideoSegment : MediaSegment,IVideoSegmentSource
{
    static int GlobalID = 0;
    public VideoSegment(IEnumerable<IVideoSegmentSource> sources) : this( new MediaSegmentList(sources) )
    {

    }
    public VideoSegment(IVideoSegmentSource source) : base(source)
    {
        Label = $"[v{GlobalID++}]";
    }
    string label;
    public override string Label 
    {
        get 
        {
            if (_commands.Count <= 0)
                return FromSegment.Label;
            return label;           
        }
        set => label = value; 
    }

    protected override MediaSegment CreateChildSegment()
    {
        return new VideoSegment(this);
    }
}

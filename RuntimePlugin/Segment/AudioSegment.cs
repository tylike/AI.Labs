namespace RuntimePlugin;

public class AudioSegment : MediaSegment, IAudioSegmentSource
{
    static int GlobalID = 0 ;
    public AudioSegment(IAudioSegmentSource source) : base(source)
    {
        this.Label = $"[a{GlobalID++}]";
    }

    protected override MediaSegment CreateChildSegment()
    {
        return new AudioSegment(this);
    }
}

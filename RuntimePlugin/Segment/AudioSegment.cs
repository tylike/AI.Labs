namespace RuntimePlugin;

public class AudioSegment : MediaSegment, IAudioSegmentSource
{
    static int GlobalID = 0;
    public AudioSegment(IAudioSegmentSource source) : base(source)
    {
        this.Label = $"[a{GlobalID++}]";
    }
    public override string Label
    {
        get

        {
            if (!_commands.Any() && !ChildSegment.Any())
            {
                return FromSegment.Label;
            }
            return base.Label;
        }
        set => base.Label = value;
    }

    protected override MediaSegment CreateChildSegment()
    {
        return new AudioSegment(this);
    }
}

namespace RuntimePlugin;

public class SubtitleTrack : Track<SubtitleSegment>
{
    public SubtitleTrack(string name) : base(name)
    {

    }
    public void AddSubtitle(string subtiltleFile)
    {
        var file = new SubtitleSegment();
        file.AddCommand(new AddSubtitleCommand(subtiltleFile));
        this.AddSubtitle(file);
    }

    public void AddSubtitle(SubtitleSegment subtitle)
    {
        this.AddSegment(subtitle);
    }
}

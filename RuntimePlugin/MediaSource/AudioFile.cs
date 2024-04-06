namespace RuntimePlugin;

public class AudioFile : MediaFile, IMediaFile, IAudioSegmentSource
{
    static int GlobalID = 0;
    public AudioFile(string fileName) : base(fileName)
    {
        id = GlobalID++;
    }
    int id;
    public override string Label => $"[{id}:a]";

    public override MediaSegment CreateChildSegment()
    {
        return new AudioSegment(this);
    }
}

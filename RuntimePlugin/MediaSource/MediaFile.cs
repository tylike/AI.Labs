namespace RuntimePlugin;

public abstract class MediaFile:ISegmentSource
{
    public string FileName { get; set; }
    public abstract string Label { get; }

    string ISegmentSource.Label => throw new NotImplementedException();

    public MediaFile(string fileName)
    {
        FileName = fileName;
    }

    public abstract MediaSegment CreateChildSegment();

    ISegmentSource ISegmentSource.CreateChildSegment()
    {
        return this.CreateChildSegment();
    }
} 

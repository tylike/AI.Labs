namespace RuntimePlugin;

public abstract class Track<T> where T : MediaSegment
{
    public T AddSegment(T mediaObjectOrSegment)
    {
        Segments.Add(mediaObjectOrSegment);
        return mediaObjectOrSegment;
    }

    public Track(string name)
    {
        this.Name = name;
    }
    public string Name { get; set; }
    public List<T> Segments { get; } = new List<T>();

    public override string ToString()
    {
        return string.Join(";", Segments.Select(t => t.ToString()));
    }

    public string GetCommand(int ident, List<MediaSegment> outLabels)
    {
        var cmds = string.Join(";\n#连接Track.Segment\n", Segments.Where(t => !t.IsEmpty).Select(t => t.GetCommand(ident + 1, outLabels)));
        var ids = ident.GetIdent();
        var rst = @$"{ids}# Track {Name}
{cmds}
";
        return rst;
    }
}

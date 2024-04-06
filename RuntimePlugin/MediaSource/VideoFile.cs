namespace RuntimePlugin;
public class VideoFile : MediaFile, IMediaFile,IVideoSegmentSource
{
    static int GlobalID = 0;

    public FileUsage FileUsage { get; set; }

    public VideoFile(string fileName,FileUsage usage) : base(fileName)
    {
        id = GlobalID++;
        this.FileUsage = usage;
    }

    int id;
    public override string Label => $"[{id}:v]";
    public override MediaSegment CreateChildSegment()
    {
        if (FileUsage == FileUsage.Input)
        {
            return new VideoSegment(this);
        }
        throw new Exception("输出文件通常是不需要创建子级片断的!请检查!");
    }
}
/// <summary>
/// 文件用途
/// </summary>
public enum FileUsage
{
    /// <summary>
    /// 输入
    /// </summary>
    Input,
    /// <summary>
    /// 输出
    /// </summary>
    Output
}

using sun.security.x509;

namespace RuntimePlugin;

/// <summary>
/// 段落实际上是逻辑上的,是指一组命令执行的结果.
/// 开始的segment是video或audio。他经历了虚拟的command,即导入,被生成出来.
/// 每个segment都有一个或多个输入
/// 经过一个或多个命令处理后
/// 输出一个或多个segment
/// </summary>
public abstract class MediaSegment : ISegmentSource
{
    public ISegmentSource FromSegment { get; set; }

    public MediaSegment(ISegmentSource fromSegment)
    {
        this.FromSegment = fromSegment;        
    }

    public int Duration { get => (int)(End - Start).TotalMilliseconds; }

    public virtual string Label { get; set; }

    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }

    /// <summary>
    /// 输入
    /// </summary>
    public List<MediaSegment> Inputs { get; } = new List<MediaSegment>();

    /// <summary>
    /// 一个段落一定是被一组命令生成出来的.
    /// 当前段落的命令
    /// 执行后,生成一个或多个segment
    /// </summary>
    protected List<FilterCommand> _commands { get; } = new List<FilterCommand>();
    public IEnumerable<FilterCommand> Commands => _commands.AsReadOnly();
    public void AddCommand(FilterCommand command)
    {
        command.ParentSegment = this;
        _commands.Add(command);
    }
    public override string ToString()
    {
        return string.Join(",", _commands.Select(t => t.ToString()));
    }
    public List<MediaSegment> ChildSegment { get; } = new List<MediaSegment>();
    protected abstract MediaSegment CreateChildSegment();
    ISegmentSource ISegmentSource.CreateChildSegment()
    {
        var t = CreateChildSegment();
        this.ChildSegment.Add(t);
        return t;
    }

    public virtual string GetCommand(int ident,List<MediaSegment> outLabels)
    {
        if(ChildSegment.Count == 0)
        {
            outLabels.Add(this);
        }

        var ids = ident.GetIdent();

        var pcmd = string.Join(",\n", _commands.Select(t => t.GetCommand(ident+1)));
        
        var selfCmd = @$"{ids}{FromSegment.Label}
{pcmd}
{ids}{Label}";
        if(_commands.Count == 0)
        {
            selfCmd = "";
        }
        var ccmd = string.Join(
            $"{ids};\n", 
            ChildSegment.Select(t => t.GetCommand(ident+1, outLabels))
            );



        return @$"{ids}# Segment:
{selfCmd}
{ccmd}";
    }
}

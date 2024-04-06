namespace RuntimePlugin;

public abstract class FilterCommand
{
    //public static int GlobalID { get; set; } = 0;
    //public TimeSpan Start { get; set; }
    //public TimeSpan End { get; set; }
    public virtual string GetCommand(int ident) => @$"# 命令
{this.GetType().Name}
";
    public MediaSegment ParentSegment { get; set; }

    public override string ToString()
    {
        var cmd = GetCommand(0);
        if (string.IsNullOrEmpty(cmd))
        {
            return CommandName;
        }
        return cmd;
    }
    public virtual string CommandName { get => this.GetType().Name; }
}

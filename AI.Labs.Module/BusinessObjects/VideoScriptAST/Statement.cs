using AI.Labs.Module.BusinessObjects.VideoScriptAST.V3;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.Drawing;

namespace AI.Labs.Module.BusinessObjects;

/// <summary>
/// filter_complex中的语句
/// </summary>
public class Statement : BaseObject
{
    public Statement(Session s) : base(s)
    {

    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        CommandJoinSpliter = ",";
    }

    public string CommandJoinSpliter
    {
        get { return GetPropertyValue<string>(nameof(CommandJoinSpliter)); }
        set { SetPropertyValue(nameof(CommandJoinSpliter), value); }
    }


    [Association]
    public VideoScript VideoScript
    {
        get { return GetPropertyValue<VideoScript>(nameof(VideoScript)); }
        set { SetPropertyValue(nameof(VideoScript), value); }
    }

    [Size(-1)]
    public string InputLables
    {
        get { return GetPropertyValue<string>(nameof(InputLables)); }
        set { SetPropertyValue(nameof(InputLables), value); }
    }


    [Association, DevExpress.Xpo.Aggregated]
    public XPCollection<MediaCommand> Commands
    {
        get
        {
            return GetCollection<MediaCommand>(nameof(Commands));
        }
    }

    public string GetScript()
    {
        return $"{InputLables}{string.Join(CommandJoinSpliter, Commands.Select(t => t.GetScript()))}{OutputLabels}";
    }

    public T CreateCommand<T>(T command) where T : MediaCommand
    {
        var rst = command;
        rst.Index = Commands.Count;
        Commands.Add(rst);
        return rst;
    }

    [Size(-1)]
    public string OutputLabels
    {
        get { return GetPropertyValue<string>(nameof(OutputLabels)); }
        set { SetPropertyValue(nameof(OutputLabels), value); }
    }

    public int Index
    {
        get { return GetPropertyValue<int>(nameof(Index)); }
        set { SetPropertyValue(nameof(Index), value); }
    }

}

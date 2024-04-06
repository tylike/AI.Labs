namespace RuntimePlugin;

public class MediaAdjustSpeedCommand : FilterCommand
{
    public double Speed { get; set; }

    public MediaAdjustSpeedCommand()
    {

    }

    public override string GetCommand(int ident)
    {
        return $"{ident.GetIdent()}setpts=PTS/{Speed.ToString("0.0000")}";
    }
    public override string CommandName => $"调速:{Speed.ToString("0.0000")}";
}

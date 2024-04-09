namespace RuntimePlugin;

public class MediaDelayCommand : FilterCommand
{
    public MediaDelayCommand()
    {
    }
    public float Delay { get; set; }
    public override string GetCommand(int ident)
    {
        return $"trim=end={Delay.ToString("0.000")}";
    }
}

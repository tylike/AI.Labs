using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects;

public class SelectMediaCommand : MediaCommand
{
    public SelectMediaCommand(Session s):base(s)
    {
        
    }

    public TimeSpan Start
    {
        get { return GetPropertyValue<TimeSpan>(nameof(Start)); }
        set { SetPropertyValue(nameof(Start), value); }
    }

    public TimeSpan End
    {
        get { return GetPropertyValue<TimeSpan>(nameof(End)); }
        set { SetPropertyValue(nameof(End), value); }
    }


    public decimal TargetSpeed
    {
        get { return GetPropertyValue<decimal>(nameof(TargetSpeed)); }
        set { SetPropertyValue(nameof(TargetSpeed), value); }
    }



    public override string GetScript()
    {
        var cmd = $"trim={Start.TotalSeconds}:{End.TotalSeconds}";
        if (TargetSpeed != 0)
        {
            cmd += $",setpts={TargetSpeed.ToString("0.000000")}*PTS";
        }
        else
        {
            cmd += ",setpts=PTS-STARTPTS";
        }
        return cmd;
    }
}

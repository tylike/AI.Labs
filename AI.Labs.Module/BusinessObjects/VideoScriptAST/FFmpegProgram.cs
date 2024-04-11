using com.sun.tools.javadoc;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using FFMpegCore.Enums;
using Microsoft.CodeAnalysis.Scripting;
using System.Drawing;
using System.Windows.Forms;

namespace AI.Labs.Module.BusinessObjects;



public class Label : BaseObject
{
    public Label(Session s) : base(s)
    {

    }

    public string Name
    {
        get { return GetPropertyValue<string>(nameof(Name)); }
        set { SetPropertyValue(nameof(Name), value); }
    }
}

public abstract class MediaCommand : BaseObject
{
    [Association]
    public Statement Statement
    {
        get { return GetPropertyValue<Statement>(nameof(Statement)); }
        set { SetPropertyValue(nameof(Statement), value); }
    }

    public MediaCommand(Session s) : base(s)
    {
    }

    public int Index
    {
        get { return GetPropertyValue<int>(nameof(Index)); }
        set { SetPropertyValue(nameof(Index), value); }
    }
    public abstract string GetScript();

}

public class ChangeSpeed : MediaCommand
{
    public ChangeSpeed(Session s) : base(s)
    {

    }

    public MediaType MediaType
    {
        get { return GetPropertyValue<MediaType>(nameof(MediaType)); }
        set { SetPropertyValue(nameof(MediaType), value); }
    }

    public decimal TargetSpeed
    {
        get { return GetPropertyValue<decimal>(nameof(TargetSpeed)); }
        set { SetPropertyValue(nameof(TargetSpeed), value); }
    }
    public override string GetScript()
    {
        switch (MediaType)
        {
            case MediaType.Video:
                return $"setpts=PTS*{TargetSpeed.ToString("0.0000")}";
            case MediaType.Audio:
                return $"asetpts=PTS*{TargetSpeed.ToString("0.0000")}";
            default:
                throw new NotImplementedException();
        }
        //return $"setpts=PTS/{TargetSpeed.ToString("0.0000")}";
    }
}

public abstract class TextOptionBase : MediaCommand
{
    public TextOptionBase(Session s) : base(s)
    {

    }

    //public string FontPath { get; set; } = FFmpegGlobalSettings.DefaultFont;
    //public Color BoxBorderColor { get; set; } = Color.White;

    public Color BoxBorderColor
    {
        get { return GetPropertyValue<Color>(nameof(BoxBorderColor)); }
        set { SetPropertyValue(nameof(BoxBorderColor), value); }
    }

    //public Color FontColor { get; set; } = Color.White;
    public Color FontColor
    {
        get { return GetPropertyValue<Color>(nameof(FontColor)); }
        set { SetPropertyValue(nameof(FontColor), value); }
    }

    //public bool BoxStyle { get; set; } = SubtitleBorderStyle.Box;

    public bool HasBoxBorder
    {
        get { return GetPropertyValue<bool>(nameof(HasBoxBorder)); }
        set { SetPropertyValue(nameof(HasBoxBorder), value); }
    }

    //public int BoxBorderWidth { get; set; } = 5;
    //public int BoxBorderHeight { get; set; } = 1;
    //public double BoxBorderAlpha { get; set; } = 1.0;
    //public int FontSize { get; set; } = 24;

    public int FontSize
    {
        get { return GetPropertyValue<int>(nameof(FontSize)); }
        set { SetPropertyValue(nameof(FontSize), value); }
    }

}


public class ConcatMedia : MediaCommand
{
    public ConcatMedia(Session s) : base(s)
    {

    }


    public bool ConcatVideo
    {
        get { return GetPropertyValue<bool>(nameof(ConcatVideo)); }
        set { SetPropertyValue(nameof(ConcatVideo), value); }
    }

    public bool ConcatAudio
    {
        get { return GetPropertyValue<bool>(nameof(ConcatAudio)); }
        set { SetPropertyValue(nameof(ConcatAudio), value); }
    }




    public int MediaCount
    {
        get { return GetPropertyValue<int>(nameof(MediaCount)); }
        set { SetPropertyValue(nameof(MediaCount), value); }
    }

    [Size(-1)]
    public string AddationCommand
    {
        get { return GetPropertyValue<string>(nameof(AddationCommand)); }
        set { SetPropertyValue(nameof(AddationCommand), value); }
    }


    public override string GetScript()
    {
        var rst = $"concat=n={MediaCount}:a={(ConcatAudio ? "1" : "0")}:v={(ConcatVideo ? "1" : "0")}";
        if (!string.IsNullOrEmpty(AddationCommand))
        {
            rst += $"{AddationCommand}";
        }
        return rst;
    }
}
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.Drawing;

namespace AI.Labs.Module.BusinessObjects;

public class TextOption : BaseObject
{
    public TextOption(Session s) : base(s)
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
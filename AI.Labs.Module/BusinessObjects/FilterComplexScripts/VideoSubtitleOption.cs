﻿namespace AI.Labs.Module.BusinessObjects.FilterComplexScripts
{
    // 枚举定义
    public enum TextAlignment
    {
        BottomLeft = 1,
        BottomCenter = 2,
        BottomRight = 3,
        MiddleLeft = 4,
        MiddleCenter = 5,
        MiddleRight = 6,
        TopLeft = 7,
        TopCenter = 8,
        TopRight = 9
    }

    public enum BorderStyle
    {
        Normal = 1,
        NoBorderShadow = 3
    }
    public class VideoSubtitleOption
    {
        public string SrtFileName { get; set; }
        public int? FontSize { get; set; }
        public string FontName { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public string OutlineColor { get; set; }
        public string BackColor { get; set; }
        public bool? Bold { get; set; }
        public bool? Italic { get; set; }
        public bool? Underline { get; set; }
        public bool? StrikeOut { get; set; }
        public int? Outline { get; set; }
        public int? Shadow { get; set; }
        public int? MarginL { get; set; }
        public int? MarginR { get; set; }
        public int? MarginV { get; set; }
        public TextAlignment? Alignment { get; set; }
        public BorderStyle? BorderStyle { get; set; }
        public string GetStyle()
        {
            var styleOptions = new List<string>();

            if (FontSize.HasValue)
                styleOptions.Add($"Fontsize={FontSize.Value}");

            if (!string.IsNullOrEmpty(FontName))
                styleOptions.Add($"Fontname={FontName}");

            if (!string.IsNullOrEmpty(PrimaryColor))
                styleOptions.Add($"PrimaryColour={PrimaryColor}");
            if (!string.IsNullOrEmpty(SecondaryColor))
                styleOptions.Add($"SecondaryColour={SecondaryColor}");
            if (!string.IsNullOrEmpty(OutlineColor))
                styleOptions.Add($"OutlineColour={OutlineColor}");
            if (!string.IsNullOrEmpty(BackColor))
                styleOptions.Add($"BackColour={BackColor}");
            if (Bold.HasValue)
                styleOptions.Add($"Bold={(Bold.Value ? -1 : 0)}");
            if (Italic.HasValue)
                styleOptions.Add($"Italic={(Italic.Value ? -1 : 0)}");
            if (Underline.HasValue)
                styleOptions.Add($"Underline={(Underline.Value ? -1 : 0)}");
            if (StrikeOut.HasValue)
                styleOptions.Add($"StrikeOut={(StrikeOut.Value ? -1 : 0)}");
            if (Outline.HasValue)
                styleOptions.Add($"Outline={Outline.Value}");
            if (Shadow.HasValue)
                styleOptions.Add($"Shadow={Shadow.Value}");
            if (MarginL.HasValue)
                styleOptions.Add($"MarginL={MarginL.Value}");
            if (MarginR.HasValue)
                styleOptions.Add($"MarginR={MarginR.Value}");
            if (MarginV.HasValue)
                styleOptions.Add($"MarginV={MarginV.Value}");
            if (Alignment.HasValue)
                styleOptions.Add($"Alignment={(int)Alignment.Value}");
            if (BorderStyle.HasValue)
                styleOptions.Add($"BorderStyle={(int)BorderStyle.Value}");

            return string.Join(",", styleOptions);
        }

        public string GetScript()
        {
            // Assuming DrawTextOption.FixText is a method to sanitize SrtFileName
            return $"subtitles='{DrawTextOption.FixText(SrtFileName)}':force_style='{GetStyle()}'";
        }
    }
}

//5.Style Lines, [v4 + Styles] section
//Styles define the appearance and position of subtitles. All styles used by the script are are defined by a Style line in the script.
//Any of the the settings in the Style, (except shadow / outline type and depth) can overridden by control codes in the subtitle text.
//The fields which appear in each Style definition line are named in a special line with the line type “Format:”. The Format line must appear before any Styles - because it defines how SSA will interpret the Style definition lines. The field names listed in the format line must be correctly spelled! The fields are as follows:
//Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
//The format line allows new fields to be added to the script format in future, and yet allow old versions of the software to read the fields it recognises - even if the field order is changed.
//Field 1:      Name.The name of the Style. Case sensitive. Cannot include commas.
//Field 2:      Fontname.The fontname as used by Windows. Case-sensitive.
//Field 3:      Fontsize.
//Field 4:      PrimaryColour.A long integer BGR (blue-green-red)  value.ie.the byte order in the hexadecimal equivelent of this number is BBGGRR
//                 This is the colour that a subtitle will normally appear in.
//Field 5:      SecondaryColour.A long integer BGR (blue-green-red)  value.ie.the byte order in the hexadecimal equivelent of this number is BBGGRR
//                 This colour may be used instead of the Primary colour when a subtitle is automatically shifted to prevent an onscreen collsion, to distinguish the different subtitles.
//Field 6:      OutlineColor(TertiaryColour).A long integer BGR (blue-green-red)  value.ie.the byte order in the hexadecimal equivelent of this number is BBGGRR
//                 This colour may be used instead of the Primary or Secondary colour when a subtitle is automatically shifted to prevent an onscreen collsion, to distinguish the different subtitles.
//Field 7:     BackColour.This is the colour of the subtitle outline or shadow, if these are used. A long integer BGR (blue-green-red)  value.ie.the byte order in the hexadecimal equivelent of this number is BBGGRR.
//Field 4-7:  The color format contains the alpha channel, too. (AABBGGRR)
//Field 8:      Bold.This defines whether text is bold (true) or not(false). -1 is True, 0 is False.This is independant of the Italic attribute - you can have have text which is both bold and italic.
//Field 9:      Italic.This defines whether text is italic (true) or not(false). -1 is True, 0 is False.This is independant of the bold attribute - you can have have text which is both bold and italic.
//Field 9.1:  Underline. [-1 or 0]
//Field 9.2:  Strikeout. [-1 or 0]
//Field 9.3:  ScaleX.Modifies the width of the font. [percent]
//Field 9.4:  ScaleY.Modifies the height of the font. [percent]
//Field 9.5:  Spacing.Extra space between characters. [pixels]
//Field 9.6:  Angle.The origin of the rotation is defined by the alignment. Can be a floating point number. [degrees]
//Field 10:    BorderStyle. 1 = Outline + drop shadow, 3=Opaque box
//Field 11:    Outline.If BorderStyle is 1, then this specifies the width of the outline around the text, in pixels.
//Values may be 0, 1, 2, 3 or 4.
//Field 12:    Shadow.If BorderStyle is 1, then this specifies the depth of the drop shadow behind the text, in pixels. Values may be 0, 1, 2, 3 or 4. Drop shadow is always used in addition to an outline - SSA will force an outline of 1 pixel if no outline width is given.
//Field 13:    Alignment.This sets how text is "justified" within the Left/Right onscreen margins, and also the vertical placing. Values may be 1=Left, 2=Centered, 3=Right. Add 4 to the value for a "Toptitle". Add 8 to the value for a "Midtitle".
//eg. 5 = left-justified toptitle
//Field 13:   Alignment, but after the layout of the numpad (1-3 sub, 4-6 mid, 7-9 top).
//Field 14:    MarginL.This defines the Left Margin in pixels. It is the distance from the left-hand edge of the screen.The three onscreen margins (MarginL, MarginR, MarginV) define areas in which the subtitle text will be displayed.
//Field 15:    MarginR.This defines the Right Margin in pixels. It is the distance from the right-hand edge of the screen. The three onscreen margins (MarginL, MarginR, MarginV) define areas in which the subtitle text will be displayed.
//Field 16:    MarginV.This defines the vertical Left Margin in pixels.
//For a subtitle, it is the distance from the bottom of the screen.
//For a toptitle, it is the distance from the top of the screen.
//For a midtitle, the value is ignored - the text will be vertically centred
 //Field 17:    AlphaLevel.This defines the transparency of the text. SSA does not use this yet.
 //Field 17:   Not present in ASS.
 //Field 18:    Encoding.This specifies the font character set or encoding and on multi-lingual Windows installations it provides access to characters used in multiple than one languages. It is usually 0 (zero) for English(Western, ANSI) Windows.
 //                 When the file is Unicode, this field is useful during file format conversions.

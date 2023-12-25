namespace AI.Labs.Module.BusinessObjects.AudioBooks
{
    public class ParseResult
    {
        public string Text;
        public string Spreaker;
        public string SpreakBefore;
        public string SpreakContent;
        public string SpreakAfter;
        public string Emotion;
        public int Volume;
    }

    ///// <summary>
    ///// 某章节中的一个段落
    ///// </summary>
    //public class AudioBookParagraphItem : XPObject
    //{
    //    public AudioBookParagraphItem(Session s) : base(s)
    //    {

    //    }
    //    [Association]
    //    public AudioBook AudioBook
    //    {
    //        get { return GetPropertyValue<AudioBook>(nameof(AudioBook)); }
    //        set { SetPropertyValue(nameof(AudioBook), value); }
    //    }

    //    public int Index
    //    {
    //        get { return GetPropertyValue<int>(nameof(Index)); }
    //        set { SetPropertyValue(nameof(Index), value); }
    //    }


    //    [Size(-1)]
    //    public string Text
    //    {
    //        get { return GetPropertyValue<string>(nameof(Text)); }
    //        set { SetPropertyValue(nameof(Text), value); }
    //    }
    //}
}

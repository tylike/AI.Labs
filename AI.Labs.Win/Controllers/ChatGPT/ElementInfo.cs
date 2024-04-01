namespace Browser
{
    public class ElementInfo
    {
        public string HTML { get; set; }
        public string Text { get; set; }
        public string Role { get; set; }
    }
    public class MessageInfo
    {
        public string Message { get; set; }
        public List<ElementInfo> History { get; set; }
    }
}

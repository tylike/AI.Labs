using System.Text;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间

//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public static class StringBuilderExtendesion
    {
        public static void AppendP(this StringBuilder sb, string text)
        {
            sb.Append("<p>");
            sb.Append(text);
            sb.Append("</p>");
        }
    }

}

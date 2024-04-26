//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public static class StringExtensions
    {
        //c#中，如何将一个字符串按长度分成N个组
        public static IEnumerable<string> SplitIntoGroups(this string input, int groupSize)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input string cannot be null or empty.");

            if (groupSize <= 0)
                throw new ArgumentException("Group size must be greater than zero.");

            for (int i = 0; i < input.Length; i += groupSize)
            {
                yield return input.Substring(i, Math.Min(groupSize, input.Length - i));
            }
        }
    }

}

//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public static class ListExtensions
    {
        // 分页扩展方法
        public static IEnumerable<IEnumerable<T>> Paginate<T>(this IEnumerable<T> source, int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }

            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return GetPage(enumerator, pageSize);
                }
            }
        }

        // 用于从迭代器中获取单个分页的帮助器方法
        private static IEnumerable<T> GetPage<T>(IEnumerator<T> source, int pageSize)
        {
            do
            {
                yield return source.Current;
            }
            while (--pageSize > 0 && source.MoveNext());
        }
    }

}

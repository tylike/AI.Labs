//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
using DevExpress.XtraSpreadsheet.Commands;
using System.Diagnostics;
using System.Text;

namespace AI.Labs.Module.BusinessObjects
{
    public static class LinqExtensions
    {
        public static void Log(this StringBuilder log,string text,bool isError = false,bool showDebug = false)
        {
            if (showDebug)
            {
                Debug.Write(text);
            }
            else
            {
                Debug.Write(".");
            }

            if(string.IsNullOrEmpty(text))
            {
                return;
            }
            if(isError)
            {
                text = $"!!!: {text}";
            }

            log.AppendLine(text);
        }

        public static IEnumerable<IEnumerable<T>> GroupWhile<T>(
            this IEnumerable<T> source, Func<T, T, bool> predicate)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break;
                }

                List<T> currentGroup = new List<T> { iterator.Current };

                while (iterator.MoveNext())
                {
                    if (predicate(currentGroup.Last(), iterator.Current))
                    {
                        // 如果当前元素满足条件，添加到当前分组
                        currentGroup.Add(iterator.Current);
                    }
                    else
                    {
                        // 如果不满足条件，返回当前分组并开始新的分组
                        yield return currentGroup;
                        currentGroup = new List<T> { iterator.Current };
                    }
                }

                // 返回最后一个分组
                yield return currentGroup;
            }
        }
    }
}

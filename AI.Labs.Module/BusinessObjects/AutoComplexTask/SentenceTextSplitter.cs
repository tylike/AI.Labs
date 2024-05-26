using OpenAI.Tokenizer.GPT3;
using System.Text;
using System.Text.RegularExpressions;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public class SentenceTextSplitter
    {
        private static readonly char[] sentenceEndingChars = new char[] { '。', '？', '！', '.', '?', '!' };

        /// <summary>
        /// 按句子和token limit来分段
        /// </summary>
        /// <param name="text">输入的文本</param>
        /// <param name="tokenLimit">每段的Token数量上限</param>
        /// <returns>分段后的文本列表</returns>
        public static List<string> SegmentText(string text, int tokenLimit)
        {
            List<string> segments = new List<string>();
            StringBuilder currentSegment = new StringBuilder();
            int currentTokenCount = 0;

            // 使用正则表达式按句子分割
            string[] sentences = Regex.Split(text, @"(?<=[。？！.!?])");

            // 如果行尾没有标点符号，认为一整行是一个句子
            List<string> completeSentences = new List<string>();
            StringBuilder currentLine = new StringBuilder();
            foreach (string sentence in sentences)
            {
                if (string.IsNullOrWhiteSpace(sentence)) continue;

                currentLine.Append(sentence.Trim() + " ");
                if (IsSentenceEnding(sentence.Trim()[^1]))
                {
                    completeSentences.Add(currentLine.ToString().Trim());
                    currentLine.Clear();
                }
            }
            if (currentLine.Length > 0)
            {
                completeSentences.Add(currentLine.ToString().Trim());
            }

            foreach (string sentence in completeSentences)
            {
                if (string.IsNullOrWhiteSpace(sentence)) continue;

                int sentenceTokenCount = TokenizerGpt3.TokenCount(sentence);

                // 如果当前段落加上这一句会超过token上限,则先处理当前段落
                if (currentTokenCount + sentenceTokenCount > tokenLimit)
                {
                    if (currentSegment.Length > 0)
                    {
                        segments.Add(currentSegment.ToString().Trim());
                        currentSegment.Clear();
                        currentTokenCount = 0;
                    }

                    // 如果单个句子本身超过Token限制则直接添加
                    if (sentenceTokenCount > tokenLimit)
                    {
                        segments.Add(sentence.Trim());
                        continue;
                    }
                }

                // 添加完整句子到当前段落
                currentSegment.Append(sentence.Trim() + " ");
                currentTokenCount += sentenceTokenCount;
            }

            // 处理最后一个段落
            if (currentSegment.Length > 0)
            {
                segments.Add(currentSegment.ToString().Trim());
            }

            return segments;
        }

        private static bool IsSentenceEnding(char c)
        {
            return Array.IndexOf(sentenceEndingChars, c) >= 0;
        }
    }

}

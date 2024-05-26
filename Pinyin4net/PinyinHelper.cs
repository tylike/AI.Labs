/**
 * Copyright (c) 2012 Yang Kuang
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;
using Pinyin4net.Format;

namespace Pinyin4net
{
    /// <summary>
    /// Summary description for PinyinHelper.
    /// </summary>
    public class PinyinHelper
    {
        /// <summary>
        /// 汉字char的16进制为主键，拼音为值，但有多个拼音，用逗号分隔。最后一位为数字表示声调。
        /// </summary>
        public static Dictionary<string, string> Dictionary;

        public static List<string[]> FindHomophone(string 词, bool 音调也需要相同)
        {
            var rst = new List<string[]>();
            foreach (var x in 词)
            {
                var 同音字 = FindHomophone(x, 音调也需要相同);
                rst.Add(同音字.ToArray());
            }
            return rst;
        }

        //输入一个汉字，给出同音字
        public static List<string> FindHomophone(char 字, bool 音调也需要相同)
        {
            string hexKey = String.Format("{0:X}", (int)字).ToUpper();

            if (!Dictionary.TryGetValue(hexKey, out string pinyinList))
            {
                return new List<string>(); // 如果字典中没有找到该字，返回空列表
            }

            string[] pinyins = pinyinList.Split(',');

            var homophones = new List<string>();

            foreach (var kvp in Dictionary)
            {
                string[] potentialPinyins = kvp.Value.Split(',');

                foreach (string pinyin in pinyins)
                {
                    string basePinyin = pinyin.Substring(0, pinyin.Length - 1);
                    string tone = pinyin.Substring(pinyin.Length - 1);

                    foreach (string potentialPinyin in potentialPinyins)
                    {
                        string potentialBasePinyin = potentialPinyin.Substring(0, potentialPinyin.Length - 1);
                        string potentialTone = potentialPinyin.Substring(potentialPinyin.Length - 1);

                        if (basePinyin == potentialBasePinyin && (!音调也需要相同 || tone == potentialTone))
                        {
                            char homophoneChar = (char)Convert.ToInt32(kvp.Key, 16);
                            homophones.Add(homophoneChar.ToString());
                            break;
                        }
                    }
                }
            }

            return homophones.Distinct().ToList(); // 去重并返回同音字列表
        }


        /// <summary>
        /// We don't need any instances of this object.
        /// </summary>
        private PinyinHelper()
        {
        }

        /// <summary>
        /// Load unicode-pinyin map to memery while this class first use.
        /// </summary>
        static PinyinHelper()
        {
            Dictionary = new Dictionary<string, string>();
            var name = "Pinyin4net.Resources.unicode_to_hanyu_pinyin.xml";
            var assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                throw new InvalidOperationException();
            }
            var doc = XDocument.Load(stream);
            var query =
                from item in doc.Root?.Descendants("item")
                select new
                {
                    Unicode = (string)item.Attribute("unicode"),
                    Hanyu = (string)item.Attribute("hanyu")
                };
            foreach (var item in query)
                if (item.Hanyu.Length > 0)
                    Dictionary.Add(item.Unicode, item.Hanyu);
        }

        /// <summary>
        /// Get all Hanyu pinyin of a single Chinese character (both
        /// Simplified Chinese and Traditional Chinese).
        /// 
        /// This function is same with: 
        ///     ToHanyuPinyinStringArray(ch, new HanyuPinyinOutputFormat());
        ///
        /// For example, if the input is '偻', the return will be an array with 
        /// two Hanyu pinyin strings: "lou2", "lv3". If the input is '李', the
        /// return will be an array with one Hanyu pinyin string: "li3".
        /// </summary>
        /// <param name="ch">The given Chinese character</param>
        /// <returns>A string array contains all Hanyu pinyin presentations; return 
        /// null for non-Chinese character.</returns>
        public static string[] ToHanyuPinyinStringArray(char ch)
        {
            return ToHanyuPinyinStringArray(ch, new HanyuPinyinOutputFormat());
        }

        /// <summary>
        /// Get all Hanyu pinyin of a single Chinese character (both
        /// Simplified Chinese and Traditional Chinese).
        /// </summary>
        /// <param name="ch">The given Chinese character</param>
        /// <param name="format">The given output format</param>
        /// <returns>A string array contains all Hanyu pinyin presentations; return 
        /// null for non-Chinese character.</returns>
        public static string[] ToHanyuPinyinStringArray(
            char ch, HanyuPinyinOutputFormat format)
        {
            return GetFomattedHanyuPinyinStringArray(ch, format);
        }

        #region Private Functions
        private static string[] GetFomattedHanyuPinyinStringArray(
            char ch, HanyuPinyinOutputFormat format)
        {
            string[] unformattedArr = GetUnformattedHanyuPinyinStringArray(ch);
            if (null != unformattedArr)
            {
                for (int i = 0; i < unformattedArr.Length; i++)
                {
                    unformattedArr[i] = PinyinFormatter.FormatHanyuPinyin(unformattedArr[i], format);
                }
            }

            return unformattedArr;
        }

        private static string[] GetUnformattedHanyuPinyinStringArray(char ch)
        {
            string code = String.Format("{0:X}", (int)ch).ToUpper();
#if DEBUG
            Console.WriteLine(code);
#endif
            if (Dictionary.ContainsKey(code))
            {
                return Dictionary[code].Split(',');
            }

            return null;
        }
        #endregion
    }
}

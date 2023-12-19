using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Edge_tts_sharp
{
    public class Tools
    {
        public static T StringToJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        /// <summary>
        /// 获取嵌入文本资源,程序集.目录名.文件名（而不是\）
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static string GetEmbedText(string res)
        {
            string result = string.Empty;

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(res))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        result = string.Empty;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}

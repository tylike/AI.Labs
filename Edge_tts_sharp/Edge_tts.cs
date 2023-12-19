using Edge_tts_sharp.Model;
using System;
using System.Resources;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebSocketSharp;


namespace Edge_tts_sharp
{
    public class Edge_tts
    {
        static string GetGUID()
        {
            return Guid.NewGuid().ToString().Replace("-","");
        }
        /// <summary>
        /// 讲一个浮点型数值转换为百分比数值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FromatPercentage(double input)
        {
            string output;

            if (input < 0)
            {
                output = input.ToString("+#;-#;0") + "%";
            }
            else
            {
                output = input.ToString("+#;-#;0") + "%";
            }
            return output;
        }
        static string ConvertToAudioFormatWebSocketString(string outputformat)
        {
            return "Content-Type:application/json; charset=utf-8\r\nPath:speech.config\r\n\r\n{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"false\"},\"outputFormat\":\"" + outputformat + "\"}}}}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang">输出语言</param>
        /// <param name="voice">音源名</param>
        /// <param name="rate">语速，-100% - 100% 之间的值，无需传递百分号</param>
        /// <param name="text"></param>
        /// <returns></returns>
        static string ConvertToSsmlText(string lang, string voice, int rate, string text)
        {
            return $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis'  xml:lang='{lang}'><voice name='{voice}'><prosody pitch='+0Hz' rate ='{FromatPercentage(rate)}' volume='+0%'>{text}</prosody></voice></speak>";
        }
        static string ConvertToSsmlWebSocketString(string requestId, string lang, string voice,int rate, string msg)
        {
            return $"X-RequestId:{requestId}\r\nContent-Type:application/ssml+xml\r\nPath:ssml\r\n\r\n{ConvertToSsmlText(lang, voice, rate, msg)}";
        }
        /// <summary>
        /// 调用微软Edge接口，文字转语音
        /// </summary>
        /// <param name="msg">文本内容</param>
        /// <param name="voice">音频名称</param>
        /// <param name="rate">（可选）调整语速，是一个-100 - 100的数值</param>
        /// <param name="savePath">（可选）保存音频到指定路径</param>
        public static void PlayText(string msg, eVoice voice, int rate = 0, string savePath = "")
        {
            var binary_delim = "Path:audio\r\n";
            var sendRequestId = GetGUID();
            var binary = new List<byte>();

            var wss = new Wss("wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken=6A5AA1D4EAFF4E9FB37E23D68491D6F4");
            wss.OnMessage += (sender, e) =>
            {
                if (e.IsText)
                {
                    var data = e.Data;
                    var requestId = Regex.Match(data, @"X-RequestId:(?<requestId>.*?)\r\n").Groups["requestId"].Value;
                    if (data.Contains("Path:turn.start"))
                    {
                        // start of turn, ignore. 开始信号，不用处理
                    }
                    else if (data.Contains("Path:turn.end"))
                    {
                        // end of turn, close stream. 结束信号，可主动关闭socket
                        // dataBuffers[requestId] = null;
                        // 不要跟着MsEdgeTTS中用上面那句，音频发送完毕后，最后还会收到一个表示音频结束的文本信息
                        wss.Close();
                    }
                    else if (data.Contains("Path:response"))
                    {
                        // context response, ignore. 响应信号，无需处理
                    }
                    else
                    {
                        // 未知错误，通常不会发生
                    }
                    Console.WriteLine(e.Data);
                }
                else if (e.IsBinary)
                {
                    var data = e.RawData;
                    var requestId = Regex.Match(e.Data, @"X-RequestId:(?<requestId>.*?)\r\n").Groups["requestId"].Value;
                    if (data[0] == 0x00 && data[1] == 0x67 && data[2] == 0x58)
                    {
                        // Last (empty) audio fragment. 空音频片段，代表音频发送结束
                    }
                    else
                    {
                        var index = Encoding.UTF8.GetString(data).IndexOf(binary_delim) + binary_delim.Length;
                        binary.AddRange(data.Skip(index));
                    }
                }
            };
            wss.OnColse += (sender, e) =>
            {
                //File.WriteAllBytes($"{savePath}temp.mp3", binary.ToArray());
                if (binary.Count > 0)
                {
                    Audio.PlayToByte(binary.ToArray());
                    if (!string.IsNullOrWhiteSpace(savePath))
                    {
                        File.WriteAllBytes(savePath, binary.ToArray());
                    }
                }
            };
            wss.OnLog += (onmsg) =>
            {
                Console.WriteLine($"[{onmsg.level.ToString()}] {onmsg.msg}");
            };
            if (wss.Run())
            {
                wss.Send(ConvertToAudioFormatWebSocketString(voice.SuggestedCodec));
                wss.Send(ConvertToSsmlWebSocketString(sendRequestId, voice.Locale, voice.Name, rate, msg));
            }

        }
        /// <summary>
        /// 获取支持的音频列表
        /// </summary>
        /// <returns></returns>
        public static List<eVoice> GetVoice()
        {
            var voiceList = Tools.GetEmbedText("Edge_tts_sharp.Source.VoiceList.json");
            return Tools.StringToJson<List<eVoice>>(voiceList);
        }
    }
}

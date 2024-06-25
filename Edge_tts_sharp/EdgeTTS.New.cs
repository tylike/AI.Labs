using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EdgeTTSSharp.Model;
using System.Diagnostics;

namespace EdgeTTSSharp
{
    public class EdgeTTS
    {
        static string GetGUID()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        public static string FormatPercentage(double input)
        {
            return input.ToString("+#;-#;0") + "%";
        }

        static string ConvertToAudioFormatWebSocketString(string outputFormat)
        {
            return "Content-Type:application/json; charset=utf-8\r\nPath:speech.config\r\n\r\n{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"false\"},\"outputFormat\":\"" + outputFormat + "\"}}}}";
        }

        static string ConvertToSsmlText(string lang, string voice, int rate, string text)
        {
            return $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis'  xml:lang='{lang}'><voice name='{voice}'><prosody pitch='+0Hz' rate ='{FormatPercentage(rate)}' volume='+0%'>{text}</prosody></voice></speak>";
        }

        static string ConvertToSsmlWebSocketString(string requestId, string lang, string voice, int rate, string msg)
        {
            return $"X-RequestId:{requestId}\r\nContent-Type:application/ssml+xml\r\nPath:ssml\r\n\r\n{ConvertToSsmlText(lang, voice, rate, msg)}";
        }

        public static async Task PlayText(string msg, string voiceShortName, int rate = 0, string savePath = "", bool play = true, List<byte> resultBytes = null)
        {
            var tcs = new TaskCompletionSource<bool>();

            var voice = Voices.FirstOrDefault(i => i.ShortName == voiceShortName);
            if (voice == null)
            {
                throw new Exception("没有找到对应的声音记录!");
            }

            var binaryDelimiter = "Path:audio\r\n";
            var sendRequestId = GetGUID();
            var audioBytes = new List<byte>();

            var wsUri = new Uri("wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken=6A5AA1D4EAFF4E9FB37E23D68491D6F4");
            using var client = new ClientWebSocket();
            var cts = new CancellationTokenSource();
            try
            {
                await client.ConnectAsync(wsUri, cts.Token);

                async Task SendAsync(string message)
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }

                async Task ReceiveLoopAsync()
                {
                    var buffer = new byte[8192];
                    while (client.State == WebSocketState.Open)
                    {
                        try
                        {
                            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cts.Token);
                                tcs.SetResult(true);
                                break;
                            }

                            var data = buffer.Take(result.Count).ToArray();
                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                var message = Encoding.UTF8.GetString(data);
                                if (message.Contains("Path:turn.end"))
                                {
                                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cts.Token);
                                    tcs.SetResult(true);
                                    break;
                                }
                                Console.WriteLine(message);
                            }
                            else if (result.MessageType == WebSocketMessageType.Binary)
                            {
                                if (data[0] == 0x00 && data[1] == 0x67 && data[2] == 0x58)
                                {
                                    continue;
                                }
                                var index = Encoding.UTF8.GetString(data).IndexOf(binaryDelimiter) + binaryDelimiter.Length;
                                audioBytes.AddRange(data.Skip(index));
                            }
                        }
                        catch (WebSocketException ex)
                        {
                            Console.WriteLine($"WebSocket Error: {ex.Message}");
                            tcs.SetResult(false);
                            break;
                        }
                    }
                }

                await SendAsync(ConvertToAudioFormatWebSocketString(voice.SuggestedCodec));
                await SendAsync(ConvertToSsmlWebSocketString(sendRequestId, voice.Locale, voice.Name, rate, msg));

                var receiveTask = ReceiveLoopAsync();
                await tcs.Task;

                if (audioBytes.Count > 0)
                {
                    if (resultBytes != null)
                    {
                        resultBytes.AddRange(audioBytes);
                        Debug.WriteLine("音频数据已经返回");
                    }
                    if (play)
                        Audio.PlayToByte(audioBytes.ToArray());

                    if (!string.IsNullOrWhiteSpace(savePath))
                    {
                        File.WriteAllBytes(savePath, audioBytes.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (client.State == WebSocketState.Open || client.State == WebSocketState.Connecting)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cts.Token);
                }
            }
        }

        static List<eVoice> voices;

        public static List<eVoice> Voices
        {
            get
            {
                if (voices == null)
                {
                    var voiceList = Tools.GetEmbedText("Edge_tts_sharp.Source.VoiceList.json");
                    voices = Tools.StringToJson<List<eVoice>>(voiceList);
                }
                return voices;
            }
        }
    }
}
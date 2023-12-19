// See https://aka.ms/new-console-template for more information
using Edge_tts_sharp;

string msg = string.Empty;
Console.WriteLine("请输入文本内容.");
msg = Console.ReadLine();
// 获取xiaoxiao语音包
var voice = Edge_tts.GetVoice().FirstOrDefault(i=> i.Name == "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)");
// 文字转语音，并且设置语速
Edge_tts.PlayText(msg, voice, -25);
Console.ReadLine();
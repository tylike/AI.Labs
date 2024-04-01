//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
using System.Diagnostics;
using Xabe.FFmpeg;
namespace AI.Labs.Module.BusinessObjects
{
    public static class XabeFFmpegExtendesion
    {
        public async static Task<IConversionResult> Run(this Xabe.FFmpeg.IConversion conversion)
        {
            var cmd = conversion.Build();
            Debug.WriteLine(cmd);
            return await conversion.Start();
        }
    }
}

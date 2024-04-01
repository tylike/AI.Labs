using AI.Labs.Module.BusinessObjects.AudioBooks;
using com.sun.tools.javadoc;
using DevExpress.Charts.Native;
using DevExpress.CodeParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;
using static edu.stanford.nlp.io.EncodingPrintWriter;


namespace VideoEditor
{
    /// <summary>
    /// Segment接口，定义输入和输出
    /// </summary>
    public interface ISegment<TInput, TOutput>
    {
        /// <summary>
        /// 输入标签
        /// </summary>
        TInput Input { get; set; }

        /// <summary>
        /// 输出标签
        /// </summary>
        TOutput Output { get; set; }
    }

    /// <summary>
    /// Filter接口，定义滤镜的基本操作
    /// </summary>
    public interface IFilter : ISegment<string, string>
    {
        /// <summary>
        /// 滤镜的参数
        /// </summary>
        string Parameters { get; set; }

        /// <summary>
        /// 生成滤镜字符串
        /// </summary>
        string GenerateFilterString();
    }

    /// <summary>
    /// FilterChain接口，定义滤镜链操作
    /// </summary>
    public interface IFilterChain
    {
        /// <summary>
        /// 添加滤镜到链中
        /// </summary>
        IFilterChain AddFilter(IFilter filter);

        /// <summary>
        /// 构建filter_complex字符串
        /// </summary>
        string Build();
    }

    /// <summary>
    /// Video接口，定义视频操作
    /// </summary>
    public interface IVideo
    {
        /// <summary>
        /// 视频文件名
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// 获取滤镜链
        /// </summary>
        IFilterChain FilterChain { get; }

        /// <summary>
        /// 添加滤镜到视频中
        /// </summary>
        void AddFilter(IFilter filter);
    }


}
//ffmpeg - i input.mp4 - vf "trim=start_frame=s1_start:end_frame=s1_end,setpts=PTS-STARTPTS[s1];
//                          trim = start_frame = s2_start:end_frame = s2_end,setpts = PTS - STARTPTS[s2];
//trim = start_frame = s3_start:end_frame = s3_end,setpts = PTS - STARTPTS[s3];
//[s2] trim = end_frame = -1,setpts = PTS - STARTPTS,fps = 1 / T,tile = 1x1[pause];
//[s1][s2][pause][s3] concat = n = 4:v = 1:a = 0[outv]" -af "atrim = start = s1_start:end = s1_end[a1];
//atrim = start = s2_start:end = s2_end[a2];
//atrim = start = s3_start:end = s3_end[a3];
//asilence = duration = 1:mode = duration[a_silence1];
//asilence = duration = 2:mode = duration[a_silence2];
//[a1][a_silence1][a2][a_silence2][a3] concat = n = 5:v = 0:a = 1[outa]" -c:a aac -c:v libx264 -scodec mov_text -metadata:s:s:0 language=eng -map "[outv]" -map "[outa]" -map 0:s output.mp4
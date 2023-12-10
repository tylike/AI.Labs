using System.Globalization;
using System.Reflection;
using System.Threading;
using Whisper;

namespace TranscribeCS
{
	public sealed record class CommandLineArgs
    {
        /// <summary>
        /// 指定计算过程中使用的线程数，默认为 Threads 变量的值。用于控制并行计算的线程数。
        /// </summary>
        public int Threads = Environment.ProcessorCount;
        
		/// <summary>
        /// 指定时间偏移量（以毫秒为单位）。用于设置音频处理的时间偏移。
        /// 参数指定音频的时间偏移量。
		/// N 是一个整数值，表示以毫秒为单位的时间偏移量。
		/// 例如，-ot 5000 表示将音频的起始位置向后偏移 5000 毫秒（5 秒）。
        /// 这个参数的作用是在处理音频时，可以指定从音频的特定时间点开始处理，而不是从音频的起始位置开始。
		/// 可以用来跳过音频的前面一部分，或者选择处理音频的特定片段。
		/// 例如，如果有一个 10 分钟的音频文件，使用5000参数，将会从音频的第 5 秒开始进行处理，忽略前面的 5 秒音频内容。
        /// </summary>
        public int SkipMilliSecond = 0;

        /// <summary>
        /// 指定“段落”索引偏移量，默认为 offset_n 变量的值。用于设置段落索引的偏移。
        /// </summary>
        public int SkipSegment = 0;

        /// <summary>
        /// 指定要处理的音频持续时间（以毫秒为单位），即结束时间。
        /// </summary>
        public int DurationMilliSecond = 0;

        /// <summary>
        /// 指定要存储的文本上下文标记的最大数量，用于控制存储的文本上下文的数量。
		/// 即，最大输出单词数量
        /// </summary>
        public int MaxOutputWords = -1;

        /// <summary>
        /// 指定段落的最大长度（以字符数为单位），用于限制段落的长度。
		/// 即最大输出长度
        /// </summary>
        public int MaxOutputLength = 0;

        /// <summary>
        /// 指定单词时间戳概率阈值，默认为 word_thold 变量的值。用于控制单词时间戳的概率阈值。
        /// 即参数指定单词时间戳的概率阈值。
		/// N 是一个浮点数值，表示单词时间戳的概率阈值。
		/// 例如，-wt 0.5 表示单词时间戳的概率必须大于等于 0.5 才会被保留。
        /// 这个参数的作用是控制生成文本时间戳时单词的保留与丢弃。
		/// 生成文本时间戳时，模型会为每个单词生成一个时间戳，并根据该单词的概率确定是否保留该时间戳。
		/// 例如，如果设置 -wt 0.8 参数，表示只有当单词的概率大于等于 0.8 时，对应的时间戳才会被保留，否则将被丢弃。
		/// 这个参数在需要过滤或控制生成文本时间戳的精确性时非常有用。
		/// 可以根据需要设置单词时间戳的概率阈值，以控制保留或丢弃单词时间戳的数量。
        /// </summary>
        public float WordThold = 0.01f;

        /// <summary>
        /// 加速音频播放速度，速度为原来的两倍（降低精度）。
        /// </summary>
        public bool SpeedUp = false;

        /// <summary>
        /// 将音频从源语言翻译成英语。
        /// </summary>
        public bool Translate = false;

        /// <summary>
        /// 对立体声音频进行话者分离。
        /// </summary>
        public bool Diarize = false;
		/// <summary>
		/// 输出为文本文件
		/// </summary>
		public bool OutputTxt = false;
		/// <summary>
		/// 输出vtt字幕
		/// </summary>
		public bool OutputVtt = false;
		/// <summary>
		/// 输出srt字幕
		/// </summary>
		public bool OutputSrt = false;
        /// <summary>
        /// 打印特殊标记
        /// </summary>
        public bool print_special = false;

		/// <summary>
		/// 打印进度
		/// </summary>
		public bool print_progress = false;

		public bool print_colors = true;
        /// <summary>
        /// 打印时间戳
        /// </summary>
        public bool NoTimestamps = false;
		public string? Prompt = null;

        /// <summary>
        /// 来源语言:指定音频的语言，默认为 language.getCode() 变量的值。
        /// </summary>
        public eLanguage Language = eLanguage.Chinese;

		const bool output_wts = false;
		public void Apply( ref Parameters p )
		{
			p.setFlag( eFullParamsFlags.PrintRealtime, false );
			p.setFlag( eFullParamsFlags.PrintProgress, print_progress );
			p.setFlag( eFullParamsFlags.PrintTimestamps, !NoTimestamps );
			p.setFlag( eFullParamsFlags.PrintSpecial, print_special );
			p.setFlag( eFullParamsFlags.Translate, Translate );
			p.language = Language;
			p.cpuThreads = Threads;
			if( MaxOutputWords >= 0 )
				p.n_max_text_ctx = MaxOutputWords;
			p.offset_ms = SkipMilliSecond;
			p.duration_ms = DurationMilliSecond;
			p.setFlag( eFullParamsFlags.TokenTimestamps, output_wts || MaxOutputLength > 0 );
			p.thold_pt = WordThold;
			p.max_len = output_wts && MaxOutputLength == 0 ? 60 : MaxOutputLength;
			p.setFlag( eFullParamsFlags.SpeedupAudio, SpeedUp );
		}

		public eResultFlags GetResultFlags()
		{
			eResultFlags flags = eResultFlags.None;
			bool wts = output_wts || MaxOutputLength > 0;
			if( !NoTimestamps || wts )
				flags |= eResultFlags.Timestamps;
			if( wts || print_colors )
				flags |= eResultFlags.Tokens;
			return flags;
		}
		

		public CommandLineArgs()
		{
		}
	}
}
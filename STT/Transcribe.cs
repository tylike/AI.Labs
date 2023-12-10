using System.Diagnostics;
using System.Globalization;
using Whisper;

namespace TranscribeCS
{
    /// <summary>
    /// Implementation of Callbacks abstract class, to print these segments as soon as they’re produced by the library.
    /// 实现回调抽象类，以便在库生成这些段落后立即打印出来。
    /// </summary>
    sealed class Transcribe: Callbacks
	{
		readonly CommandLineArgs args;
		readonly eResultFlags resultFlags;

		public Transcribe( CommandLineArgs args )
		{
			this.args = args;
			resultFlags = args.GetResultFlags();
			//Console.OutputEncoding = System.Text.Encoding.UTF8;
		}

        // Terminal color map. 10 colors grouped in ranges [0.0, 0.1, ..., 0.9]
        // Lowest is red, middle is yellow, highest is green.
        // 终端颜色映射。将 10 种颜色分为范围 [0.0，0.1，...，0.9]
        // 最低的是红色，中间的是黄色，最高的是绿色。
        readonly string[] k_colors = new string[]
		{
			"\x1B[38;5;196m", "\x1B[38;5;202m", "\x1B[38;5;208m", "\x1B[38;5;214m", "\x1B[38;5;220m",
			"\x1B[38;5;226m", "\x1B[38;5;190m", "\x1B[38;5;154m", "\x1B[38;5;118m", "\x1B[38;5;82m"
		};

		int colorIndex( in sToken tok )
		{
			//根据概率取得颜色，结合上面的说明，可能是
			//概率越高是绿色、中间黄色、最低是红色
			float p = tok.probability;
			float p3 = p * p * p;
			int col = (int)( p3 * k_colors.Length );
			col = Math.Clamp( col, 0, k_colors.Length - 1 );
			return col;
		}

		public static string printTime( TimeSpan ts ) =>
			ts.ToString( "hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture );
		public static string printTimeWithComma( TimeSpan ts ) =>
			ts.ToString( "hh':'mm':'ss','fff", CultureInfo.InvariantCulture );

		public event EventHandler<NewSegmentArgs> NewSegment;

		protected override void onNewSegment( Context sender, int countNew )
		{
			TranscribeResult res = sender.results( resultFlags );

			//NewSegment?.Invoke(sender, )
			if (NewSegment != null)
			{
				var arg = new NewSegmentArgs();
				arg.NewCount = countNew;
				foreach (var item in res.segments)
				{
					arg.Segments.Add(new NewSegment { Text = item.text, Start = DateTime.Now, End = DateTime.Now, StartTimeSpan = item.time.begin, EndTimeSpan = item.time.end });
					foreach (sToken tok in res.getTokens(item))
					{
						//if (!args.print_special && tok.hasFlag(eTokenFlags.Special))
						//    continue;
						var spec = tok.hasFlag(eTokenFlags.Special) ? "特殊" : "";
						Debug.WriteLine($"{tok.probability}={tok.text} {spec}");
					}
				}
				NewSegment(sender, arg);
			}

			ReadOnlySpan<sToken> tokens = res.tokens;

			int s0 = res.segments.Length - countNew;
			if( s0 == 0 )
				Console.WriteLine();

			for( int i = s0; i < res.segments.Length; i++ )
			{
				sSegment seg = res.segments[ i ];

				if( args.NoTimestamps )
				{
					if( args.print_colors && AnsiCodes.enabled )
					{
						foreach( sToken tok in res.getTokens( seg ) )
						{
							if( !args.print_special && tok.hasFlag( eTokenFlags.Special ) )
								continue;
							Console.Write( "{0}{1}{2}", k_colors[ colorIndex( tok ) ], tok.text, "\x1B[0m" );
						}
					}
					else
						Console.Write( seg.text );
					Console.Out.Flush();
					continue;
				}

				string speaker = "";
				if( args.Diarize )
				{
					speaker = sender.detectSpeaker( seg.time ) switch
					{
						eSpeakerChannel.Unsure => "(speaker ?)",
						eSpeakerChannel.Left => "(speaker 0)",
						eSpeakerChannel.Right => "(speaker 1)",
						_ => ""
					};
				}

				if( args.print_colors && AnsiCodes.enabled )
				{
					Console.Write( "[{0} --> {1}] {2} ", printTime( seg.time.begin ), printTime( seg.time.end ), speaker );
					foreach( sToken tok in res.getTokens( seg ) )
					{
						if( !args.print_special && tok.hasFlag( eTokenFlags.Special ) )
							continue;
						Console.Write( "{0}{1}{2}", k_colors[ colorIndex( tok ) ], tok.text, "\x1B[0m" );
					}
					Console.WriteLine();
				}
				else
					Console.WriteLine( "[{0} --> {1}] {2} {3}", printTime( seg.time.begin ), printTime( seg.time.end ), speaker, seg.text );
			}
		}
	}

	public class NewSegmentArgs
	{
		public List<NewSegment> Segments { get; } = new List<NewSegment>();
		public int NewCount { get; set; }
    }

	public class NewSegment
	{
		public string Text { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public TimeSpan StartTimeSpan { get; set; }
		public TimeSpan EndTimeSpan { get; set; }
	}
}
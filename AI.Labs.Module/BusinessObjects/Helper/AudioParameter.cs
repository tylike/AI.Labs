using NAudio.Wave;

namespace AI.Labs.Module.BusinessObjects
{
    public class AudioParameter
    {
        public string FileName { get; set; }
        //是指主时间线的:
        public int StartTimeMS { get; set; }
        public int EndTimeMS { get; set; }
        //是指当前片段的:
        public int ClipStartTimeMS { get; set; }
        public int ClipEndTimeMS { get; set; }
        public int Index { get; set; }

        /// <summary>
        /// 处理完成后的结果
        /// </summary>
        public MemoryStream Stream { get; set; }

        public double Speed { get; set; } = 1;

        public void ChangeSpeed(double speed)
        {
            if (Speed > 0 && Speed != 1)
            {
                Stream = new MemoryStream();
                using var t = new WaveFileReader(FileName);
                FFmpegHelper.NAudioChangeSpeed(t, Speed, Stream);
                Stream.Position = 0;
            }
        }

        WaveFileReader waveFileReader;
        public WaveFileReader WaveFileReader 
        {
            get
            {
                if(waveFileReader == null)
                {
                    if(Stream == null)
                    {
                        Stream = new MemoryStream();
                        File.OpenRead(FileName).CopyTo(Stream);
                    }
                    Stream.Position = 0;
                    waveFileReader = new WaveFileReader(Stream);
                }
                return waveFileReader;
            }
            set { waveFileReader = value; } 
        }
    }
}

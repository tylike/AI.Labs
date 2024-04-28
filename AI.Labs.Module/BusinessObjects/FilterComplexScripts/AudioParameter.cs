using NAudio.Wave;

namespace AI.Labs.Module.BusinessObjects.FilterComplexScripts
{
    public class AudioParameter
    {
        public string FileName { get; set; }
        //是指主时间线的:
        public int StartTimeMS { get; set; }
        public int EndTimeMS { get; set; }

        #region 暂时不用
        ////是指当前片段的:
        //public int ClipStartTimeMS { get; set; }
        //public int ClipEndTimeMS { get; set; } 
        #endregion

        public int Index { get; set; }

        /// <summary>
        /// 处理完成后的结果
        /// </summary>
        public MemoryStream Stream { get; set; }

        public double Speed { get; set; } = 1;

        Mp3FileReader waveFileReader;
        public Mp3FileReader WaveFileReader
        {
            get
            {
                if (waveFileReader == null)
                {
                    if (Stream == null)
                    {
                        Stream = new MemoryStream();
                        File.OpenRead(FileName).CopyTo(Stream);
                    }
                    Stream.Position = 0;
                    waveFileReader = new Mp3FileReader(Stream);
                }
                return waveFileReader;
            }
            set { waveFileReader = value; }
        }
    }
}

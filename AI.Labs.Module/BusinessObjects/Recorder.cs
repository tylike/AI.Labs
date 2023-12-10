using AI.Labs.Module.BusinessObjects.STT;
using DevExpress.ExpressApp;
using NAudio.Wave;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace AI.Labs.Module.BusinessObjects
{

    [SupportedOSPlatform("windows")]
    public class Recorder
    {
        public Recorder()
        {

        }
        public bool IsStarted { get; protected set; }
        string saveFileName;
        public string SaveFileName
        {
            get => saveFileName;
            set
            {
                saveFileName = value;
                var fi = new FileInfo(saveFileName);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
            }
        }
        
        private WaveFileWriter m_waveFileWriter;
        private WaveInEvent waveIn = new WaveInEvent();
        //WaveFormat WaveFormat = new WaveFormat(16000, 1);
        public void StartRecord()
        {
            m_waveFileWriter = new WaveFileWriter(SaveFileName, waveIn.WaveFormat);
            waveIn.DataAvailable += WaveIn_DataAvailable;
            //(s, a) =>
            //{
            //    Debug.Write("有录音数据");
            //    m_waveFileWriter.Write(a.Buffer, 0, a.BytesRecorded);
            //};
            try
            {
                waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message+ "\n也许是因为没有麦克风!", ex);
            }
            IsStarted = true;
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs a)
        {
            //Debug.Write("有录音数据");
            m_waveFileWriter.Write(a.Buffer, 0, a.BytesRecorded);
            try
            {
                Debug.WriteLine(STTService.Instance.SpeechRecognition(a.Buffer));
            }
            catch
            {
                Debug.WriteLine("X");
            }
        }

        public void StopRecord()
        {
            waveIn.DataAvailable -= WaveIn_DataAvailable;
            waveIn.StopRecording();
            m_waveFileWriter.Dispose();
            //waveIn.Dispose();
            IsStarted = false;
        }
    }
}

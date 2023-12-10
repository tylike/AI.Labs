using TranscribeCS;
using Whisper;

/// <summary>
/// 实时语单识别
/// </summary>
public class RealTimeSpeechRecognitionEngine
{
    public RealTimeSpeechRecognitionEngine(string modelFile, CaptureDeviceId? captureDevice = null)
    {
        var cla = new CommandLineArgs();

        //args = new string[] { 
        //	"-m",@"d:\ai.stt\ggml-large.bin",
        //	"-l","zh"
        //          };
        try
        {
            const eLoggerFlags loggerFlags = eLoggerFlags.UseStandardError | eLoggerFlags.SkipFormatMessage;
            Library.setLogSink(eLogLevel.Debug, loggerFlags);

            using iMediaFoundation mf = Library.initMediaFoundation();
            CaptureDeviceId[] devices = mf.listCaptureDevices() ??
                throw new ApplicationException("This computer has no audio capture devices");
            //使用第一个默认设备
            if (captureDevice == null)
            {
                captureDevice = devices.First();
            }
            //if( cla.ListDevices )
            //{
            //	for( int i = 0; i < devices.Length; i++ )
            //		Console.WriteLine( "#{0}: {1}", i, devices[ i ].displayName );
            //	return 0;
            //}
            //if( cla.captureDeviceIndex < 0 || cla.captureDeviceIndex >= devices.Length )
            //	throw new ApplicationException( $"Capture device index is out of range; the valid range is [ 0 .. {devices.Length - 1} ]" );

            sCaptureParams cp = new sCaptureParams(true);
            if (cla.Diarize)
                cp.flags |= eCaptureFlags.Stereo;

            using iAudioCapture captureDev = mf.openCaptureDevice(captureDevice.Value, cp);

            using iModel model = Library.loadModel(modelFile);
            using Context context = model.createContext();
            cla.Apply(ref context.parameters);

            CaptureThread thread = new CaptureThread(cla, context, captureDev);
            thread.Join();

            context.timingsPrint();
        }
        catch (Exception ex)
        {
            // Console.WriteLine( ex.Message );
            Console.WriteLine(ex.ToString());
            throw ex;
        }
    }
}
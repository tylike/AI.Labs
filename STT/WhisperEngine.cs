namespace TranscribeCS;

using System.Diagnostics;
using System.Runtime.InteropServices;
using Whisper;

public class WhisperEngine : IDisposable, IWhisperEngine
{
    CommandLineArgs args = new();

    public string ModelFile { get; private set; }
    public WhisperEngine(string modelFile = @"d:\ai.stt\ggml-large-v1.bin")
    {
        ModelFile = modelFile;
    }
    iModel model;
    Context context;
    public void Setup()
    {
        var sw = Stopwatch.StartNew();
        model = Library.loadModel(ModelFile);
        context = model.createContext();
        sw.Stop();
        Debug.WriteLine("加载模型耗时:" + sw.ElapsedMilliseconds);

        const eLoggerFlags loggerFlags = eLoggerFlags.None;// eLoggerFlags.UseStandardError | eLoggerFlags.SkipFormatMessage;
        Library.setLogSink(eLogLevel.Debug, loggerFlags);

    }
    /// <summary>
    /// 语音识别:从wave音频数据转换为文字
    /// </summary>
    /// <param name="wavFileData"></param>
    /// <param name="prompts"></param>
    /// <returns></returns>
    public string GetTextFromWavData(byte[] wavFileData, string prompts = null)
    {
        int[]? prompt = null;

        if (!string.IsNullOrEmpty(prompts))
            prompt = model.tokenize(prompts);

        args.Apply(ref context.parameters);

        // When there're multiple input files, assuming they're independent clips
        context.parameters.setFlag(eFullParamsFlags.NoContext, true);

        using iMediaFoundation mf = Library.initMediaFoundation();
        Transcribe transcribe = new Transcribe(args);

        var sw = Stopwatch.StartNew();
        using iAudioReader reader = mf.loadAudioFileData(wavFileData); //mf.openAudioFile(audioFile);

        context.runFull(reader, transcribe, null, prompt);
        var rst = string.Join("\r\n", context.results().segments.ToArray().Select(t => t.text));

        sw.Stop();
        Console.WriteLine("用时：" + sw.Elapsed);

        context.timingsPrint();
        return rst;
    }
    public event EventHandler<NewSegmentArgs> NewSegments;
    public void StopRealTimeSpeechRecognition()
    {
        Debug.WriteLine("开始等待捕获线程结束");
        captureThread.Join();
        Debug.WriteLine("捕获线程结束了?");

        context.timingsPrint();
    }
    CaptureThread captureThread;
    iAudioCapture captureDev;

    public void StartRealTimeSpeechRecognition()
    {
        StartRealTimeSpeechRecognition(null);
    }

    private void StartRealTimeSpeechRecognition(CaptureDeviceId? captureDevice = null)
    {
        try
        {
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

            cp.minDuration = 1;
            cp.maxDuration = 20;

            if (args.Diarize)
                cp.flags |= eCaptureFlags.Stereo;

            captureDev = mf.openCaptureDevice(captureDevice.Value, cp);

            //using Context context = model.createContext();
            args.Apply(ref context.parameters);

            captureThread = new CaptureThread(args, context, captureDev);

            captureThread.NewSegment += (s, e) =>
            {
                if (this.NewSegments != null)
                {
                    this.NewSegments(s, e);
                }
            };
            Debug.WriteLine("captureThread.Start();");
            captureThread.Start();
        }
        catch (Exception ex)
        {
            // Console.WriteLine( ex.Message );
            Console.WriteLine(ex.ToString());
            throw ex;
        }
    }


    #region old
    //static int MainX(string[] args)
    //{
    //    try
    //    {
    //        // dbgListGPUs();

    //        CommandLineArgs cla;
    //        try
    //        {
    //            cla = new CommandLineArgs(args);
    //        }
    //        catch (OperationCanceledException)
    //        {
    //            return 1;
    //        }
    //        const eLoggerFlags loggerFlags = eLoggerFlags.UseStandardError | eLoggerFlags.SkipFormatMessage;
    //        Library.setLogSink(eLogLevel.Debug, loggerFlags);

    //        using iModel model = Library.loadModel(cla.ModelFile);
    //        int[]? prompt = null;
    //        if (!string.IsNullOrEmpty(cla.Prompt))
    //            prompt = model.tokenize(cla.Prompt);

    //        using Context context = model.createContext();
    //        cla.Apply(ref context.parameters);
    //        // When there're multiple input files, assuming they're independent clips
    //        context.parameters.setFlag(eFullParamsFlags.NoContext, true);
    //        using iMediaFoundation mf = Library.initMediaFoundation();
    //        Transcribe transcribe = new Transcribe(cla);

    //        foreach (string audioFile in cla.InputWavFileNames)
    //        {
    //            if (openMode == eFileOpenMode.StreamFile)
    //            {
    //                using iAudioReader reader = mf.openAudioFile(audioFile, cla.Diarize);
    //                context.runFull(reader, transcribe, null, prompt);
    //            }
    //            else if (openMode == eFileOpenMode.BufferPCM)
    //            {
    //                using iAudioBuffer buffer = mf.loadAudioFile(audioFile, cla.Diarize);
    //                context.runFull(buffer, transcribe, prompt);
    //            }
    //            else if (openMode == eFileOpenMode.BufferFile)
    //            {
    //                byte[] buffer = File.ReadAllBytes(audioFile);
    //                using iAudioReader reader = mf.loadAudioFileData(buffer, cla.Diarize);
    //                context.runFull(reader, transcribe, null, prompt);
    //            }

    //            // When asked to, produce these text files
    //            if (cla.OutputTxt)
    //                writeTextFile(context, audioFile);
    //            if (cla.OutputSrt)
    //                writeSubRip(context, audioFile, cla);
    //            if (cla.OutputVtt)
    //                writeWebVTT(context, audioFile);
    //        }

    //        context.timingsPrint();
    //        return 0;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.Message);
    //        return ex.HResult;
    //    }
    //}

    //static void writeTextFile(Context context, string audioPath)
    //{
    //    using var stream = File.CreateText(Path.ChangeExtension(audioPath, ".txt"));
    //    foreach (sSegment seg in context.results().segments)
    //        stream.WriteLine(seg.text);
    //}

    //static void writeSubRip(Context context, string audioPath, CommandLineArgs cliArgs)
    //{
    //    using var stream = File.CreateText(Path.ChangeExtension(audioPath, ".srt"));
    //    var segments = context.results(eResultFlags.Timestamps).segments;

    //    for (int i = 0; i < segments.Length; i++)
    //    {
    //        stream.WriteLine(i + 1 + cliArgs.SkipSegment);
    //        sSegment seg = segments[i];
    //        string begin = Transcribe.printTimeWithComma(seg.time.begin);
    //        string end = Transcribe.printTimeWithComma(seg.time.end);
    //        stream.WriteLine("{0} --> {1}", begin, end);
    //        stream.WriteLine(seg.text);
    //        stream.WriteLine();
    //    }
    //}

    //static void writeWebVTT(Context context, string audioPath)
    //{
    //    using var stream = File.CreateText(Path.ChangeExtension(audioPath, ".vtt"));
    //    stream.WriteLine("WEBVTT");
    //    stream.WriteLine();

    //    foreach (sSegment seg in context.results(eResultFlags.Timestamps).segments)
    //    {
    //        string begin = Transcribe.printTime(seg.time.begin);
    //        string end = Transcribe.printTime(seg.time.end);
    //        stream.WriteLine("{0} --> {1}", begin, end);
    //        stream.WriteLine(seg.text);
    //        stream.WriteLine();
    //    }
    //}


    #endregion
    public static string[] GetGPUs()
    {
        return Library.listGraphicAdapters();
        //Console.WriteLine("    Graphics Adapters:\n{0}", string.Join("\n", list));
    }
    public void Unload()
    {
        model.Dispose();
        model = null;
        context = null;
    }
    public void Dispose()
    {
        Unload();
    }
}
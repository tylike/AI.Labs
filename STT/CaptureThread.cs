using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Whisper;

namespace TranscribeCS
{
    public sealed class CaptureThread : CaptureCallbacks
    {
        public event EventHandler<NewSegmentArgs> NewSegment;
        public CaptureThread(CommandLineArgs args, Context context, iAudioCapture source)
        {
            callbacks = new Transcribe(args);
            callbacks.NewSegment += (s, e) =>
            {
                if (NewSegment != null)
                {
                    NewSegment(s, e);
                }
            };

            this.context = context;
            this.source = source;

            thread = new Thread(ThreadMain) { Name = "Capture Thread" };
            //Console.WriteLine( "Press any key to quit" );
            
        }
        public void Start()
        {
            thread.Start();
        }
        void ThreadMain()
        {
            try
            {
                Debug.WriteLine("捕获线程开始!");
                context.runCapture(source, callbacks, this);
                Debug.WriteLine("捕获线程结束!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"捕获线程报错:{ex.Message}!");
                edi = ExceptionDispatchInfo.Capture(ex);
            }
        }

        static void readKeyCallback(object? state)
        {
            CaptureThread ct = (state as CaptureThread) ?? throw new ApplicationException();
            //int i = 0;
            //while (true)
            //{
            //    i += 200;
            //    Thread.Sleep(200);
            //    if (i > 2000)
            //    {
            //        i = 0;
            //        Debug.Write(".");
            //    }
            //}
            //Console.ReadKey();
            ct.shouldQuit = true;
        }

        public void Join()
        {
            ThreadPool.QueueUserWorkItem(readKeyCallback, this);
            thread.Join();
            edi?.Throw();
        }

        volatile bool shouldQuit = false;

        protected override bool ShouldCancel(Context sender) => shouldQuit;

        protected override void CaptureStatusChanged(Context sender, eCaptureStatus status)
        {
            Debug.WriteLine($"CaptureStatusChanged: {status}");
        }

        readonly Transcribe callbacks;
        readonly Thread thread;
        readonly Context context;
        readonly iAudioCapture source;
        ExceptionDispatchInfo? edi = null;

    }
}
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using SpeechRecognizer = Microsoft.CognitiveServices.Speech.SpeechRecognizer;
//using System.Speech.Recognition;

namespace AI.Labs.Module.BusinessObjects.STT
{
    public class AzureASR
    {
        // This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
        const string speechKey = "408ca52594684cc4b53e72f4c83bbc9b";// Environment.GetEnvironmentVariable("SPEECH_KEY");
        const string speechRegion = "japanwest";//Environment.GetEnvironmentVariable("SPEECH_REGION");
        public SpeechConfig SpeechConfig { get; set; }
        public AudioConfig AudioConfig { get; set; }
        public SpeechRecognizer SpeechRecognizer { get; set; }
        TaskCompletionSource<int> StopRecognition { get; set; }
        public AzureASR()
        {
            SpeechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            //https://learn.microsoft.com/zh-cn/azure/ai-services/speech-service/language-support?tabs=stt
            SpeechConfig.SpeechRecognitionLanguage = "zh-CN";

            AudioConfig = AudioConfig.FromDefaultMicrophoneInput();
            SpeechRecognizer = new SpeechRecognizer(SpeechConfig, AudioConfig);

            Console.WriteLine("Speak into your microphone.");
            SpeechRecognizer.Recognizing += SpeechRecognizer_Recognizing;
            SpeechRecognizer.Recognized += SpeechRecognizer_Recognized;
            SpeechRecognizer.Canceled += SpeechRecognizer_Canceled;
            SpeechRecognizer.SessionStopped += SpeechRecognizer_SessionStopped;
            StopRecognition = new TaskCompletionSource<int>();
        }

        private void SpeechRecognizer_SessionStopped(object sender, SessionEventArgs e)
        {
            Console.WriteLine("\n    Session stopped event.");
            StopRecognition.TrySetResult(0);
        }

        private void SpeechRecognizer_Canceled(object sender, SpeechRecognitionCanceledEventArgs e)
        {
            Console.WriteLine($"CANCELED: Reason={e.Reason}");
            if (e.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
            }
            StopRecognition.TrySetResult(0);
        }

        private void SpeechRecognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
            }
            else if (e.Result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
            }
        }

        private void SpeechRecognizer_Recognizing(object sender, SpeechRecognitionEventArgs e)
        {
            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
        }

        public async Task Start()
        {

            await SpeechRecognizer.StartContinuousRecognitionAsync();

            // Waits for completion. Use Task.WaitAny to keep the task rooted.
            //Task.WaitAny(new[] { StopRecognition.Task });

            // Make the following call at some point to stop recognition:
            // await speechRecognizer.StopContinuousRecognitionAsync();

        }
    }
}

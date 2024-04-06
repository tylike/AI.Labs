using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Editing;
using Windows.Storage;

namespace MediaEditingConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var path = "D:\\videoInfo\\1\\A.mp4";

            byte[] buffer = System.IO.File.ReadAllBytes(path);
            // 填充memoryStream...


            StorageFile storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("myfile.mp4", CreationCollisionOption.ReplaceExisting);

            using (Stream fileStream = await storageFile.OpenStreamForWriteAsync())
            {
                await fileStream.WriteAsync(buffer, 0, buffer.Length);
            }

            Console.WriteLine("Hello World!");
            

            //var file = await StorageFile.GetFileFromPathAsync(Path.Combine(path));

            var mc = await MediaComposition.LoadAsync(storageFile);
            
            var b = Windows.UI.Color.FromArgb(0, 255, 255, 255);
            var c = Windows.UI.Color.FromArgb(255, 0, 255, 255);

            mc.Clips.Add(MediaClip.CreateFromColor(b, TimeSpan.FromSeconds(5)));
            mc.Clips.Add(MediaClip.CreateFromColor(c, TimeSpan.FromSeconds(5)));
            await mc.SaveAsync(storageFile);
        }
    }
}

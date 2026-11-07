using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Windows.Foundation;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.Streams;

// Uso: FrameGrab.exe video.mp4 carpetaSalida ancho alto t1,t2,t3
public static class FrameGrab
{
    static T Wait<T>(IAsyncOperation<T> op)
    {
        while (op.Status == AsyncStatus.Started) Thread.Sleep(5);
        if (op.Status != AsyncStatus.Completed) throw new Exception("Operación fallida: " + op.ErrorCode);
        return op.GetResults();
    }

    public static int Main(string[] args)
    {
        string video = args[0], outDir = args[1];
        int w = int.Parse(args[2]), h = int.Parse(args[3]);
        Directory.CreateDirectory(outDir);
        var file = Wait(StorageFile.GetFileFromPathAsync(video));
        var clip = Wait(MediaClip.CreateFromFileAsync(file));
        var comp = new MediaComposition();
        comp.Clips.Add(clip);
        var vp = clip.GetVideoEncodingProperties(); Console.WriteLine("duracion=" + clip.OriginalDuration.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " tamano=" + vp.Width + "x" + vp.Height);
        foreach (var part in args[4].Split(','))
        {
            double t = double.Parse(part, CultureInfo.InvariantCulture);
            var img = Wait(comp.GetThumbnailAsync(TimeSpan.FromSeconds(t), w, h, VideoFramePrecision.NearestFrame));
            var reader = new DataReader(img.GetInputStreamAt(0));
            uint size = (uint)img.Size;
            var load = reader.LoadAsync(size);
            while (load.Status == AsyncStatus.Started) Thread.Sleep(5);
            var bytes = new byte[load.GetResults()];
            reader.ReadBytes(bytes);
            string path = Path.Combine(outDir, "f_" + t.ToString("00.0", CultureInfo.InvariantCulture) + ".jpg");
            File.WriteAllBytes(path, bytes);
            Console.WriteLine(path);
        }
        return 0;
    }
}

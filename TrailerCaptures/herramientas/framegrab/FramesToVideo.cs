using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

// Convierte una carpeta de fotogramas (JPG o PNG, en orden por nombre) en un MP4 H.264 con música opcional.
// Uso: FramesToVideo.exe carpeta fps salida.mp4 [musica.wav] [ancho alto]
public static class FramesToVideo
{
    static T Wait<T>(IAsyncOperation<T> op)
    {
        while (op.Status == AsyncStatus.Started) Thread.Sleep(2);
        if (op.Status != AsyncStatus.Completed) throw new Exception("Operación fallida: " + op.ErrorCode);
        return op.GetResults();
    }

    static StorageFile Open(string path) { return Wait(StorageFile.GetFileFromPathAsync(Path.GetFullPath(path))); }

    public static int Main(string[] args)
    {
        string dir = args[0];
        double fps = double.Parse(args[1], CultureInfo.InvariantCulture);
        string outPath = Path.GetFullPath(args[2]);
        string music = args.Length > 3 && args[3] != "-" ? args[3] : null;
        int w = args.Length > 5 ? int.Parse(args[4]) : 1920, h = args.Length > 5 ? int.Parse(args[5]) : 1080;

        var files = Directory.GetFiles(dir).Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f, StringComparer.Ordinal).ToArray();
        var comp = new MediaComposition();
        var frameDur = TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerSecond / fps));
        int i = 0;
        foreach (var f in files)
        {
            comp.Clips.Add(Wait(MediaClip.CreateFromImageFileAsync(Open(f), frameDur)));
            if (++i % 100 == 0) Console.WriteLine("  " + i + " / " + files.Length);
        }
        var total = TimeSpan.FromTicks(frameDur.Ticks * files.Length);
        if (music != null)
        {
            var track = Wait(BackgroundAudioTrack.CreateFromFileAsync(Open(music)));
            if (track.OriginalDuration > total) track.TrimTimeFromEnd = track.OriginalDuration - total;
            comp.BackgroundAudioTracks.Add(track);
        }

        var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
        profile.Video.Width = (uint)w; profile.Video.Height = (uint)h;
        profile.Video.FrameRate.Numerator = (uint)Math.Round(fps); profile.Video.FrameRate.Denominator = 1;
        profile.Video.Bitrate = 16000000;
        profile.Audio.Bitrate = 192000; profile.Audio.SampleRate = 48000;

        var folder = Wait(StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(outPath)));
        var outFile = Wait(folder.CreateFileAsync(Path.GetFileName(outPath), CreationCollisionOption.ReplaceExisting));
        var render = comp.RenderToFileAsync(outFile, MediaTrimmingPreference.Precise, profile);
        while (render.Status == AsyncStatus.Started) Thread.Sleep(200);
        if (render.Status != AsyncStatus.Completed) throw new Exception("Render fallido: " + render.ErrorCode);
        var result = render.GetResults();
        Console.WriteLine("resultado=" + result + " fotogramas=" + files.Length + " duracion=" + total.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture) + " s -> " + outPath);
        return result == TranscodeFailureReason.None ? 0 : 1;
    }
}

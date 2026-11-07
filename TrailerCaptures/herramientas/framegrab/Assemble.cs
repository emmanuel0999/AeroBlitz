using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Windows.Foundation;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

// Monta el tráiler con las API de edición de Windows.
// Uso: Assemble.exe guion.txt salida.mp4
// Líneas del guion:
//   clip|video_vertical.mp4|inicio|fin|rotulo.png   (juego en el centro sobre el rótulo)
//   image|imagen.png|duracion                       (imagen a pantalla completa)
//   music|musica.wav
//   size|ancho|alto                                 (tamaño del video; por defecto 1920x1080)
//   clip|...|rotulo.png|x|y|ancho|alto              (posición del juego, opcional)
//   text|imagen.png|inicio|fin|x|y|ancho|alto       (imagen con transparencia encima de todo, en tiempo absoluto)
public static class Assemble
{
    static T Wait<T>(IAsyncOperation<T> op)
    {
        while (op.Status == AsyncStatus.Started) Thread.Sleep(5);
        if (op.Status != AsyncStatus.Completed) throw new Exception("Operación fallida: " + op.ErrorCode);
        return op.GetResults();
    }

    static double D(string s) { return double.Parse(s, CultureInfo.InvariantCulture); }

    static StorageFile Open(string path) { return Wait(StorageFile.GetFileFromPathAsync(Path.GetFullPath(path))); }

    public static int Main(string[] args)
    {
        var comp = new MediaComposition();
        var layer = new MediaOverlayLayer();
        var top = new MediaOverlayLayer();
        var total = TimeSpan.Zero;
        int outW = 1920, outH = 1080;
        foreach (var raw in File.ReadAllLines(args[0]))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            var p = line.Split('|');
            if (p[0] == "clip")
            {
                double start = D(p[2]), end = D(p[3]);
                var dur = TimeSpan.FromSeconds(end - start);
                comp.Clips.Add(Wait(MediaClip.CreateFromImageFileAsync(Open(p[4]), dur)));
                var video = Wait(MediaClip.CreateFromFileAsync(Open(p[1])));
                video.TrimTimeFromStart = TimeSpan.FromSeconds(start);
                video.TrimTimeFromEnd = video.OriginalDuration - TimeSpan.FromSeconds(end);
                var props = video.GetVideoEncodingProperties();
                double h = outH, w = Math.Round(h * props.Width / props.Height);
                var rect = p.Length >= 9 ? new Rect(D(p[5]), D(p[6]), D(p[7]), D(p[8])) : new Rect((outW - w) / 2, 0, w, h);
                var overlay = new MediaOverlay(video, rect, 1.0);
                overlay.Delay = total;
                layer.Overlays.Add(overlay);
                total += dur;
                Console.WriteLine("clip  " + Path.GetFileName(p[1]) + " " + start + "-" + end + " s");
            }
            else if (p[0] == "image")
            {
                var dur = TimeSpan.FromSeconds(D(p[2]));
                comp.Clips.Add(Wait(MediaClip.CreateFromImageFileAsync(Open(p[1]), dur)));
                total += dur;
                Console.WriteLine("image " + Path.GetFileName(p[1]) + " " + dur.TotalSeconds + " s");
            }
            else if (p[0] == "text")
            {
                double start = D(p[2]), end = D(p[3]);
                var img = Wait(MediaClip.CreateFromImageFileAsync(Open(p[1]), TimeSpan.FromSeconds(end - start)));
                var overlay = new MediaOverlay(img, new Rect(D(p[4]), D(p[5]), D(p[6]), D(p[7])), 1.0);
                overlay.Delay = TimeSpan.FromSeconds(start);
                top.Overlays.Add(overlay);
                Console.WriteLine("text  " + Path.GetFileName(p[1]) + " " + start + "-" + end + " s");
            }
            else if (p[0] == "size")
            {
                outW = int.Parse(p[1]); outH = int.Parse(p[2]);
            }
            else if (p[0] == "music")
            {
                comp.BackgroundAudioTracks.Add(Wait(BackgroundAudioTrack.CreateFromFileAsync(Open(p[1]))));
            }
        }
        comp.OverlayLayers.Add(layer);
        if (top.Overlays.Count > 0) comp.OverlayLayers.Add(top);
        foreach (var m in comp.BackgroundAudioTracks)
            if (m.OriginalDuration > total) m.TrimTimeFromEnd = m.OriginalDuration - total;

        var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
        profile.Video.Width = (uint)outW;
        profile.Video.Height = (uint)outH;
        profile.Video.FrameRate.Numerator = 30;
        profile.Video.FrameRate.Denominator = 1;
        profile.Video.Bitrate = 12000000;
        profile.Audio.Bitrate = 192000;   // máximo del codificador AAC de Windows
        profile.Audio.SampleRate = 48000;
        Console.WriteLine("audio: AAC " + profile.Audio.Bitrate / 1000 + " kbps, " + profile.Audio.SampleRate + " Hz, " + profile.Audio.ChannelCount + " canales");

        string outPath = Path.GetFullPath(args[1]);
        var folder = Wait(StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(outPath)));
        var outFile = Wait(folder.CreateFileAsync(Path.GetFileName(outPath), CreationCollisionOption.ReplaceExisting));
        var render = comp.RenderToFileAsync(outFile, MediaTrimmingPreference.Precise, profile);
        while (render.Status == AsyncStatus.Started) Thread.Sleep(200);
        if (render.Status != AsyncStatus.Completed) throw new Exception("Render fallido: " + render.ErrorCode);
        var result = render.GetResults();
        Console.WriteLine("resultado=" + result + " duracion=" + total.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " s -> " + outPath);
        return result == TranscodeFailureReason.None ? 0 : 1;
    }
}

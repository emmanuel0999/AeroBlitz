using System;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Streams;

// Genera un WAV con las voces de Windows (OneCore).
// Uso: Speak.exe "Nombre de voz" archivo.ssml salida.wav
public static class Speak
{
    static T Wait<T>(IAsyncOperation<T> op)
    {
        while (op.Status == AsyncStatus.Started) Thread.Sleep(5);
        if (op.Status != AsyncStatus.Completed) throw new Exception("Operación fallida: " + op.ErrorCode);
        return op.GetResults();
    }

    public static int Main(string[] args)
    {
        var synth = new SpeechSynthesizer();
        var voice = SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.DisplayName.Contains(args[0]));
        if (voice == null) { Console.WriteLine("Voz no encontrada: " + args[0]); return 1; }
        synth.Voice = voice;
        string ssml = File.ReadAllText(args[1]);
        var stream = Wait(synth.SynthesizeSsmlToStreamAsync(ssml));
        var reader = new DataReader(stream.GetInputStreamAt(0));
        uint size = (uint)stream.Size;
        var load = reader.LoadAsync(size);
        while (load.Status == AsyncStatus.Started) Thread.Sleep(5);
        var bytes = new byte[load.GetResults()];
        reader.ReadBytes(bytes);
        File.WriteAllBytes(args[2], bytes);
        Console.WriteLine(voice.DisplayName + " -> " + args[2] + " (" + bytes.Length / 1024 + " KB)");
        return 0;
    }
}

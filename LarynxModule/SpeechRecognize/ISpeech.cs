using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LarynxModule.SpeechRecognize
{
    interface ISpeech<T> where T : ITextResponse
    {
        CultureInfo CultureLanguage { get; set; }
        string EndPointAddress { get; }

        event EventHandler<SpeechTextEventArgs> TextRecognized;

        IEnumerable<T> Recognize(Stream stream, int SampleRateHz, int Channels, AudioFormat format);
        Task<IEnumerable<T>> RecognizeAsync(Stream stream, int SampleRateHz, int Channels, AudioFormat format);
        Task RecognizeStreaming(WaveInEvent waveIn, CancellationToken ct);

    }
}

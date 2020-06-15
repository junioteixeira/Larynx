using Google.Api.Gax.Grpc;
using Google.Cloud.Speech.V1;
using Google.Protobuf;
using Grpc.Core;
using LarynxModule.SpeechRecognize;
using LarynxModule.SpeechRecognize.Google;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Google.Cloud.Speech.V1.SpeechClient;

namespace LarynxModule.SpeechRecognize.Google
{

    public class GoogleSpeech : ISpeech<GoogleTextResponse>
    {
        private const string DEFAULT_KEY = "AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw";
        private const int SAFE_STREAM_TIMEOUT = 280;

        private object writeLock = new object();
        private List<byte> audioBuffer = new List<byte>();

        // Read from the microphone and stream to API.
        private volatile bool writeMore = true;
        private StreamingRecognizeStream streamingGoogle;
        private bool InitializedStreaming = false;

        public event EventHandler<SpeechTextEventArgs> TextRecognized;

        /// <summary>
        /// API Key
        /// </summary>
        public string ApiKey { get; private set; }

        public CultureInfo CultureLanguage { get; set; }

        public string EndPointAddress => "speech.googleapis.com";

        public GoogleSpeech() : this(DEFAULT_KEY, CultureInfo.CurrentCulture)
        {
        }
        public GoogleSpeech(CultureInfo cultureLanguage) : this(DEFAULT_KEY, CultureInfo.CurrentCulture)
        {
        }
        public GoogleSpeech(string apiKey) : this(apiKey, CultureInfo.CurrentCulture)
        {
        }
        public GoogleSpeech(string apikey, CultureInfo cultureLanguage)
        {
            this.ApiKey = apikey;
            this.CultureLanguage = cultureLanguage;
        }

        public IEnumerable<GoogleTextResponse> Recognize(Stream stream, int SampleRateHz, int Channels, AudioFormat format)
        {
            return RecognizeAsync(stream, SampleRateHz, Channels, format).Result;
        }

        public async Task<IEnumerable<GoogleTextResponse>> RecognizeAsync(Stream stream, int SampleRateHz, int Channels, AudioFormat format)
        {
            if (format != AudioFormat.FLAC)
                throw new InvalidCastException("Format not supported for GoogleSpeech Class");

            await Task.Delay(0);
            throw new NotImplementedException();

        }

        private async Task<StreamingRecognizeStream> GetGoogleStream(int SampleRate)
        {
            var sslCredentials = new SslCredentials();

            var speechBuilder = new SpeechClientBuilder();
            speechBuilder.ChannelCredentials = sslCredentials;

            var speech = speechBuilder.Build();
            var streamingCall = speech.StreamingRecognize(CallSettings.FromHeader("x-goog-api-key", ApiKey));

            // Write the initial request with the config.
            await streamingCall.WriteAsync(
                new StreamingRecognizeRequest()
                {
                    StreamingConfig = new StreamingRecognitionConfig()
                    {
                        Config = new RecognitionConfig()
                        {
                            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = SampleRate,
                            LanguageCode = CultureLanguage.ToString(),
                            EnableAutomaticPunctuation = false,
                        },
                        InterimResults = false,
                    }
                });

            return streamingCall;
        }

        private async Task<object> StreamingMicRecognizeAsync(int SampleRate, int timeseconds, CancellationToken ct)
        {
            streamingGoogle = await GetGoogleStream(SampleRate);
            DateTime lastWrite = DateTime.Now;

            // Print responses as they arrive.
            Task receiveResponses = Task.Run(async () =>
            {
                var responseStream = streamingGoogle.GetResponseStream();
                while (await responseStream.MoveNextAsync())
                {
                    StreamingRecognizeResponse response = responseStream.Current;
                    foreach (StreamingRecognitionResult result in response.Results)
                    {
                        foreach (SpeechRecognitionAlternative alternative in result.Alternatives)
                        {
                            OnTextRecognized(alternative.Transcript);
                            lastWrite = DateTime.Now;
                        }
                    }
                }
            });

            InitializedStreaming = true;
            writeMore = true;
            await Task.Delay(TimeSpan.FromSeconds(timeseconds), ct);
            while (DateTime.Now - lastWrite > TimeSpan.FromSeconds(2) &&
                   DateTime.Now - lastWrite < TimeSpan.FromSeconds(10) &&
                   !ct.IsCancellationRequested)
            {
                await Task.Delay(100);
            }

            // Stop write data to speech server and buffer in memory.
            lock (writeLock)
                writeMore = false;

            await streamingGoogle.WriteCompleteAsync();
            await receiveResponses.ConfigureAwait(false);
            return 0;
        }

        public Task RecognizeStreaming(WaveInEvent waveIn, CancellationToken ct)
        {
            if (waveIn.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
                throw new InvalidCastException("Format not supported for Streaming recognize");

            waveIn.DataAvailable += WaveIn_DataAvailable;
            
            Task recognizeTask = Task.Factory.StartNew(async () =>
            {
                for (; !ct.IsCancellationRequested; )
                    await StreamingMicRecognizeAsync(waveIn.WaveFormat.SampleRate, 
                                                     SAFE_STREAM_TIMEOUT, 
                                                     ct);
            }, ct ,TaskCreationOptions.LongRunning, TaskScheduler.Default);
            waveIn.StartRecording();
            return recognizeTask;
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (!InitializedStreaming)
                return;
            lock (writeLock)
            {
                if (!writeMore)
                {
                    for (int i = 0; i < e.BytesRecorded; i++)
                        audioBuffer.Add(e.Buffer[i]);
                }
                else
                {
                    if (audioBuffer.Count > 0)
                    {
                        streamingGoogle.WriteAsync(
                            new StreamingRecognizeRequest()
                            {
                                AudioContent = ByteString
                                    .CopyFrom(audioBuffer.ToArray(), 0, audioBuffer.Count)
                            }).Wait();

                        audioBuffer.Clear();
                    }

                    streamingGoogle.WriteAsync(
                        new StreamingRecognizeRequest()
                        {
                            AudioContent = ByteString
                                .CopyFrom(e.Buffer, 0, e.BytesRecorded)
                        }).Wait();
                }
            }
        }

        public virtual void OnTextRecognized(string Text)
        {
            TextRecognized(this, new SpeechTextEventArgs(Text));
        }
    }
}

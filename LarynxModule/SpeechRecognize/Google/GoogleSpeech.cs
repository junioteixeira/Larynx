using LarynxModule.SpeechRecognize;
using LarynxModule.SpeechRecognize.Google;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LarynxModule.SpeechRecognize.Google
{

    public class GoogleSpeech : ISpeech<GoogleTextResponse>
    {
        private const string GOOGLE_SPEECH_URL = "https://www.google.com/speech-api/v2/recognize?client=chromium";
        private const string DEFAULT_KEY = "AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw";

        private HttpClient httpClient = new HttpClient();

        /// <summary>
        /// API Key
        /// </summary>
        public string ApiKey { get; private set; }

        public CultureInfo CultureLanguage { get; set; }
        public string EndPointAddress => GOOGLE_SPEECH_URL + $"&lang={CultureLanguage.Name}&key=" + ApiKey;


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

        public async Task<string> SpeechToTextAsync(byte[] buf_flac, int sampleRateHz)
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, EndPointAddress);
            req.Content = new ByteArrayContent(buf_flac);
            req.Content.Headers.Add("Content-Type", $"audio/x-flac; rate={sampleRateHz}");
            req.Content.Headers.ContentLength = buf_flac.Length;

            HttpResponseMessage response = await httpClient.SendAsync(req)
                                                           .ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync();
        }

        public IEnumerable<GoogleTextResponse> Recognize(Stream stream, int SampleRateHz, int Channels, AudioFormat format)
        {
            return RecognizeAsync(stream, SampleRateHz, Channels, format).Result;
        }

        public async Task<IEnumerable<GoogleTextResponse>> RecognizeAsync(Stream stream, int SampleRateHz, int Channels, AudioFormat format)
        {
            if (format != AudioFormat.FLAC)
                throw new InvalidCastException("Format not supported for GoogleSpeech Class");
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, EndPointAddress);
            req.Content = new StreamContent(stream);
            req.Content.Headers.Add("Content-Type", $"audio/x-flac; rate={SampleRateHz}");
            req.Content.Headers.ContentLength = stream.Length;

            HttpResponseMessage response = await httpClient.SendAsync(req)
                                                           .ConfigureAwait(false);
            List<GoogleTextResponse> listText = new List<GoogleTextResponse>();
            string response_str = await response.Content.ReadAsStringAsync();
            // JSON CODE HERE

            return listText;
        }
    }
}

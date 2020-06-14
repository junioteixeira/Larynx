using HttpTwo;
using LarynxModule.SpeechRecognize;
using LarynxModule.SpeechRecognize.Google;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LarynxModule.SpeechRecognize.Google
{

    public class GoogleSpeech : ISpeech<GoogleTextResponse>
    {
        private const string GOOGLE_BASE_URL = "http://www.google.com/speech-api/full-duplex/v1";
        private const string GOOGLE_UP_URL = GOOGLE_BASE_URL + "/up?";
        private const string GOOGLE_DOWN_URL = GOOGLE_BASE_URL + "/down?";

        private const string DEFAULT_KEY = "AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw";

        ///speech-api/full-duplex/v1/up?key=AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw
        ///&pair=C880878ED0BA34BA
        ///&output=pb
        ///&lang=pt-BR
        ///&pFilter=2
        ///&maxAlternatives=1
        ///&app=chromium
        ///&continuous
        ///&interim -- ignore
        ///

        // /speech-api/full-duplex/v1/down?key=AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw
        // &pair=C880878ED0BA34BA
        // &output=pb 

        private Http2Client http2Client;


        private const string GOOGLE_SPEECH_URL = "https://www.google.com/speech-api/v2/recognize?client=chromium?maxAlternatives=1";

        private RNGCryptoServiceProvider Gen = new RNGCryptoServiceProvider();

        private HttpClient httpClient = new HttpClient();

        private string pair;

        /// <summary>
        /// API Key
        /// </summary>
        public string ApiKey { get; private set; }

        public CultureInfo CultureLanguage { get; set; }
        private string UpEndPointAddress => GOOGLE_UP_URL
                                         + $"key={ApiKey}&pair={pair}&lang={CultureLanguage.Name}&pFilter=2&maxAlternatives=1"
                                         + $"&app=chromium&continuous";
        private string DownEndPointAddress => GOOGLE_DOWN_URL
                                           + $"key={ApiKey}&pair={pair}&output=pb";

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

        private string GenerateRequestKey()
        {
            var bytes = new byte[sizeof(Int64)];
            Gen.GetBytes(bytes);
            return BitConverter.ToString(bytes)
                               .Replace("-", string.Empty);
        }

        public async Task<string> SpeechToTextAsync(byte[] buf_flac, int sampleRateHz)
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, UpEndPointAddress);
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

        MemoryStream[] streamsArray = new MemoryStream[12];

        public static void CopyStream(Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[32768];
            int read;
            while (bytes > 0 &&
                   (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }

        public async Task<IEnumerable<GoogleTextResponse>> RecognizeAsync(Stream stream, int SampleRateHz, int Channels, AudioFormat format)
        {
            if (format != AudioFormat.FLAC)
                throw new InvalidCastException("Format not supported for GoogleSpeech Class");

            //int dived = (int)stream.Length / streamsArray.Length;
            //for (int i = 0; i < streamsArray.Length; i++)
            //{
            //    streamsArray[i] = new MemoryStream();
            //    if (i == streamsArray.Length - 1)
            //        CopyStream(stream, streamsArray[i], (int)(stream.Length - stream.Position));
            //    else
            //        CopyStream(stream, streamsArray[i], dived);
            //}

            pair = GenerateRequestKey();
            var postTask = await httpPostStream(stream, SampleRateHz);
            await httpGetStream();


            return new List<GoogleTextResponse>();
            //HttpResponseMessage response = await httpClient.SendAsync(req)
            //                                               .ConfigureAwait(false);
            //List<GoogleTextResponse> listText = new List<GoogleTextResponse>();
            //string response_str = await response.Content.ReadAsStringAsync();
            //string[] list_resut = response_str.Split('\n');
            //foreach (var json_result in list_resut)
            //{
            //    if (json_result == string.Empty)
            //        continue;

            //    JObject o = JObject.Parse(json_result);
            //    if (o["result"].Count() == 0)
            //        continue;

            //    var result = o["result"][0];
            //    foreach (var alternative in result["alternative"])
            //    {
            //        var textResponse = new GoogleTextResponse();
            //        textResponse.Text = (string)alternative["transcript"];
            //        textResponse.Confidence = (double?)alternative["confidence"] ?? double.NaN;
            //        listText.Add(textResponse);
            //    }
            //}

            //return listText.OrderByDescending(r => r.Confidence)
            //               .ToList();
        }

        private async Task httpGetStream()
        {
            MemoryStream memoryStream = new MemoryStream();
            var response = await http2Client.Send(new Uri(DownEndPointAddress), HttpMethod.Get, null, memoryStream);
            //var connectionStream = await http2Client.StreamManager.Get(0);
            response.Stream.OnFrameReceived += (frame) =>
            {
                // Watch for an ack'd settings frame after we sent the frame with no push promise
                if (frame.Type == FrameType.Data)
                {
                    string textPost = Encoding.Default.GetString((frame as DataFrame).Data);
                    Debug.WriteLine("POST response: " + textPost);
                }
            };
        }

        private async Task<bool> httpPostStream(Stream stream, int SampleRateHz)
        {
            var postUri = new Uri(UpEndPointAddress);
            var settings = new Http2ConnectionSettings(new Uri(UpEndPointAddress));

            http2Client = new Http2Client(settings);

            //await http2Client.Connect();

            var response = await http2Client.Send(postUri, HttpMethod.Post, null, stream);
            //var connectionStream = await http2Client.StreamManager.Get(0);
            response.Stream.OnFrameReceived += (frame) =>
            {
                // Watch for an ack'd settings frame after we sent the frame with no push promise
                if (frame.Type == FrameType.Data)
                {
                    string textPost = Encoding.Default.GetString((frame as DataFrame).Data);
                    Debug.WriteLine("POST response: " + textPost);
                }
            };

            return true;
        }


        private async Task httpGetStream_old(string pair, Task<bool> postTask)
        {
            HttpClient client2 = new HttpClient();
            HttpRequestMessage reqDown = new HttpRequestMessage(HttpMethod.Get, DownEndPointAddress);
            //reqDown.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            var responseDown = await client2.SendAsync(reqDown)
                                            .ConfigureAwait(false);

            using (Stream streamDown = await responseDown.Content.ReadAsStreamAsync())
                //using (Stream decompressed = new GZipStream(streamDown, CompressionMode.Decompress))
                while (!postTask.IsCompleted)
                {
                    using (StreamReader reader = new StreamReader(streamDown))
                    {
                        while (!reader.EndOfStream)
                        {
                            var textResponse = reader.ReadToEnd();

                        }
                    }
                    Thread.Sleep(50);
                }
        }

        

        private async Task<bool> httpPostStream_old(Stream[] streams, string pair, int SampleRateHz)
        {
            //HttpRequestMessage reqUp = new HttpRequestMessage(HttpMethod.Post, UpEndPointAddress);
            //reqUp.Headers.TransferEncodingChunked = true;
            //reqUp.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            //reqUp.Content = new StreamContent(voiceStream);
            //reqUp.Content.Headers.Add("Content-Type", $"audio/x-flac; rate={SampleRateHz}");

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(UpEndPointAddress);
            httpWebRequest.Method = "POST";
            //httpWebRequest.SendChunked = true;
            httpWebRequest.AllowWriteStreamBuffering = true;
            httpWebRequest.AllowReadStreamBuffering = false;
            httpWebRequest.ContentType = $"audio/x-flac; rate={SampleRateHz}";

            await Task.Run(() =>
            {
                Stream st = httpWebRequest.GetRequestStream();

                //reqUp.Content.Headers.ContentLength = stream.Length;

                for (int i = 0; i < streams.Length; i++)
                {
                    //voiceStream.SetLength(0);
                    streams[i].Seek(0, SeekOrigin.Begin);
                    byte[] buffer = new byte[streams[i].Length];
                    streams[i].Read(buffer, 0, buffer.Length);
                    st.Write(buffer, 0, buffer.Length);
                    st.Flush();
                    //var response = await httpClient.SendAsync(reqUp)
                    //                               .ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
            return response.StatusCode == HttpStatusCode.OK;
        }

        private bool GetResponseUp(Task<HttpResponseMessage> task)
        {
            HttpResponseMessage response = task.Result;
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}

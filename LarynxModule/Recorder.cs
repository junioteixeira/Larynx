using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarynxModule
{
    public class Recorder
    {

        public Recorder()
        {
            GoogleCredential credential =
               GoogleCredential.FromAccessToken("AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw");
            var channel = new Grpc.Core.Channel(SpeechClient.DefaultEndpoint,
                credential.ToChannelCredentials());
            var speech = SpeechClient.Create();
            speech.Chann
            var config = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Flac,
                SampleRateHertz = 44100,
                LanguageCode = LanguageCodes.Portuguese.Brazil,
                EnableWordTimeOffsets = true
            };
            var audio = RecognitionAudio.FromFile("apenas_um_show.flac");

            var response = speech.Recognize(config, audio);

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    Console.WriteLine($"Transcript: { alternative.Transcript}");
                    Console.WriteLine("Word details:");
                    Console.WriteLine($" Word count:{alternative.Words.Count}");
                    foreach (var item in alternative.Words)
                    {
                        Console.WriteLine($"  {item.Word}");
                        Console.WriteLine($"    WordStartTime: {item.StartTime}");
                        Console.WriteLine($"    WordEndTime: {item.EndTime}");
                    }
                }
            }
        }
    }      
}

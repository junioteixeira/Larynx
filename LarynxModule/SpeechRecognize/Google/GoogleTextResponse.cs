using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarynxModule.SpeechRecognize.Google
{
    public class GoogleTextResponse : ITextResponse
    {
        public string Text { get ; set ; }
        public double Confidence { get ; set ; }
    }
}

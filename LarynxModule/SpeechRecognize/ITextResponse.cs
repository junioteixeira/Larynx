using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarynxModule.SpeechRecognize
{
    interface ITextResponse
    {
        string Text { get; set; }
        double Confidence { get; set; }
    }
}

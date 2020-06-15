using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarynxModule.SpeechRecognize
{
    public class SpeechTextEventArgs : EventArgs
    {
        public string Transcript { get; private set; }

        public SpeechTextEventArgs(string Transcript)
        {
            this.Transcript = Transcript;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LarynxModule.SpeechRecognize
{
    public class AudioFormatException : Exception
    {
        public AudioFormatException()
        {
        }

        public AudioFormatException(string message) : base(message)
        {
        }

        public AudioFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AudioFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

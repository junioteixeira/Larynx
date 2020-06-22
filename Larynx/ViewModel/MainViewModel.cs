using LarynxModule.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larynx.ViewModel
{
    public class MainViewModel
    {
        private LarynxEngine larynxEngine = new LarynxEngine();

        public LarynxEngine Engine => larynxEngine;

        public MicrophoneViewModel MicrophoneViewModel { get; private set; }

        public MainViewModel()
        {
            MicrophoneViewModel = new MicrophoneViewModel(larynxEngine);
        }
    }
}

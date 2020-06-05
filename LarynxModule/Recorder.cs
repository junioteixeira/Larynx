using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarynxModule
{
    public class Recorder
    {
        private WaveIn waveSource = new WaveIn();

        public Recorder()
        {
            waveSource.WaveFormat = new WaveFormat(44100, 1);

            waveSource.DataAvailable += waveSource_DataAvailable;
            waveSource.RecordingStopped += waveSource_RecordingStopped;

            waveSource.StartRecording();
        }

        private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("Stopped");
        }

        private void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            Debug.WriteLine("Total available: "+e.BytesRecorded);

        }
    }
}

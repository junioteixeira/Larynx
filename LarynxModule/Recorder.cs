using NAudio.CoreAudioApi;
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
        private WasapiCapture capture;
        private int SamplesPerSeconds = 44100;


        public Recorder()
        {
            capture = new WasapiCapture();

            capture.WaveFormat =  WaveFormat.CreateIeeeFloatWaveFormat(SamplesPerSeconds, 1);

            capture.DataAvailable += Capture_DataAvailable;
            capture.StartRecording();
            //waveSource.WaveFormat = new WaveFormat(44100, 1);

            //waveSource.DataAvailable += waveSource_DataAvailable;
            //waveSource.RecordingStopped += waveSource_RecordingStopped;

            //waveSource.StartRecording();
        }

        double ewma_threshold = 300;
        double dynamic_alpha = 0.15;
        double dynamic_rms_ratio = 1.5;

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            //Debug.WriteLine("Total available: " + e.BytesRecorded);
            float[] samples = new float[e.BytesRecorded / 4]; // Each sample has 16bits
            Buffer.BlockCopy(e.Buffer, 0, samples, 0, samples.Length);

            double signal_rms = Math.Sqrt(calc_energy(samples) / samples.Length);
            if (signal_rms > ewma_threshold)
                Debug.WriteLine("Voz");

            double seconds = (double)samples.Length / SamplesPerSeconds;
            double alpha = Math.Pow(dynamic_alpha, seconds);
            double target_energy = signal_rms * dynamic_rms_ratio;

            ewma_threshold = ewma_threshold * alpha + target_energy * (1 - alpha);

            /*
             # detect whether speaking has started on audio input
            energy = audioop.rms(buffer, source.SAMPLE_WIDTH)  # energy of the audio signal
            if energy > self.energy_threshold: break
            damping = self.dynamic_energy_adjustment_damping ** seconds_per_buffer  # account for different chunk sizes and rates
            target_energy = energy * self.dynamic_energy_ratio
            self.energy_threshold = self.energy_threshold * damping + target_energy * (1 - damping)

             */
        }

        private double calc_energy(float[] samples)
        {
            double energy = 0;
            for (int i = 0; i < samples.Length; i++)
                energy += (double)samples[i] * samples[i];

            return energy;
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

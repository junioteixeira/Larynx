using LarynxModule.SpeechRecognize.Google;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;

namespace LarynxModule.Engine
{
    public class LarynxEngine
    {
        private InputSimulator input = new InputSimulator();
        private GoogleSpeech speechModule = new GoogleSpeech();
        private CancellationTokenSource speechStreamCancellation = null;
        private Task speechTask = null;
        private bool detected_newLine = false;

        public LarynxEngineState State { get; private set; }

        public LarynxEngine()
        {
            State = LarynxEngineState.Stopped;
            speechModule.TextRecognized += SpeechModule_TextRecognized;
        }

        private void SpeechModule_TextRecognized(object sender, SpeechRecognize.SpeechTextEventArgs e)
        {
            if (State == LarynxEngineState.Paused)
                return; // Ignore transcripts

            string text = string.Empty;
            if (detected_newLine)
                text = EngineWords.ReplacePunctuation(e.Transcript.Trim());
            else
                text = EngineWords.ReplacePunctuation(e.Transcript);
            detected_newLine = text.Contains("\n");
            input.Keyboard.TextEntry(text);
        }

        public List<string> GetInputDevices()
        {
            List<string> devicesName = new List<string>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
                devicesName.Add(WaveIn.GetCapabilities(i).ProductName);

            return devicesName;
        }

        public bool PauseEngine()
        {
            if (State != LarynxEngineState.Running || speechTask == null)
                return false;
            else
            {
                State = LarynxEngineState.Paused;
                return true;
            }
        }

        public bool ResumeEngine()
        {
            if (State != LarynxEngineState.Paused || speechTask == null)
                return false;
            else
            {
                State = LarynxEngineState.Running;
                return true;
            }
        }

        public bool StartEngine()
        {
            return StartEngine(string.Empty);
        }

        public bool StartEngine(string DeviceName)
        {
            if (State == LarynxEngineState.Running || State == LarynxEngineState.Starting)
                return false;

            State = LarynxEngineState.Starting;
            speechStreamCancellation = new CancellationTokenSource();

            int deviceSelected = -1;
            if (DeviceName != string.Empty)
            {
                for (int i = 0; i < WaveIn.DeviceCount; i++)
                    if (WaveIn.GetCapabilities(i).ProductName == DeviceName)
                        deviceSelected = i;

                if (deviceSelected == -1)
                    return false; // Do Exception after
            }
            else
                deviceSelected = 0;

            WaveInEvent waveIn = new WaveInEvent();
            waveIn.DeviceNumber = deviceSelected;
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            speechTask = speechModule.RecognizeStreaming(waveIn, speechStreamCancellation.Token);
            if (speechTask.Status == TaskStatus.Running)
            {
                State = LarynxEngineState.Running;
                return true;
            }
            else
            {
                State = LarynxEngineState.Stopped;
                return false;
            }

        }

        public void StopEngine()
        {
            if (State != LarynxEngineState.Running)
                return;
            if (speechStreamCancellation != null)
                speechStreamCancellation.Cancel();
            State = LarynxEngineState.Stopped;
        }
    }
}

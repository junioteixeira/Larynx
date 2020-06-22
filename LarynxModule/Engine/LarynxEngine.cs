using LarynxModule.SpeechRecognize.Google;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;

namespace LarynxModule.Engine
{
    public class LarynxEngine : INotifyPropertyChanged
    {
        private InputSimulator input = new InputSimulator();
        private GoogleSpeech speechModule = new GoogleSpeech();
        private CancellationTokenSource speechStreamCancellation = null;
        private Task speechTask = null;
        private bool detected_newLine = false;

        public event PropertyChangedEventHandler PropertyChanged;
        private string deviceInName = string.Empty;
        private LarynxEngineState state = LarynxEngineState.Stopped;



        public LarynxEngineState State
        {
            get => this.state;
            private set
            {
                if (value != this.state)
                {
                    this.state = value;
                    OnNotifyPropertyChanged();
                }
            }
        }

        public string DeviceInName
        {
            get => this.deviceInName;
            set
            {
                if (value != this.deviceInName && value != null)
                {
                    this.deviceInName = value;
                    OnNotifyPropertyChanged();
                }
            }
        }
        
        protected virtual void OnNotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public LarynxEngine()
        {
            State = LarynxEngineState.Stopped;
            PropertyChanged += LarynxEngine_PropertyChanged;
            speechModule.TextRecognized += SpeechModule_TextRecognized;
            LogHelper.ErrorEngine += LogHelper_ErrorEngine;
        }

        private async void LarynxEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceInName) && State == LarynxEngineState.Running)
            {
                StopEngine();
                await Task.Delay(200);
                StartEngine();
            }
        }

        private void LogHelper_ErrorEngine(object sender, EventArgs e)
        {
            this.StopEngine();
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
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            foreach (var device in devices)
                devicesName.Add(device.DeviceFriendlyName);

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
            if (State == LarynxEngineState.Running || State == LarynxEngineState.Starting)
                return false;

            try
            {
                State = LarynxEngineState.Starting;
                speechStreamCancellation = new CancellationTokenSource();

                int deviceSelected = -1;
                if (DeviceInName != string.Empty)
                {
                    MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                    var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                    for (int i = 0; i < devices.Count; i++)
                        if (devices[i].DeviceFriendlyName == DeviceInName)
                            deviceSelected = i;

                    if (deviceSelected == -1)
                        throw new Exception("Microfone selecionado não encontrado.");
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
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, ex.ToString());
                State = LarynxEngineState.Stopped;
                return false;
            }
        }

        public bool StopEngine()
        {
            if (State == LarynxEngineState.Stopped)
                return false;
            if (speechStreamCancellation != null)
                speechStreamCancellation.Cancel();
            State = LarynxEngineState.Stopped;
            return true;
        }
    }
}

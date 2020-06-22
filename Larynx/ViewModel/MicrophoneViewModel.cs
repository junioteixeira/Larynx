using Larynx.Views;
using LarynxModule.Engine;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Larynx.ViewModel
{
    public class MicrophoneViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public LarynxEngine _engine;
        public LarynxEngine Engine
        {
            get => _engine;
            set
            {
                if (value == _engine || value == null)
                    return;

                _engine = value;
                OnNotifyPropertyChanged();
            }
        }

        private ObservableCollection<string> _microphoneList;
        private string _selectedMicrophone;

        public ObservableCollection<string> MicrophoneList
        {
            get => _microphoneList;
            set
            {
                _microphoneList = value;
                OnNotifyPropertyChanged();
            }
        }

        public string SelectedMicrophone
        {
            get => _selectedMicrophone;
            set
            {
                if (value == null || value.Equals(_selectedMicrophone)) return;

                _selectedMicrophone = value;
                Engine.DeviceInName = _selectedMicrophone;
                OnNotifyPropertyChanged();
            }
        }

        private string _statusEngine;
        public string StatusEngine
        {
            get => _statusEngine;
            set
            {
                if (value == null || value.Equals(_statusEngine)) return;

                _statusEngine = value;
                OnNotifyPropertyChanged();
            }
        }

        protected virtual void OnNotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engine">Received from MainViewModel</param>
        public MicrophoneViewModel(LarynxEngine engine)
        {
            Engine = engine;
            MicrophoneList = new ObservableCollection<string>(_engine.GetInputDevices());
            if (MicrophoneList.Count > 0)
                SelectedMicrophone = MicrophoneList[0];
            
            Engine.PropertyChanged += Engine_PropertyChanged;
            ApplyStateEngine(Engine.State);
        }

        private void ApplyStateEngine(LarynxEngineState state)
        {
            if (state == LarynxEngineState.Stopped)
                StatusEngine = "Iniciar";
            else if (state == LarynxEngineState.Starting)
                StatusEngine = "Iniciando...";
            else if (state == LarynxEngineState.Paused)
                StatusEngine = "Recomeçar";
            else // Running
                StatusEngine = "Pausar";
        }

        private void Engine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
                ApplyStateEngine(Engine.State);
        }
    }
}

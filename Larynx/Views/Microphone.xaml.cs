using Larynx.ViewModel;
using LarynxModule.Engine;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Larynx.Views
{
    /// <summary>
    /// Interação lógica para Microphone.xam
    /// </summary>
    public partial class Microphone : UserControl
    {
        private MicrophoneViewModel vm; // Received from mainwindow as datacontext

        public Microphone()
        {
            InitializeComponent();
        }

        private async void EngineButton_Click(object sender, RoutedEventArgs e)
        {
            vm = DataContext as MicrophoneViewModel;
            var engine = vm.Engine;
            if (engine.DeviceInName == string.Empty)
            {
                var dialogError = new DialogMessage
                {
                    Message = { Text = "Nenhum microfone detectado." }
                };
                await DialogHost.Show(dialogError);
                return;
            }

            switch (engine.State)
            {
                case LarynxEngineState.Stopped:
                    if (engine.StartEngine())
                        EngineButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop; //! Use MVVM to do this
                    else
                    {
                        var dialogError = new DialogMessage
                        {
                            Message = { Text = "Falha ao iniciar módulos!\nRecomendado que reinicie o aplicativo\ne consulte os logs." }
                        };
                        await DialogHost.Show(dialogError);
                    }
                    break;
                case LarynxEngineState.Running:
                    if (engine.StopEngine())
                        EngineButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play; //! Use MVVM to do this
                    else
                    {
                        var dialogError = new DialogMessage
                        {
                            Message = { Text = "Falha ao iniciar módulos!\nRecomendado que reinicie o aplicativo\ne consulte os logs." }
                        };
                        await DialogHost.Show(dialogError);
                    }
                    break;
                case LarynxEngineState.Paused: // Future use - Resume and Pause
                    if (engine.ResumeEngine())
                        EngineButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause; //! Use MVVM to do this
                    else
                    {
                        var dialogError = new DialogMessage
                        {
                            Message = { Text = "Falha ao iniciar módulos!\nRecomendado que reinicie o aplicativo\ne consulte os logs." }
                        };
                        await DialogHost.Show(dialogError);
                    }
                    break;

                case LarynxEngineState.Starting: // Future case
                default:
                    break;
            }

            
        }
    }
}

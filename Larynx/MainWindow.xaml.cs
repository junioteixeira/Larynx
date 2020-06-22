using Larynx.Views;
using LarynxModule;
using LarynxModule.Engine;
using LarynxModule.SpeechRecognize;
using LarynxModule.SpeechRecognize.Google;
using MaterialDesignThemes.Wpf;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
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

namespace Larynx
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {

        // *Teclado virtual
        // *Configurar microfone
        // *Configurar caps ou apenas minúsculo
        // *Atalhos 
        //	->Volume por atalho
        //	->Passar música
        //	->Voltar
        //	->Pausar
        //  ->Caps Lock
        // *Configuração avançada de Flac e codificação
        // *Statusbar de ping

        public MainWindow()
        {
            InitializeComponent();
            LogHelper.ErrorEngine += LogHelper_ErrorEngine;
        }

        private void LogHelper_ErrorEngine(object sender, EventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                var dialogError = new DialogMessage
                {
                    Message = { Text = "Falha crítica!\nRecomendado que reinicie o aplicativo\ne consulte os logs." }
                };
                await DialogHost.Show(dialogError);
            });
        }
    }
}

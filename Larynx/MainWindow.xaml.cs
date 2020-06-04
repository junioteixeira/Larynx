using LarynxModule;
using LarynxModule.SpeechRecognize.Google;
using System;
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
        // *Configuração avançada de Flac e codificação
        // *Statusbar de ping

        public MainWindow()
        {
            InitializeComponent();
            //FileStream fs1 = new FileStream("apenas_um_show.flac", FileMode.Open);
            //byte[] flacAudio1 = new byte[fs1.Length];

            //FileStream fs2 = new FileStream("text.flac", FileMode.Open);
            //byte[] flacAudio2 = new byte[fs2.Length];

            //fs1.Read(flacAudio1, 0, flacAudio1.Length);
            //fs2.Read(flacAudio2, 0, flacAudio2.Length);
            //LarynxGoogleSpeech larynxGoogleSpeech = new LarynxGoogleSpeech();
            //string text = larynxGoogleSpeech.SpeechToTextAsync(flacAudio1, 44100).Result;
            //string text2 = larynxGoogleSpeech.SpeechToTextAsync(flacAudio2, 44100).Result;
            //MessageBox.Show(text);

            GoogleSpeech googleSpeech = new GoogleSpeech();
            FileStream fs1 = new FileStream(Path.Combine("FlacTests", "apenas_um_show.flac"), FileMode.Open);
            googleSpeech.Recognize(fs1, 44100, 1, LarynxModule.SpeechRecognize.AudioFormat.FLAC);
        }
    }
}

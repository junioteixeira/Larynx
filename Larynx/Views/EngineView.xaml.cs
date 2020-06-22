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
    /// Interação lógica para EngineView.xam
    /// </summary>
    public partial class EngineView : UserControl
    {
        

        public EngineView()
        {
            InitializeComponent();
        }

        private void KeyOnOff_KeyUp(object sender, KeyEventArgs e) // Optimize - future
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
                KeyOnOff.Text = string.Empty;
            else
            {
                if (KeyOnOff.Text != string.Empty)
                    KeyOnOff.Text += $" + {e.Key.ToString()}";
                else
                    KeyOnOff.Text = e.Key.ToString();
            }
        }
    }
}

using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interação lógica para MainHeader.xam
    /// </summary>
    public partial class MainBottom : UserControl
    {
        public MainBottom()
        {
            InitializeComponent();
        }

        private void Git_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Metalus/Larynx");
        }

        private async void CreditsPopup_OnClick(object sender, RoutedEventArgs e)
        {
            await DialogHost.Show(new CreditsDialog());
        }
    }
}

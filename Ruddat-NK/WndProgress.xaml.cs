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
using System.Windows.Shapes;

namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für WndProgress.xaml
    /// </summary>
    public partial class WndProgress : Window
    {
        // Nchrichten senden
        private Timeline messageSender;

        public WndProgress()
        {
            InitializeComponent();

            Timeline sender = new Timeline();
            sender.NachrichtGesendet += OnNachrichtEmpfangen;

            Process();
        }

        private void OnNachrichtEmpfangen(string nachricht)
        {
            // Hier können Sie die empfangene Nachricht verarbeiten
            MessageBox.Show(nachricht);
        }

        // Create a Delegate that matches the Signature of the ProgressBar's SetValue method
        // public delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        private void Process()
        {
            //Configure the ProgressBar
            PBar.Minimum = 0;
            PBar.Maximum = 100;
            PBar.Value = 0;
        }


        public void SetData(double AdValue, double AdMaximum)
        {
            PBar.Maximum = AdMaximum;
            PBar.Value = AdValue;
        }
    }
}

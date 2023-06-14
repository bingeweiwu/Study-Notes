using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace MS.Internal.SulpHur.SulpHurClient
{
    /// <summary>
    /// Capture State enumeration
    /// </summary>
    public enum CaptureState
    {
        None,
        Processing,
        Ok,
        Error,
        Warning
    }

    /// <summary>
    /// Interaction logic for StateForm.xaml
    /// </summary>
    public partial class StateForm : Window
    {
        // private members
        private static readonly Uri ThinkingUri = new Uri("/Images/Processing.ico", UriKind.Relative);
        private static readonly Uri WarningUri = new Uri("/Images/Warning.ico", UriKind.Relative);
        private static readonly Uri OkUri = new Uri("/Images/OK.ico", UriKind.Relative);
        private static readonly Uri ErrorUri = new Uri("/Images/Error.ico", UriKind.Relative);
        private CaptureState state = CaptureState.None;
        public StateForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets results of the analysis
        /// </summary>
        public CaptureState State
        {
            set
            {
                this.state = value;
                this.StateImage.BeginInit();
                switch (state)
                {
                    case CaptureState.Processing: this.StateImage.Source = new BitmapImage(ThinkingUri); break;
                    case CaptureState.Ok: this.StateImage.Source = new BitmapImage(OkUri); break;
                    case CaptureState.Error: this.StateImage.Source = new BitmapImage(ErrorUri); break;
                    case CaptureState.Warning: this.StateImage.Source = new BitmapImage(WarningUri); break;
                }
                this.StateImage.EndInit();
            }
            get
            {
                return this.state;
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}

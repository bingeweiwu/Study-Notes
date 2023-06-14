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
    public enum UploadState
    {
        Processing,
        Ok,
        Error,
        Abandon
    }

    /// <summary>
    /// Interaction logic for StateForm.xaml
    /// </summary>
    public partial class UploadStateForm : Window
    {
        // private members
        private static readonly Uri ThinkingUri = new Uri("/Images/Processing.ico", UriKind.Relative);
        private static readonly Uri AbandonUri = new Uri("/Images/Warning.ico", UriKind.Relative);
        private static readonly Uri OkUri = new Uri("/Images/OK.ico", UriKind.Relative);
        private static readonly Uri ErrorUri = new Uri("/Images/Error.ico", UriKind.Relative);
        private UploadState state = UploadState.Processing;
        public UploadStateForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets results of the analysis
        /// </summary>
        public UploadState State
        {
            set
            {
                this.state = value;
                this.UploadStateImage.BeginInit();
                switch (state)
                {
                    case UploadState.Processing: this.UploadStateImage.Source = new BitmapImage(ThinkingUri); break;
                    case UploadState.Ok: this.UploadStateImage.Source = new BitmapImage(OkUri); break;
                    case UploadState.Error: this.UploadStateImage.Source = new BitmapImage(ErrorUri); break;
                    case UploadState.Abandon: this.UploadStateImage.Source = new BitmapImage(AbandonUri); break;
                }
                this.UploadStateImage.EndInit();
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

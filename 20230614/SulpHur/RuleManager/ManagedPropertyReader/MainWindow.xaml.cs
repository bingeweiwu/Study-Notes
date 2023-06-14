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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Automation;

namespace ManagedPropertyReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ManagedInfo view;
        public MainWindow()
        {
            InitializeComponent();
            view = new ManagedInfo();
            this.DataContext = view;
        }

        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            IntPtr ptr = new IntPtr(Convert.ToInt32(this.textBoxHandle.Text.Trim(), 16));
            AutomationElement current = AutomationElement.FromHandle(ptr);
            object obj = Native.GetProperty(ptr, this.tbPropertyName.Text.Trim());
            if (obj != null)
            {
                view.PropertyValue = obj;
            }
            else
            {
                view.PropertyValue = "NULL";
            }
        }

        protected bool IsManagedControl(AutomationElement window)
        {
            if (window.Current.FrameworkId.IndexOf("wpf", StringComparison.InvariantCultureIgnoreCase) != -1
                || window.Current.FrameworkId.IndexOf("winform", StringComparison.InvariantCultureIgnoreCase) != -1)
                return true;
            return false;
        }
    }
}

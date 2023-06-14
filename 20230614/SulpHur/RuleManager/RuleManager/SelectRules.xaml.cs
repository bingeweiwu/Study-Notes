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

namespace RuleManager
{
    /// <summary>
    /// Interaction logic for SelectRules.xaml
    /// </summary>
    public partial class SelectRules : Window
    {
        MainView view;
        //IAdminOperations wrapper;
        string buildNo;
        //public SelectRules(MainView view,IAdminOperations wrapper,string buildNo)
        //{
        //    InitializeComponent();

        //    this.view = view;
        //    this.DataContext = view;

        //    view.RuleList1.Clear();
        //    foreach (ComplianceRule cr in view.RuleList) {
        //        ComplianceRule temp = new ComplianceRule();
        //        temp.Name = cr.Name;
        //        temp.IsSelected = false;
        //        view.RuleList1.Add(temp);
        //    }
        //    this.wrapper = wrapper;
        //    this.buildNo = buildNo;
        //}

        public SelectRules() {
            InitializeComponent();
        }

        private void btnRescanBySelectedRules_Click(object sender, RoutedEventArgs e)
        {
            List<string> rules = new List<string>();
            foreach (ComplianceRule cr in view.RuleList1) {
                if (cr.IsSelected) {
                    rules.Add(cr.Name);
                }
            }
            //wrapper.ReScanByBuildNo(buildNo, rules);
            this.Close();
        }
    }
}

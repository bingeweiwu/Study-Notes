using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Configuration;

namespace MS.Internal.SulpHur.UICompliance
{

   
    abstract public class UIComplianceRuleBase
    {
        private bool enabled = false;
        public string lan = "ENU";
        public bool isOSidentity = true;

        public UIComplianceRuleBase()
        {
            this.IsEnabled = ConfigurationManager.AppSettings[this.Name.Trim()+ ".isEnabled"] == "true";
        }

        abstract public string Name
        {
            get;
        }


        abstract public string Description
        {
            get;
        }

        public bool IsEnabled
        {
            get { return enabled; }
            set { enabled = value;}
        }

        abstract public List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls);

        abstract public List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements);
    }

    [DataContract]
    public class ComplianceRule : INotifyPropertyChanged
    {
        public ComplianceRule() { }

        private string name;
        private string description;
        private bool isEnable;

        [DataMember]
        public bool IsEnable
        {
            get { return isEnable; }
            set { isEnable = value; OnPropertyChanged(new PropertyChangedEventArgs("IsEnable")); }
        }

        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(new PropertyChangedEventArgs("Name")); }
        }

        [DataMember]
        public string Description
        {
            get { return description; }
            set { description = value; OnPropertyChanged(new PropertyChangedEventArgs("Description")); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}

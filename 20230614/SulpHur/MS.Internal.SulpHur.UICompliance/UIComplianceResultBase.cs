using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace MS.Internal.SulpHur.UICompliance
{
    /// <summary>
    /// Rules' type
    /// </summary>
    public enum ResultType
    {
        Info = 0,
        Pass = 1,
        Warning = 2,
        Fail = 3,
        Disaster = 4,
    }

    public class UIComplianceResultBase
    {
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        private string ruleName;

        public string RuleName
        {
            get { return ruleName; }
            set { ruleName = value; }
        }
        private ResultType type;

        public ResultType Type
        {
            get { return type; }
            set { type = value; }
        }
        private List<ElementInformation> controls;

        public List<ElementInformation> Controls
        {
            get { return controls; }
            set { controls = value; }
        }
        private List<WebElementInfo> webTags;

        public List<WebElementInfo> WebTags
        {
            get { return webTags; }
            set { webTags = value; }
        }
        private Bitmap image;

        public Bitmap Image
        {
            get { return image; }
            set { image = value; }
        }
        private int uiid;

        public int UIID
        {
            get { return uiid; }
            set { uiid = value; }
        }

        public UIComplianceResultBase(ResultType type, string message, string ruleName) {
            this.type = type;
            this.message = message;
            this.ruleName = ruleName;
            controls = new List<ElementInformation>();
            webTags = new List<WebElementInfo>();
        }

        public void AddRelatedControls(ElementInformation ei) {
            this.controls.Add(ei);
        }

        public void AddRelatedTags(WebElementInfo ei)
        {
            this.webTags.Add(ei);
        }
    }

    [DataContract]
    public class ComplianceResult : INotifyPropertyChanged
    {
        private string type;
        private string message;
        private Bitmap image;
        private string ruleName;

        public ComplianceResult() { }
        public ComplianceResult(string type, string msg, Bitmap bitMap)
        {
            this.type = type;
            this.message = msg;
            this.Image = bitMap;
        }
        private ElementInformation root;

        public ElementInformation Root
        {
            get { return root; }
            set { root = value; }
        }

        private List<ElementInformation> controls;

        public List<ElementInformation> Controls
        {
            get { return controls; }
            set { controls = value; }
        }

        [DataMember]
        public string Type
        {
            get { return type; }
            set { type = value; OnPropertyChanged(new PropertyChangedEventArgs("Type")); }
        }

        [DataMember]
        public Bitmap Image
        {
            get { return image; }
            set { image = value; OnPropertyChanged(new PropertyChangedEventArgs("Image")); }
        }

        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged(new PropertyChangedEventArgs("Message")); }
        }

        [DataMember]
        public string RuleName
        {
            get { return ruleName; }
            set { ruleName = value; OnPropertyChanged(new PropertyChangedEventArgs("RuleName")); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}

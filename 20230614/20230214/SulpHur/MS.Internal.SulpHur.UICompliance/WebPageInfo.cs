using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mshtml;
using System.Runtime.Serialization;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using MS.Internal.SulpHur.Utilities;

namespace MS.Internal.SulpHur.UICompliance
{
    [DataContract]
    public class WebPageInfo
    {
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public int PageSummaryHashCode { get; set; }
        [DataMember]
        public WebElementInfo webElementInfo { get; set; }
    }

    [DataContract]
    public class WebElementInfo
    {
        public WebElementInfo() { }
        public WebElementInfo(IHTMLElement htmlElement)
        {
            try
            {
                if(htmlElement.id!=null)
                    this.Id = htmlElement.id;
                dynamic name = htmlElement.getAttribute("Name") ?? "";
                this.Name = name.GetType() == typeof(string) ? (string)name : "";
                this.TagName = htmlElement.tagName;
                dynamic type = htmlElement.getAttribute("Type") ?? "";
                this.Type = type.GetType() == typeof(string) ? (string)type : "";
                dynamic value = htmlElement.getAttribute("Value") ?? "";
                this.Value = value.GetType() == typeof(string) ? (string)value : "";

                IHTMLElement2 ele2 = (IHTMLElement2)htmlElement;
                IHTMLRect rect = ele2.getBoundingClientRect();
                this.BoundingRectangle = new Rectangle(rect.left, rect.top, rect.right-rect.left, rect.bottom-rect.top);
                dynamic accessKey = htmlElement.getAttribute("AccessKey") ?? "";
                this.AccessKey = accessKey.GetType() == typeof(string) ? (string)accessKey : "";
                dynamic tabIndex = htmlElement.getAttribute("TabIndex") ?? 0;
                this.TabIndex = tabIndex.GetType() == typeof(int) ? (int)tabIndex : 0;

                if (htmlElement.innerText != null)
                    this.InnerText = NullRemover(htmlElement.innerText);
                if (htmlElement.innerHTML != null)
                    this.InnerHTML = NullRemover(htmlElement.innerHTML);
            }
            catch (Exception e) {
                Trace.WriteLine(e);
            }
        }

        private string NullRemover(string DataStream)
        {
            List<char> array = new List<char>();
            foreach (char c in DataStream)
            {
                if (c != 0x00) array.Add(c);
            }
            return new String(array.ToArray());
        }

        internal static Rectangle GetHtmlElementBounds(IHTMLElement element)
        {
            int offsetLeft = element.offsetLeft;
            int offsetTop = element.offsetTop;
            for (IHTMLElement element2 = element.parentElement; element2 != null; element2 = element2.parentElement)
            {
                offsetLeft += element2.offsetLeft;
                offsetTop += element2.offsetTop;
            }
            int width = element.offsetWidth / 2;
            return new Rectangle(offsetLeft, offsetTop, width, element.offsetHeight / 2);
        }

        [DataMember]
        public string TagName { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public Rectangle BoundingRectangle { get; set; }
        [DataMember]
        public string AccessKey { get; set; }
        [DataMember]
        public int TabIndex { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string InnerText { get; set; }
        [DataMember]
        public string InnerHTML { get; set; }
        [DataMember]
        public bool IsEnable { get; set; }
        [DataMember]
        public bool IsVisible { get; set; }

        [DataMember]
        public List<WebElementInfo> Children { get; set; }

        public WebElementInfo Parent { get; set; }
    }
}

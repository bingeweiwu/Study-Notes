using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Net;
using System.Xml.Linq;

namespace MS.Internal.SulpHur.UICompliance
{
    [DataContract]
    public class AdditionInformations
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public IPAddress IP { get; set; }

        [DataMember]
        public string MacAddress { get; set; }

        [DataMember]
        public string ComputerName { get; set; }

        [DataMember]
        public string OSType { get; set; }

        [DataMember]
        public string OSLanguage { get; set; }

        [DataMember]
        public Version ProductVersion { get; set; }

        [DataMember]
        public string ProductLanguage { get; set; }

        [DataMember]
        public string BuildType { get; set; }
    }

    [DataContract]
    public class UploadResults
    {
        [DataMember]
        public UploadResultType Type { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string id { get; set; }
    }

    [DataContract]
    public enum UploadResultType
    {
        [EnumMember]
        UpLoaded,
        [EnumMember]
        Failed,
        [EnumMember]
        Exists
    }

    [DataContract]
    public class CapturedData
    {
        public CapturedData() { }
        public CapturedData(System.Drawing.Bitmap image, XElement elementInformation)
        {
            this.Image = image;
            this.Ei = MS.Internal.SulpHur.Utilities.ExtensionMethods.FromXElement<ElementInformation>(elementInformation);;
        }

        [DataMember]
        public System.Drawing.Bitmap Image
        {
            get;
            set;
        }

        [DataMember]
        public ElementInformation Ei
        {
            get;
            set;
        }

    }

    [DataContract]
    public class ForegroundData
    {

        [DataMember]
        public string AssemblyName
        {
            get;
            set;
        }

        [DataMember]
        public string AssemblyType
        {
            get;
            set;
        }

        [DataMember]
        public System.Drawing.Bitmap Image
        {
            get;
            set;
        }

        [DataMember]
        public ElementInformation Ei
        {
            get;
            set;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MS.Internal.SulpHur.UICompliance
{
    [DataContract]
    public class WindowPageInfo
    {
        [DataMember]
        public ElementInformation ei { get; set; }

        [DataMember]
        public List<AssemblyInfo> AssemblyInfoList { get; set; }

        [DataMember]
        public string LaunchedFrom { get; set; }

        [DataMember]
        public string WindowHierarchy { get; set; }
    }
}

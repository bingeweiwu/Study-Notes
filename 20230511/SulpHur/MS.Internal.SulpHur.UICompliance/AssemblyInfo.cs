using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MS.Internal.SulpHur.UICompliance
{
    [DataContract]
    public class AssemblyInfo
    {
        public AssemblyInfo() { }
        public AssemblyInfo(string assemblyName, string fullTypeName, bool isPageIdentifier)
        {
            this.AssemblyName = assemblyName;
            this.FullTypeName = fullTypeName;
            this.IsPageIdentifier = isPageIdentifier;
        }

        [DataMember]
        public string AssemblyName { get; set; }
        [DataMember]
        public string FullTypeName { get; set; }
        [DataMember]
        public bool IsPageIdentifier { get; set; }


        public override int GetHashCode()
        {
            return this.AssemblyName.GetHashCode() ^ this.FullTypeName.GetHashCode() ^ this.IsPageIdentifier.GetHashCode();
        }
        public override bool Equals(object objDest)
        {
            if (!(objDest.GetType().Equals(typeof(AssemblyInfo))))
                return false;

            AssemblyInfo objAssemblyInfo = objDest as AssemblyInfo;
            if (this.AssemblyName == objAssemblyInfo.AssemblyName &&
                this.FullTypeName == objAssemblyInfo.FullTypeName &&
                this.IsPageIdentifier == objAssemblyInfo.IsPageIdentifier)
            {
                return true;
            }

            return false;
        }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SulpHurReport
{
    using System;
    using System.Collections.Generic;
    
    public partial class DeletedContent
    {
        public int ContentID { get; set; }
        public Nullable<System.Guid> GUID { get; set; }
        public Nullable<int> ClientID { get; set; }
        public Nullable<int> BuildID { get; set; }
        public string UIName { get; set; }
        public string UIContent { get; set; }
        public byte[] UIScreenShot { get; set; }
        public Nullable<bool> IsWebUI { get; set; }
        public Nullable<System.DateTime> DateUploaded { get; set; }
        public Nullable<int> TraceID { get; set; }
        public Nullable<int> Reserve1 { get; set; }
        public string Reserve2 { get; set; }
        public string Reserve3 { get; set; }
        public string Reserve4 { get; set; }
        public string Reserve5 { get; set; }
        public string LaunchedFrom { get; set; }
        public string WindowHierarchy { get; set; }
        public string AssemblyLinkIDs { get; set; }
    }
}

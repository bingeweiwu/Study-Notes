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
    
    public partial class AssemblyLink
    {
        public int AssemblyLinkID { get; set; }
        public int ContentID { get; set; }
        public int TypeID { get; set; }
        public bool IsPageIdentifier { get; set; }
        public Nullable<int> Reserve1 { get; set; }
        public string Reserve2 { get; set; }
    
        public virtual AssemblyInfo AssemblyInfo { get; set; }
        public virtual UIContent UIContent { get; set; }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MogenDebug
{
    using System;
    using System.Collections.Generic;
    
    public partial class LCX_AssociationAssembly
    {
        public int ID { get; set; }
        public int MainAssemblyID { get; set; }
        public int ReferencedAssemblyID { get; set; }
    
        public virtual LCX_Assemblies LCX_Assemblies { get; set; }
        public virtual LCX_Assemblies LCX_Assemblies1 { get; set; }
    }
}

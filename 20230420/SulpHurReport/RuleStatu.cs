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
    
    public partial class RuleStatu
    {
        public int ContentID { get; set; }
        public int RuleID { get; set; }
        public Nullable<bool> RuleStatus { get; set; }
        public bool IsChecked { get; set; }
        public System.DateTime DateUpdated { get; set; }
    
        public virtual Rule Rule { get; set; }
        public virtual UIContent UIContent { get; set; }
    }
}

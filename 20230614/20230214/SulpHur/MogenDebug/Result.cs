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
    
    public partial class Result
    {
        public int ResultID { get; set; }
        public int ContentID { get; set; }
        public string RelatedControls { get; set; }
        public int RuleID { get; set; }
        public string ResultType { get; set; }
        public byte[] ResultImage { get; set; }
        public string ResultLog { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<int> ReviewFlag { get; set; }
        public string ReviewLog { get; set; }
    
        public virtual Rule Rule { get; set; }
        public virtual UIContent UIContent { get; set; }
    }
}

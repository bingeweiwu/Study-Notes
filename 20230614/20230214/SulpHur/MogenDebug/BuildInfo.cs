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
    
    public partial class BuildInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BuildInfo()
        {
            this.LogExceptions = new HashSet<LogException>();
            this.UIContents = new HashSet<UIContent>();
        }
    
        public int BuildID { get; set; }
        public string BuildNo { get; set; }
        public string Language { get; set; }
        public System.DateTime DateCreated { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LogException> LogExceptions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UIContent> UIContents { get; set; }
    }
}

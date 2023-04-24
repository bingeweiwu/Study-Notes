using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SulpHurReport.Models
{
    public class QueryResult
    {
        [DataMemberAttribute()]
        public int ResultID { get; set; }
        [DataMemberAttribute()]
        public string BuildNo { get; set; }
        [DataMemberAttribute()]
        public string Language { get; set; }
        [DataMemberAttribute()]
        public string RuleName { get; set; }
        [DataMemberAttribute()]
        public string ResultType { get; set; }
        [DataMemberAttribute()]
        public string UIName { get; set; }
        [DataMemberAttribute()]
        public string UserName { get; set; }
        [DataMemberAttribute()]
        public string OSType { get; set; }
        
        [DataMemberAttribute()]
        public string DateUploadedStr { get; set; }
        
        [DataMemberAttribute()]
        public string DateCheckedStr { get; set; }
        [DataMemberAttribute()]
        public bool ReviewFlag { get; set; }
        
    }
}
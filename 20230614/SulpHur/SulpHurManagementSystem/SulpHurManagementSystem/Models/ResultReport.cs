using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SulpHurManagementSystem.Models
{
    public class ResultReport
    {
        [DataMemberAttribute()]
        public int ResultID { get; set; }
        [DataMemberAttribute()]
        public int RowID { get; set; }
        [DataMemberAttribute()]
        public string BuildNo { get; set; }
        [DataMemberAttribute()]
        public string RuleName { get; set; }
        [DataMemberAttribute()]
        public string PageTitle { get; set; }
        [DataMemberAttribute()]
        public string Result { get; set; }
        [DataMemberAttribute()]
        public string UserName { get; set; }
        [DataMemberAttribute()]
        public string BuildLanguage { get; set; }
        [DataMemberAttribute()]
        public string OSType { get; set; }
        [DataMemberAttribute()]
        public string LinkedBugIDs { get; set; }
        [DataMemberAttribute()]
        public DateTime DateUploaded { get; set; }
        [DataMemberAttribute()]
        public DateTime DateChecked { get; set; }
    }
}
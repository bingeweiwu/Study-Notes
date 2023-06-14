using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SulpHurManagementSystem.Models
{
    public class ResultReport
    {
        public int ResultID { get; set; }
        public int RowID { get; set; }
        public string BuildNo { get; set; }
        public string RuleName { get; set; }
        public string PageTitle { get; set; }
        public string Result { get; set; }
        public string UserName { get; set; }
        public string BuildLanguage { get; set; }
        public string OSType { get; set; }
        public string LinkedBugIDs { get; set; }
        public DateTime DateUploaded { get; set; }
        public DateTime DateChecked { get; set; }
    }
}
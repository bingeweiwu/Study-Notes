using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SulpHurManagementSystem.Models
{
    public class LatestResultStatus
    {
        public string RuleName { get; set; }
        public string Language { get; set; }
        public string LatestBuildNo { get; set; }
        public bool Reviewed  { get; set; }
        public string ResultType { get; set; }
        public int ResultID { get; set; }
    }
}
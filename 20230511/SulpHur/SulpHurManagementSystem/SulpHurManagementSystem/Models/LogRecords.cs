using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SulpHurManagementSystem.Models
{
    public class LogRecords
    {
        public int LogID { get; set; }
        public string BuildNo { get; set; }
        public string UserName { get; set; }
        public string OSType { get; set; }
        public string ExceptionContent { get; set; }
        public string FTime { get; set; }
        public string LTime { get; set; }
        public int Count { get; set; }
    }
}
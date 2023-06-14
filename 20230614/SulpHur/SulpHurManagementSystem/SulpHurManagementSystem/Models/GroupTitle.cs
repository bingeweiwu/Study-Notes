using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SulpHurManagementSystem.Models
{
    [Serializable()]
    [DataContractAttribute(IsReference = true)]
    public class GroupTitle
    {
        [DataMemberAttribute()]
        public string UIName { get; set; }
        [DataMemberAttribute()]
        public string Count { get; set; }
        [DataMemberAttribute()]
        public string ContentID { get; set; }
    }
}
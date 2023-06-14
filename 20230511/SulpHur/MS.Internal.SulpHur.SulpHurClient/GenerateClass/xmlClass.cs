using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenameLog
{
    public class xmlClass
    {
        private string Id;
        private string parentId;

        public string ID {
            get { return this.Id; }
            set { }
        }

        public string ParentID
        {
            get { return this.parentId; }
            set { }
        }

        public xmlClass(string id,string pid)
        {
            Id = id;
            parentId = pid;

        }
    }
}

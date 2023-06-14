using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SulpHurManagementSystem.Common
{
    public class SulpHurErrorException: Exception
    {
        public SulpHurErrorException()
            : base()
        { }
        public SulpHurErrorException(string message)
            : base(message)
        { }
        public SulpHurErrorException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
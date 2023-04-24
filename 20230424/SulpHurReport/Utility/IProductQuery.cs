using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SulpHurReport.Utility
{
    internal interface IProductQuery
    {
        string QueryAvailableOSTypes();
        string QueryAvailableOSLanguage();
        string QueryAvailableTypes();
        string QueryAvailableCapturedLanguages();
        string QueryAvailableCapturedBuilds();
        string QueryAvailableAssembly();
        Dictionary<string, string> QueryAssemblyTypesInfo();
        Dictionary<string, Rule> QueryAvailableRules();
    }
}

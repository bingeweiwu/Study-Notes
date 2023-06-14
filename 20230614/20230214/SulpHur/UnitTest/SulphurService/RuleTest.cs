using Microsoft.VisualStudio.TestTools.UnitTesting;
using MS.Internal.SulpHur.CMRules;
using MS.Internal.SulpHur.SulpHurService;
using MS.Internal.SulpHur.SulpHurService.DataAccess;
using MS.Internal.SulpHur.UICompliance;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.SulphurService
{
    [TestClass]
    public class RuleTest
    {
        [TestMethod]
        public void CMAccessKeyDuplicateRule()
        {
            string rule = "Access Key non Duplicate Rule";

            LinqDataClassesDataContext dataContext = new LinqDataClassesDataContext();
            var varList = (from r in dataContext.UIContents
                           where r.ContentID==441345
                           select new UIANDRule(r.UIContent1, rule,
                               r.IsWebUI, MS.Internal.SulpHur.UICompliance.Utility.BytesToBmp_MemStream(r.UIScreenShot), r.ContentID)).FirstOrDefault();
            CMAccessKeyDuplicateRule cMAccessKeyDuplicateRule = new CMAccessKeyDuplicateRule();

            ElementInformation eiRoot = MS.Internal.SulpHur.Utilities.ExtensionMethods.FromXElement<ElementInformation>(varList.Element);
            UIContentVerification verification = new UIContentVerification();
            List<ElementInformation> eiList = verification.ParseTreeToList(eiRoot);

            cMAccessKeyDuplicateRule.UIVerify(eiList);
            
        }
    }
}

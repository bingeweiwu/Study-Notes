using MogenDebug;
using MS.Internal.SulpHur.CMRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMogenDebug
{
    public class TempDB
    {
        SulpHurEntitiesForCMRules entityes ;
        public TempDB()
        {
            entityes = new SulpHurEntitiesForCMRules();
        }

        public List<string> GetVerticalSortPageNameList()
        {
            //SulpHurEntitiesForCMRules entityes = new SulpHurEntitiesForCMRules();
            var results = from n in entityes.VerticalSortPageNameLists
                          select n.name;
            return results.ToList();
        }


        public List<string> GetFilterSpellCheck()
        {
            var results = from n in entityes.FilterSpellChecks
                          select n.regexString;
            return results.ToList();
        }
        public List<string> GetNonEnglishButtonName()
        {
            var results = from n in entityes.FilterSpellChecks
                          select n.nonEnglishButtonName;
            return results.ToList();
        }
        public List<string> GetProperNounList()
        {
            var results = from n in entityes.FilterSpellChecks
                          select n.properNoun;
            return results.ToList();
        }
        public List<string> GetPageExceptionTypeList()
        {
            var results = from n in entityes.FilterSpellChecks
                          select n.pageExceptionType;
            return results.ToList();
        }
        public List<string> GetExceptionPageTitleList()
        {
            var results = from n in entityes.FilterSpellChecks
                          select n.exceptionPageTitle;
            return results.ToList();
        }
        public List<string> GetExcludeTextDetectPageTitleList()
        {
            var results = from n in entityes.FilterSpellChecks
                          select n.excludeTextDetectPageTitle;
            return results.ToList();
        }
        public List<string> GetNormalPunctuation()
        {
            var results = from n in entityes.FilterSpellChecks
                          select n.normalPunctuation;
            return results.ToList();
        }
    }
}

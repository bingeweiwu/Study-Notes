using Microsoft.Vbe.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MS.Internal.SulpHur.CMRules
{
    public class CMDBoperator
    {

        SulpHurEntitiesForCMRules entityes;

        public CMDBoperator()
        {
            entityes = new SulpHurEntitiesForCMRules();
        }

        public List<string> GetVerticalSortPageNameList()
        {
            //   SulpHurEntitiesForCMRules entityes = new SulpHurEntitiesForCMRules();
            var results = from n in entityes.VerticalSortPageNameLists
                          where n.name != null
                          select n.name;
            return results.ToList();
        }


        public List<string> GetFilterSpellCheck()
        {
            var results = from n in entityes.FilterSpellCheck_regexString
                          where n.regexString != null
                          select n.regexString;
            return results.ToList();
        }
        public List<string> GetNonEnglishButtonName()
        {
            var results = from n in entityes.FilterSpellCheck_nonEnglishButtonName
                          where n.nonEnglishButtonName != null
                          select n.nonEnglishButtonName;
            return results.ToList();
        }
        public List<string> GetProperNounList()
        {
            var results = from n in entityes.FilterSpellCheck_properNoun
                          where n.properNoun != null
                          select n.properNoun;
            return results.ToList();
        }
        public List<string> GetPageExceptionTypeList()
        {
            var results = from n in entityes.FilterSpellCheck_pageExceptionType
                          where n.pageExceptionType != null
                          select n.pageExceptionType;
            return results.ToList();
        }
        public List<string> GetExceptionPageTitleList()
        {
            var results = from n in entityes.FilterSpellCheck_exceptionPageTitle
                          where n.exceptionPageTitle != null
                          select n.exceptionPageTitle;
            return results.ToList();
        }
        public List<string> GetExcludeTextDetectPageTitleList()
        {
            var results = from n in entityes.FilterSpellCheck_excludeTextDetectPageTitle
                          where n.excludeTextDetectPageTitle != null
                          select n.excludeTextDetectPageTitle;
            return results.ToList();
        }
        public List<string> GetNormalPunctuation()
        {
            var results = from n in entityes.FilterSpellCheck_normalPunctuation
                          where n.normalPunctuation != null
                          select n.normalPunctuation;
            return results.ToList();
        }
        public List<int> GetAbnormalButNoImpact()
        {
            var results = from n in entityes.FilterSpellCheck_abnormalButNoImpact                          
                          select n.abnormalButNoImpact;
            return results.ToList();
        }
        public List<string> GetNonEnglishWords()
        {
            var results = from n in entityes.NonEnglishWordsButEnglishLetters
                          select n.NonEnglishWord;
            return results.ToList();
        }

        public List<string> GetWordsByLanguage(string lan)
        {
            var filter = GetBoolExpression<WordsSharedWithEnglish, bool>("x", "Equals", lan, true);                     
            var results = entityes.WordsSharedWithEnglishes.Where(filter).Select(W => W.Word);
            return results.ToList();
        }

        public bool WhetherSameAsEnglish(string word)
        {            
            bool result = entityes.WordsSharedWithEnglishes.Where(w => w.Word.Equals(word)).Select(w => w.ENU).Single();            
            return result;
        }

        public static Expression<Func<T, bool>> GetBoolExpression<T, V>(string lambdaVariableName, string operatorName,
             string propertyName, V propertyValue)
        {
            var parameterExp = Expression.Parameter(typeof(T), lambdaVariableName);
            var propertyExp = Expression.Property(parameterExp, propertyName);

            var method = typeof(V).GetMethod(operatorName, new[] { typeof(V) });
            var someValue = Expression.Constant(propertyValue, typeof(V));

            var methodExp = Expression.Call(propertyExp, method ?? throw new InvalidOperationException(), someValue);
            return Expression.Lambda<Func<T, bool>>(methodExp, parameterExp);
        }

    }
}

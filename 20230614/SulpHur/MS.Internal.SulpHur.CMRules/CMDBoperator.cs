using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MS.Internal.SulpHur.CMRules
{
    public class CMDBoperator
    {

        SulpHurEntitiesForCMRules entityes;

        public CMDBoperator()
        {
            entityes = new SulpHurEntitiesForCMRules();
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

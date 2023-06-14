using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class UATextRule : UIComplianceRuleBase
    {
        public override string Name
        {
            get { return @"UA Text Rule"; }
        }

        public override string Description
        {
            get { return @"This rule is used to check text for F&F."; }
        }

        public UATextRule()
        {
            ruleProperties = new UATextRuleProperties();

            ruleProperties.PunctuationKeys.Add(':');
            //ruleProperties.PunctuationKeys.Add('.');
            ruleProperties.PunctuationKeys.Add(';');
            ruleProperties.PunctuationKeys.Add(',');
            ruleProperties.PunctuationKeys.Add('"');

            ruleProperties.ForbiddenWords.Add("sccm");
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();

            foreach (ElementInformation c1 in controls)
            {
                string text = c1.Name;
                if (string.IsNullOrEmpty(text))
                    continue;
                #region check Forbidden words
                string errorInfo = ContainsInvalidProductName(c1.Name);
                if (!string.IsNullOrEmpty(errorInfo))
                {
                    UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, errorInfo, Name);
                    result.AddRelatedControls(c1);
                    results.Add(result);
                }
                #endregion

                #region Check no space or multiple spaces after a punctuation
                if (ruleProperties.SpaceAfterPunctuation)
                    results.AddRange(CheckSpaceAfterPunc(c1, c1.Name));
                #endregion

                #region Check 2 spaces in a sentence
                if (ruleProperties.MultiSpaceBetweenWords)
                    results.AddRange(CheckMultiSpaceConstraints(c1, c1.Name));
                #endregion
            }

            return results;
        }


        string ContainsInvalidProductName(string text)
        {
            foreach (string InvalidName in ruleProperties.ForbiddenWords)
            {
                if (text.ToLower().Contains(InvalidName.ToLower()))
                {
                    string errorInfo = string.Format("\"{0}\" contains \"{1}\" which we should avoid to use.", text);

                    return errorInfo;
                }
            }
            return null; ;
        }

        //Check no space or multiple spaces after a punctuation
        //invalid:   abc:a     abc:  a
        List<UIComplianceResultBase> CheckSpaceAfterPunc(ElementInformation control, string text)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            for (int i = 0; i < text.Length; i++)
            {
                char current = text[i];
                if (ruleProperties.PunctuationKeys.Contains(current))
                {
                    if (i == text.Length - 1)
                        return results;

                    char next = text[i + 1];
                    if (next != ' ' && next != '\r' && next != '\r' && !ruleProperties.PunctuationKeys.Contains(next))////"ab:ac."
                    {
                        string errorInfo = string.Format("No space after a punctuation.[@index = {0}  \"...{1}...\"]", i, ab(text, i));
                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, errorInfo, Name);
                        result.AddRelatedControls(control);
                        results.Add(result);
                        return results;
                    }
                    else if (next == ' ')
                    {
                        if (i == text.Length - 2)//"abac. "
                        {
                            string errorInfo = string.Format("An unnecessary space in the end of sentence");
                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, errorInfo, Name);
                            result.AddRelatedControls(control);
                            results.Add(result);
                            return results;
                        }
                        //else
                        //{
                        //    char next1 = text[i + 2];
                        //    if (next1 == ' ')////"abac.  "
                        //    {
                        //        string errorInfo = string.Format(string.Format("Multiple spaces after punctuation: [{0}]", current));
                        //        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, errorInfo, Name, true);
                        //        result.AddRelatedControls(control);
                        //        results.Add(result);
                        //    }
                        //}
                    }

                }
            }
            return results;
        }

        List<UIComplianceResultBase> CheckMultiSpaceConstraints(ElementInformation control, string text)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            int index = text.IndexOf("  ");
            if (index >= 0)
            {
                string errorInfo = string.Format("2 or more than 2 spaces are in the sentence[ [@index = {0} \"...{1}...\"]", index, ab(text, index));
                UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, errorInfo, Name);
                result.AddRelatedControls(control);
                results.Add(result);
                return results;
            }
            return results;
        }

        string ab(string str, int index)
        {
            int start = -1;
            if (index <= 6)
            {
                start = 0;
            }
            else
            {
                start = index - 6;
            }
            int length = (index + 14) <= str.Length ? 14 : (str.Length - index);
            return str.Substring(start, length);
        }

        private UATextRuleProperties ruleProperties;



        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }

    public class UATextRuleProperties
    {
        public List<string> ForbiddenWords
        {
            get
            {
                return forbiddenWords;
            }
            set { forbiddenWords = value; }
        }
        private List<string> forbiddenWords = new List<string>();

        public List<char> PunctuationKeys
        {
            get
            {
                if (punctuationKeys.Count == 0)
                {
                    //punctuationKeys.Add('"');
                    //punctuationKeys.Add('.');
                    //punctuationKeys.Add(';');
                    //punctuationKeys.Add(',');
                    //punctuationKeys.Add(':');
                }
                return punctuationKeys;
            }
            set { punctuationKeys = value; }
        }
        private List<char> punctuationKeys = new List<char>();


        public bool SpaceAfterPunctuation
        {
            get { return spaceAfterPunctuation; }
            set { spaceAfterPunctuation = value; }
        }
        private bool spaceAfterPunctuation = false;

        public bool MultiSpaceBetweenWords
        {
            get { return multiSpaceBetweenWords; }
            set { multiSpaceBetweenWords = value; }
        }
        private bool multiSpaceBetweenWords = false;

    }
}

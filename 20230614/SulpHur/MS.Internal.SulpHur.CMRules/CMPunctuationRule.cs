using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class PunctuationRule : UIComplianceRuleBase
    {
        public override string Name
        {
            get { return @"Punctuation Rule"; }
        }

        public override string Description
        {
            get { return @"This rule is used to check text miss punctuation."; }
        }

        public PunctuationRule()
        {
            ruleProperties = new PunctuationRuleProperties();
            ruleProperties.PunctuationKeys.Add(".");
            ruleProperties.PunctuationKeys.Add(":");
            ruleProperties.PunctuationKeys.Add("?");
            ruleProperties.PunctuationKeys.Add(">>");
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();

            foreach (ElementInformation c1 in controls)
            {
                if (IsTextControl(c1))
                {
                    string text = c1.Name;
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }
                    if (IsMissingPunctuation(text) && !ContainsEmpty(text))
                    {
                        string errorInfo = string.Format("\"{0}\" doesn't end with \".\", \"?\", or \":\"", text);
                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, errorInfo, Name);
                        result.AddRelatedControls(c1);
                        results.Add(result);
                    }
                }

                if (c1.ControlType == ControlType.CheckBox || (c1.ControlType == ControlType.RadioButton))
                {
                    if (c1.Name!=null&&!IsMissingPunctuation(c1.Name))
                    {
                        string errorInfo = string.Format("\"{0}\" should not end with \".\", \"?\", or \":\"", c1.Name);
                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, errorInfo, Name);
                        result.AddRelatedControls(c1);
                        results.Add(result);
                    }

                }

                if (c1.ControlType == ControlType.Button)
                {
                    if (c1.Name!=null&&!IsMissingPunctuation(c1.Name) && !c1.Name.Trim().EndsWith("..."))
                    {
                        string errorInfo = string.Format("\"{0}\" should not end with \".\", \"?\", or \":\"", c1.Name);
                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, errorInfo, Name);
                        result.AddRelatedControls(c1);
                        results.Add(result);
                    }
                }
            }

            if (results.Count == 0) {
                UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Pass, "No punctuation issue found in this UI", Name);
                results.Add(result);
            }

            return results;
        }

        bool IsTextControl(ElementInformation control)
        {
            #region false alarm
            if (control.ControlType == ControlType.Text && control.AutomationId == "labelCaption") return false;
            #endregion
            if (!string.IsNullOrEmpty(control.Name))
            {
                if (control.ControlType == ControlType.Text && control.Name.Trim().StartsWith("Example:") == true) return false;
            }
            //Main instruction don't need punctuationSupplemental 
            if (control.ControlType == ControlType.Text && control.Ancestors.Any(c => c.AutomationId == "_navPanel")) return false;
            return (control.ControlType == ControlType.Text && control.AutomationId != "_headerBar"
                && control.AutomationId != "_headlineLabel" && control.AutomationId != "labelHeader" && control.AutomationId != "labelMainInstruction");
        }

        bool IsMissingPunctuation(string text)
        {
            text = text.Trim();
            foreach (string p in ruleProperties.PunctuationKeys)
            {
                if (text.EndsWith(p))
                    return false;
            }
            return true;
        }

        bool ContainsEmpty(string text)
        {
            text = text.Trim();
            return text.ToLower().Contains("empty");
        }
        private PunctuationRuleProperties ruleProperties;

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }

    public class PunctuationRuleProperties
    {
        public List<string> PunctuationKeys
        {
            get { return punctuationKeys; }
            set { punctuationKeys = value; }
        }
        private List<string> punctuationKeys = new List<string>();

        public List<char> PunctuationKeys1
        {
            get { return punctuationKeys1; }
            set { punctuationKeys1 = value; }
        }
        private List<char> punctuationKeys1 = new List<char>();
    }
}

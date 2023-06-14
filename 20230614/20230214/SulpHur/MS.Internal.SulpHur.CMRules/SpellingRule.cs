using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MS.Internal.SulpHur.CMRules.DataAccess;
using System.Text.RegularExpressions;
using MS.Internal.SulpHur.UICompliance;
using System.Diagnostics;

namespace MS.Internal.SulpHur.CMRules
{
    public class SpellingRule : UIComplianceRuleBase
    {
        //private static Dictionary<string, List<string>> spellFilters = new Dictionary<string, List<string>>();
        //private static Dictionary<string, List<string>> spellIgnores = new Dictionary<string, List<string>>();
        public static List<string> ignoreSpellCheckList = new List<string>();
        public static List<string> nonEnglishButtonNameList = new List<string>();
        public static List<string> properNounList = new List<string>();
        public static List<string> pageExceptionTypeList = new List<string>();
        public static List<string> exceptionPageTitleList = new List<string>();
        public static List<string> excludeTextDetectPageTitleList = new List<string>();
        public static List<string> punctuationList = new List<string>();
        public static List<int> abnormalButNoImpact = new List<int>();


        static SpellingRule()
        {
            CMDBoperator CMDB = new CMDBoperator();
            ignoreSpellCheckList = CMDB.GetFilterSpellCheck();
            nonEnglishButtonNameList = CMDB.GetNonEnglishButtonName();
            properNounList = CMDB.GetProperNounList();
            pageExceptionTypeList = CMDB.GetPageExceptionTypeList();
            exceptionPageTitleList = CMDB.GetExceptionPageTitleList();
            excludeTextDetectPageTitleList = CMDB.GetExcludeTextDetectPageTitleList();
            punctuationList = CMDB.GetNormalPunctuation();
            abnormalButNoImpact = CMDB.GetAbnormalButNoImpact();

            //LinqDataClassesDataContext dataContext = new LinqDataClassesDataContext();
            //List<SpellFilter> filters = dataContext.SpellFilters.ToList();

            //foreach (SpellFilter filter in filters)
            //{
            //    if (spellFilters.Keys.Contains(filter.SpellFilterWord))
            //    {
            //        if (!spellFilters[filter.SpellFilterWord].Contains(filter.SpellFilterWordInString))
            //            spellFilters[filter.SpellFilterWord].Add(filter.SpellFilterWordInString);
            //    }
            //    else
            //    {
            //        spellFilters.Add(filter.SpellFilterWord, new List<string>() { filter.SpellFilterWordInString });
            //    }
            //}
            //PropertyInfo[] properties = typeof(SpellIgnore).GetProperties();
            //object propertyValue = null;
            //string propertyName = string.Empty;
            //string temp = string.Empty;
            //List<SpellIgnore> ignores = dataContext.SpellIgnores.ToList();

            //foreach (SpellIgnore ignore in ignores)
            //{
            //    foreach (PropertyInfo propertyInfo in properties)
            //    {
            //        propertyName = propertyInfo.Name;
            //        if (propertyName.Contains("Str"))
            //        {
            //            propertyValue = propertyInfo.GetValue(ignore, null);
            //            if (propertyValue != null)
            //            {
            //                temp = propertyValue.ToString();
            //                if (spellIgnores.Keys.Contains(propertyName))
            //                {
            //                    spellIgnores[propertyName].Add(temp);
            //                }
            //                else
            //                {
            //                    spellIgnores.Add(propertyName, new List<string>() { temp });
            //                }
            //            }
            //        }
            //    }
            //}
            //dataContext.Dispose();
        }

        public override string Name
        {
            get { return @"Spelling Rule"; }
        }

        public override string Description
        {
            get { return @"This rule checks the UI with the spell checker provided by Microsoft Word."; }
        }

        public SpellingRule()
        {
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            List<UIComplianceResultBase> result = new List<UIComplianceResultBase>();
            string realName = string.Empty;
            string formatName = string.Empty;
            char nonEnglishCharacter = '\0';
            bool ignoreSpellCheck = false;
            bool isRootNameContainColon = false;
            bool existInExceptionPageTitleList = false;
            bool isContainNonEnglish = false;
            bool excludeText = false;
            bool isButtonType = false;
            object template = Missing.Value;
            object newTemplate = Missing.Value;
            object documentType = Missing.Value;
            object visible = true;
            object optional = System.Reflection.Missing.Value;
            object saveOption = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
            ElementInformation root = CommonRuleUtility.GetRootElement(Controls);
            ElementInformation nonButtonTypeControl = null;
            isRootNameContainColon = root.Name.Contains(":");
            excludeText = root.Name == "Default Settings";

            //filter checks for Text-type controls
            excludeText = excludeTextDetectPageTitleList.Contains(root.Name);
            existInExceptionPageTitleList = exceptionPageTitleList.Contains(root.Name);

            UIComplianceResultBase newResult = null;
            string failMessage = "The following words in the highlighted controls did not pass the spelling check: {0}";
            string controlTypeMessage = "ControlType: {0}";
            string completeString = "completeString: ";
            string warnMessage = "The following character in the highlighted control is not English: {0}";

            Trace.WriteLine("start enter foreach loop that check all language page, try to find product name issue");
            foreach (ElementInformation ei in Controls)
            {
                try
                {
                    if (ei.Name != null)
                        realName = GetRealName(ei);
                    if (realName != null && !string.IsNullOrEmpty(realName) && realName.Length >= 40)
                    {
                        Trace.WriteLine("realName != null && !realName.IsNUllOrEmpty && realName.Length >=40 ");
                        if (realName.Contains("Microsoft Endpoint Configuration Manager"))
                        {
                            controlTypeMessage = string.Format(controlTypeMessage, ei.ControlType);
                            newResult = new UIComplianceResultBase(ResultType.Fail, "this control contain old name: " + "\r\n\r\n" + controlTypeMessage + "\r\n\r\n" + completeString + realName, this.Name);
                            newResult.AddRelatedControls(ei);
                            result.Add(newResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex + " at SulpHur\\MS.Internal.SulpHur.CMRules\\SpellingRule.cs line 151");
                    throw ex;
                }
            }
            Trace.WriteLine("end detect all language page in spelling rule for product name issue");


            if (lan == "ENU")
            {
                Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
                try
                {
                    foreach (ElementInformation ei in Controls)
                    { 
                        if (ei.ControlType == ControlType.Button
                            || "_navPanel" == ei.AutomationId.Trim()
                            || ei.ControlType == ControlType.CheckBox
                            || ei.ControlType == ControlType.RadioButton
                            || (ei.ControlType == ControlType.Text && !excludeText)
                            || ei.ControlType == ControlType.Group                          
                            )
                        {
                            if (FilterSpellingCheck(string.Empty, ei.Name)) continue;
                            char[] eiCharArray = GetRealName(ei).Trim().ToCharArray();
                            //
                            if (nonEnglishCharacter == '\0' || ei.ControlType == ControlType.Button)
                            {
                                foreach (char c in eiCharArray)
                                {
                                    if (!((c >= ' ' && c <= '~')
                                        || c == '\n'
                                        || c == '–'
                                        || c == '​'
                                        || c == ' '
                                        || c == '•'
                                        || c == '●'
                                        || c == '\t'
                                        || abnormalButNoImpact.Contains((int)c)
                                        || punctuationList.Contains(c.ToString())
                                        ))
                                    {
                                        nonEnglishCharacter = c;
                                        isContainNonEnglish = true;

                                        if (ei.ControlType != ControlType.Button)
                                        {
                                            nonButtonTypeControl = ei;
                                        }
                                        else
                                        {
                                            isButtonType = true;
                                        }
                                        break;
                                    }
                                }
                            }
                            //please pay attention: "isButtonType" and "isContainNonEnglish are binding relationships
                            if (ei.ControlType == ControlType.Button && ((isButtonType && isContainNonEnglish) || nonEnglishButtonNameList.Contains(ei.Name)))
                            {
                                result.Add(new UIComplianceResultBase(ResultType.Warning, "Spelling rule Only works on ENU. Others will set to warning.\n", Name));
                                return result;
                            }
                        }
                    }

                    if (isContainNonEnglish && !(isRootNameContainColon && nonButtonTypeControl.Name == root.Name.Split(':')[1].Trim()))
                    {
                        newResult = new UIComplianceResultBase(ResultType.Warning, string.Format(warnMessage, nonEnglishCharacter), this.Name);
                        newResult.AddRelatedControls(nonButtonTypeControl);
                        result.Add(newResult);
                        return result;
                    }

                    foreach (ElementInformation ei in Controls)
                    {
                        if (
                            !string.IsNullOrEmpty(ei.Name) && (
                                    ei.ControlType == ControlType.Button ||
                                    (ei.ControlType == ControlType.Text && !excludeText) ||
                                    ei.ControlType == ControlType.Group ||
                                    ei.ControlType == ControlType.RadioButton ||
                                    ei.ControlType == ControlType.CheckBox)
                            )
                        {
                            //skip to check Yes/No for non-ENU languages  
                           // ignoreSpellCheck = IgnoreCheckForENU(ei);
                            if (existInExceptionPageTitleList)
                                ignoreSpellCheck = IgnoreCheckForExceptionPage(ei);
                            if (ignoreSpellCheck)
                                continue;

                            //Filter - Not selected*List item, Selected*List item;
                            realName = GetRealName(ei);
                            //format string to be detected. 
                            formatName = GetFormatName(realName);

                            app.Visible = false;
                            template = Missing.Value;
                            newTemplate = Missing.Value;
                            documentType = Missing.Value;
                            visible = true;
                            Microsoft.Office.Interop.Word._Document doc1 = app.Documents.Add(ref template, ref newTemplate, ref documentType, ref visible);
                            #region
                            //v-linchz:Spelling rule just need run on ENU, if you want to Spelling rule run on every language, please uncoment below code and download related language pack.
                            //if (lan == "CHS")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdSimplifiedChinese;
                            //}
                            //if (lan == "CHT")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdTraditionalChinese;
                            //}
                            //if (lan == "CSY")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdCzech;
                            //}
                            //if (lan == "DEU")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdGerman;
                            //}
                            //if (lan == "ESN")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdSpanish;
                            //}
                            //if (lan == "FRA")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdFrench;
                            //}
                            //if (lan == "HUN")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdHungarian;
                            //}
                            //if (lan == "ITA")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdItalian;
                            //}
                            //if (lan == "JPN")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdJapanese;
                            //}
                            //if (lan == "KOR")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdKorean;
                            //}
                            //if (lan == "NLD")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdDutch;
                            //}
                            //if (lan == "PLK")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdPolish;
                            //}
                            //if (lan == "PTB")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdPortugueseBrazil;
                            //}
                            //if (lan == "PTG")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdPortuguese;
                            //}
                            //if (lan == "RUS")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdRussian;
                            //}
                            //if (lan == "SVE")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdSwedish;
                            //}
                            //if (lan == "TRK")
                            //{
                            //    doc1.Content.LanguageID = WdLanguageID.wdTurkish;
                            //}]
                            #endregion
                            try
                            {
                                // Use "formatName" instead of "realName"  to filter custom nouns in hump nomenclature,
                                // and add a space if there is no space after punctuation in the sentence
                                doc1.Words.First.InsertBefore(formatName);
                                //verify the spelling and store the wrong words in spellErrorsColl list
                                Microsoft.Office.Interop.Word.ProofreadingErrors spellErrorsColl = doc1.SpellingErrors;

                                bool filterSpellingCheck = false;
                                foreach (Microsoft.Office.Interop.Word.Range range in spellErrorsColl)
                                {
                                    //Use "realName" instead of "formatName" to filter URLs ending in ".com"
                                    filterSpellingCheck = FilterSpellingCheck(range.Text, realName);
                                    if (!filterSpellingCheck)
                                    {
                                        //filter bug that example bug id is 42875124
                                        if ((isRootNameContainColon && ei.Name == root.Name.Split(':')[1].Trim())
                                            //filter proper Noun. "PNG", "msi"
                                            || properNounList.Contains(range.Text))
                                           //||FilterSpellingCheck(string.Empty, range.Text))
                                            continue;
                                        failMessage = string.Format(failMessage, range.Text);
                                        controlTypeMessage = string.Format(controlTypeMessage, ei.ControlType);
                                        //failMessage = string.Format(failMessage, range.Text);
                                        //newResult = new UIComplianceResultBase(ResultType.Fail, string.Format(failMessage, range.Text), this.Name);
                                        // newResult = new UIComplianceResultBase(ResultType.Fail, string.Format(failMessage+controlTypeMessage+completeString+realName, range.Text, ei.ControlType), this.Name);
                                        //Console.WriteLine(failMessage + controlTypeMessage + completeString + realName);
                                        newResult = new UIComplianceResultBase(ResultType.Fail, failMessage + "\r\n\r\n" + controlTypeMessage + "\r\n\r\n" + completeString + CommonRuleUtility.TruncateControlFullName(realName) , this.Name);
                                        newResult.AddRelatedControls(ei);
                                        result.Add(newResult);
                                    }
                                }
                            }
                            finally
                            {
                                optional = System.Reflection.Missing.Value;
                                saveOption = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                                doc1.Close(ref saveOption, ref optional, ref optional);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    result.Add(new UIComplianceResultBase(ResultType.Info, "Encounter an error in verification.", Name));

                }
                finally
                {
                    optional = System.Reflection.Missing.Value;
                    saveOption = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    app.Quit(ref saveOption, ref optional, ref optional);
                }

                if (result.Count == 0)
                {
                    result.Add(new UIComplianceResultBase(ResultType.Pass, "No spelling issue found.", Name));
                }
            }
            else
            {

                result.Add(new UIComplianceResultBase(ResultType.Warning, "Spelling rule Only works on ENU. Others will set to warning.\n", Name));
            }
            return result;
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
        private string InsertSpaceBeforeSecondSentence(string testString)
        {
            char c = '.';
            if (testString.Contains(c))
            {
                int index = testString.LastIndexOf(c);
                while (index > -1)
                {
                    if (index != testString.Length - 1 && testString[index + 1] >= 'a' && testString[index + 1] <= 'Z')
                        testString = testString.Insert(index + 1, " ");
                    index = testString.LastIndexOf(c, index - 1);
                }
            }
            return testString;
        }

        private string InsertSpace(string inString)
        {

            string outString = string.Empty;
            List<string> tempProperNounList = new List<string>();
            foreach (string s in properNounList)
            {
                if (inString.Contains(s))
                {
                    tempProperNounList.Add(s);
                }
            }
            //Sort temProperNounList by string length from large to small
            for (int i = 0; i < tempProperNounList.Count(); i++)
            {
                for (int k = tempProperNounList.Count() - i - 1; k > 0; k--)
                {
                    if (tempProperNounList[k].Length > tempProperNounList[k - 1].Length)
                    {
                        string temp = tempProperNounList[k];
                        tempProperNounList[k] = tempProperNounList[k - 1];
                        tempProperNounList[k - 1] = temp;
                    }
                }
            }

            for (int i = 0; i < tempProperNounList.Count(); i++)
            {
                if (inString.Contains(tempProperNounList[i]))
                {
                    inString = inString.Replace(tempProperNounList[i], tempProperNounList[i].ToLower());
                }
            }

            char[] a = inString.ToCharArray();
            int j = 0;
            for (int i = 0; i < a.Length; ++i)
            {
                if (i + 1 < a.Length)
                {
                    if ((a[i] == '.' && a[i + 1] != ' ')
                        || (a[i] == ',' && a[i + 1] != ' ')
                        || ((a[i] >= 'a' && a[i] <= 'z') && (a[i + 1] >= 'A' && a[i + 1] <= 'Z')))
                    {
                        outString = outString.Insert(j++, a[i].ToString());
                        outString = outString.Insert(j++, " ");
                    }
                    else
                    {
                        outString = outString.Insert(j++, a[i].ToString());
                    }
                }
                else
                {
                    outString = outString.Insert(j++, a[i].ToString());
                }
            }
            for (int i = 0; i < tempProperNounList.Count(); i++)
            {
                if (outString.Contains(tempProperNounList[i].ToLower()))
                {
                    outString = outString.Replace(tempProperNounList[i].ToLower(), tempProperNounList[i]);
                }
            }
            return outString;
        }

        private bool IsUrl(string str)
        {
            try
            {
                string Url = @"([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
                string Url2 = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
                return Regex.IsMatch(str, Url) || Regex.IsMatch(str, Url2);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //filter URL, for example: input " xxxx.xxxx.xxxx.com" output " %.%.%.com", please pay attention that ' ' at line head
        private string GetFormatURL(string S)
        {
            string s = S.ToLower();
            short periodCount = 0;
            string formatURL = string.Empty;
            //insert ".com" by reverse interpolation
            for (int i = 0; i < S.Length - S.LastIndexOf(".com") - 4; i++)
            {
                formatURL = formatURL.Insert(0, S[S.Length - 1 - i].ToString());
            }
            //count number of '.' in string like " *.*.*.com", periodCount =3
            for (int i = s.Length - 1; i > 0; i--)
            {
                if (s[i] != ' ')
                {
                    if (s[i] == '.') periodCount++;
                }
                else
                {
                    break;
                }
            }

            //Replace the part other than '.com' with ' %.%.%'
            for (int i = S.Length - 1; i >= 0; i--)
            {
                if (periodCount > 0)
                {
                    do
                    {
                        formatURL = formatURL.Insert(0, "%");
                        while (i - 1 > 0 && S[i] != '.') i--;
                        formatURL = formatURL.Insert(0, S[i--].ToString());
                        periodCount--;
                    } while (periodCount > 0);

                    while (s[i] != ' ') i--;
                    formatURL = formatURL.Insert(0, "%");
                    formatURL = formatURL.Insert(0, S[i].ToString());
                }
                else
                {
                    formatURL = formatURL.Insert(0, S[i].ToString());
                }
            }
            //return ' %.%.%.com'
            return formatURL;
        }
        //filter network path, for example: input "\\XXXX\XXX\XXXX.xxxx", return "\\%\%\%.%"
        private string GetFormatNetPath(string S)
        {
            string formatNetPath = string.Empty;
            formatNetPath += "\\\\";
            for (int i = 2; i < S.Length; i++)
            {
                if (S[i] == '\\' || S[i] == '.')
                {
                    formatNetPath += "%";
                    formatNetPath += S[i];
                }
            }
            formatNetPath += "%";
            return formatNetPath;
        }


        public string GetFormatName(string realName)
        {
            string formatName = string.Empty;
            if (realName == string.Empty || realName.Equals("")) return formatName;
            // filter either XXXX *.*.*.com or XXXX( *.*.*.com)XXXX
            if (
                (realName.TrimStart().TrimEnd().Contains(" ") && realName.TrimStart().TrimEnd().ToLower().EndsWith(".com"))
                || (realName.Contains('(') && realName.Contains(')') && realName.Split('(')[1].Split(')')[0].TrimEnd().EndsWith(".com") && realName.Split('(')[1].Split(')')[0].TrimEnd().Contains(" "))
                )

            {
                formatName = GetFormatURL(realName.TrimStart().TrimEnd());
            }

            // filter \\<server>\<folder path> and "\\" is not at the beginning of the line, and the character before "\\" is used as the delimiter
            else if (realName.Contains("\\\\") && realName.IndexOf("\\\\") > 0)
            {
                char seperator = realName[realName.IndexOf("\\\\") - 1];          // Take the character before "\\"
                string[] a = realName.Split(seperator);
                string temp = string.Empty;
                int index = 0, length = 0;

                // Special treatment for containing "\\" and the rest remains unchanged
                foreach (string s in a)
                {
                    if (s.Contains("\\\\") && !s.Contains(seperator.ToString()))
                    {
                        temp = GetFormatNetPath(s);
                        index = realName.IndexOf(s);
                        length = s.Length;
                    }
                }
                // copy and replace the replacement part from the original string
                for (int i = 0; i < realName.Length;)
                {
                    if (i == index)
                    {
                        formatName += temp;
                        i += length - 1;
                    }
                    else
                    {
                        formatName += realName[i];
                        i++;
                    }
                }
            }
            else
            {
                formatName = InsertSpace(realName);
            }

            return formatName;
        }

        private string GetRealName(ElementInformation ei)
        {
            if (ei.Name == string.Empty || ei.Name.Equals("")) return string.Empty;
            string validName = ei.Name.Replace("&", string.Empty);
            string realName = validName;
            // remove additional value
            // tree node : type - Text; name - Not selected*List item, Selected*List item;
            string pattern = "^((Not s)|S)elected.*List item$";
            Regex regex = new Regex(pattern);
            if (regex.IsMatch(validName, 0) && ei.ControlType == ControlType.Text)
            {
                realName = validName.Substring(validName.IndexOf("elected") + 7, validName.Length - 9 - validName.IndexOf("elected") - 7);
            }
            //filter that ".XXXXX.xxx.net", bug id is 43083056
            if (ei.ControlType == ControlType.Text && (ei.Name)[0] == '.' && ei.Name.Length >= 4 && (ei.Name.Substring(ei.Name.Length - 4)) == ".net")
            {
                realName = realName.Substring(1);
            }
            return realName;
        }

        //private bool IgnoreCheckForENU(ElementInformation ei)
        //{
            //string validName = ei.Name.Replace("&", string.Empty);
            ////Skip below spell check for non-ENU language
            ////Yes,No
            ////Cancel
            //if (ei.ControlType == ControlType.Button)
            //{
            //    foreach (List<string> list in spellIgnores.Values)
            //    {
            //        if (list.Contains(validName))
            //        {
            //            return true;
            //        }
            //    }
            //}
        //    return false;
        //}

        private bool IgnoreCheckForExceptionPage(ElementInformation ei)
        {
            foreach (string s in pageExceptionTypeList)
            {
                if (ei.Name.Contains(s)) return true;
            }
            return false;
        }
        private bool FilterSpellingCheck(string stringToCheck, string completeString)
        {
            int index = completeString.IndexOf(stringToCheck);

            // filter customer created string which is included by ' or "
            // filter: '*' or "*"
            // sulphur result - 6341641
            if (index > 0 && index + stringToCheck.Length < completeString.Length - 1
               && (completeString[index - 1] == '\'' && completeString[index + stringToCheck.Length] == '\''
                || completeString[index - 1] == '\"' && completeString[index + stringToCheck.Length] == '\"'))
            {
                return true;
            }
            //filter: ConfigMgr_ATE
            //sulphur result - 499115
            else if (completeString.StartsWith("ConfigMgr_") && stringToCheck.Length == 13)
            {
                return true;
            }

            //filter: Example: *.*.*.com
            else if (completeString.TrimStart().ToLower().StartsWith("example:") && completeString.TrimEnd().EndsWith(". com"))
            {
                return true;
            }
            else if (index >= 0)
            { 
                if (completeString.Contains('\n')) completeString = completeString.Replace('\n', ' ');
                foreach (string s in ignoreSpellCheckList)
                {
                    Regex r = new Regex('^' + s + '$');
                    if (r.IsMatch(completeString)) return true;
                }

            }
            //if (spellFilters.Keys.Contains(stringToCheck))
            //{
            //    foreach (string s in spellFilters[stringToCheck])
            //    {
            //        if (string.IsNullOrEmpty(s) || completeString.Contains(s))
            //        {
            //            return true;
            //        }
            //    }
            //}
            return false;

            #region
            /*
              
            // filter: Are you sure you want to delete '*'?
            // sulphur result - 5929952 and 5929905
            else if (completeString.StartsWith("Are you sure you want to delete '")
                && completeString.EndsWith("'?"))
            {
                return true;
            }
            // filter: Are you sure that you want to delete the * administrative user from Configuration Manager?
            // sulphur result - 5873005
            else if (completeString.StartsWith("Are you sure that you want to delete the ")
                && completeString.EndsWith(" administrative user from Configuration Manager?"))
            {
                return true;
            }
            // filter below customer create setting
            // Are you sure you want to delete setting '*'?
            // sulphur result - 5977286
            else if (completeString.StartsWith("Are you sure you want to delete setting '")
                && completeString.EndsWith("'?"))
            {
                return true;
            }
            // filter below customer create policy
            // filter: Are you sure you want to delete policy *?
            // sulphur result - 6724241
            else if (completeString.StartsWith("Are you sure you want to delete policy ")
                && completeString.EndsWith("?"))
            {
                return true;
            }
            // filter: Are you sure you want to remove Dependency * from the dependency chain?
            // sulphur result - 6345151 ---OK
            else if (completeString.StartsWith("Are you sure you want to remove Dependency ")
                && completeString.EndsWith(" from the dependency chain?"))
            {
                return true;
            }
            // filter: Do you want to remove * from the selected distribution point? This action will delete the content files when they are not associated with other packages.
            // sulphur result - 6297162
            else if (completeString.StartsWith("Do you want to remove ")
                && completeString.EndsWith(" from the selected distribution point? This action will delete the content files when they are not associated with other packages."))
            {
                return true;
            }
            // filter: This name is already in use as an inventoried name for the following display name: *. To use this inventoried name you must first delete it from the list of inventoried names where it is currently in use.
            // sulphur result - 5656340
            else if (completeString.StartsWith("This name is already in use as an inventoried name for the following display name: ")
                && completeString.EndsWith(". To use this inventoried name you must first delete it from the list of inventoried names where it is currently in use."))
            {
                return true;
            }
            */
            #endregion

        }
    }
}

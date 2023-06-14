using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;
using System.Windows.Forms;

namespace MS.Internal.SulpHur.CMRules
{
    public class Truncation30Rule : UIComplianceRuleBase
    {
        const string ruleName = "Text 30% Buffer Truncation Rule";
        const string ruleDescrition = "The rule is used to check there is 30% length buffer for text truncation in the controls";

        #region Control's Margin
        private static System.Windows.Forms.Padding managePushButtonDefaultMargin = new System.Windows.Forms.Padding(8, 5, 6, 0);
        private static System.Windows.Forms.Padding manageLabelDefaultMargin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        private static System.Windows.Forms.Padding manageRadioDefaultMargin = new System.Windows.Forms.Padding(17, 2, 8, 0);
        private static System.Windows.Forms.Padding manageCheckDefaultMargin = new System.Windows.Forms.Padding(17, 2, 8, 0);

        private static System.Windows.Forms.Padding ZeroMargin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        #endregion

        public override string Name
        {
            get
            {
                return ruleName;
            }
        }

        public override string Description
        {
            get
            {
                return ruleDescrition;
            }
        }

        string percentInfo = string.Empty;
        int planLine = 1;
        int resultLine = 1;

        float warningLevel = 0.3f;
        float errorLevel = 0.25f;

        /// <summary>
        /// Rule entry point
        /// </summary>
        /// <param name="Controls"></param>
        /// <returns></returns>
        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            try
            {

                // Get Parent Window
                IEnumerator<ElementInformation> enumerator = Controls.GetEnumerator();
                enumerator.MoveNext();
                ElementInformation Parent = enumerator.Current;

                // Check each controls of that window
                foreach (ElementInformation winInfo in Controls)
                {
                    if (winInfo.ControlType != ControlType.Text && winInfo.ControlType != ControlType.Button &&
                        winInfo.ControlType != ControlType.RadioButton && winInfo.ControlType != ControlType.CheckBox &&
                        winInfo.ControlType != ControlType.Group)
                        continue;

                    if (string.IsNullOrEmpty(winInfo.Name))
                        continue;

                    // Check each control and add the result to container
                    UIComplianceResultBase tempResult = null;
                    if (winInfo is ManagedElementInformation)
                    {
                        tempResult = CheckManagedControl(winInfo);
                    }
                    else
                    {
                        tempResult = CheckWin32Control(winInfo);
                    }

                    if (tempResult != null)
                    {
                        tempResult.AddRelatedControls(winInfo);
                        results.Add(tempResult);
                    }

                }

                // If no Error Result, Create a Pass Result for the window
                if (results.Count == 0)
                {
                    UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Pass, "No 30% truncation found in window : " + Parent.Name, ruleName);
                    results.Add(passResult);
                }

                return results;
            }
            catch (Exception ex)
            {
                //WriteLineToLog(ex.Message);
                //WriteLineToLog(ex.StackTrace);
                return results;
            }
        }

        public static void WriteToDailyCopyLog(string line)
        {
            try
            {
                string mCopyDLLog = "c:\\telescope\\Truncation30.txt";
                System.IO.FileInfo fi = new System.IO.FileInfo(mCopyDLLog);
                if (!fi.Directory.Exists)
                    fi.Directory.Create();
                System.IO.FileStream fs = new System.IO.FileStream(mCopyDLLog, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs);
                sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString(), line));
                sw.Close();
                fs.Close();
            }
            catch
            {
                //eat it
            }
        }
        /// <summary>
        /// Truncation Check for Managed controls
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public UIComplianceResultBase CheckManagedControl(ElementInformation winInfo)
        {
            ResultType result = ResultType.Pass;
            result = CommonTruncationCheck(winInfo);

            if (result == ResultType.Fail || result == ResultType.Warning)
            {
                if (!winInfo.Name.Contains("\n"))
                {
                    string message = "The " + winInfo.ControlType.ToString() + " control : '" + GetRidof(winInfo.Name) +
                        "' may be truncated! It doesn's has 30% buffer for the text, and its reserver buffer is"
                        + percentInfo + ".  The Propose LineNumber is " + planLine;
                    return (new UIComplianceResultBase(result, message, this.Name));
                }
                else
                {
                    StringBuilder r1 = new StringBuilder();
                    r1.AppendLine("The " + winInfo.ControlType.ToString() + " control may be truncated: '");
                    r1.AppendLine(winInfo.Name);
                    r1.AppendLine();
                    r1.AppendLine("It doesn's has 30% buffer for the text, and its reserver buffer is" + percentInfo);
                    r1.AppendLine("The Propose LineNumber is " + planLine);
                    r1.AppendLine("The Result LineNumber is " + resultLine);
                    r1.AppendLine();
                    r1.AppendLine("The fake text is:");
                    r1.AppendLine(Generate30String(winInfo.Name));

                    //string message = "The " + winInfo.ControlType.ToString() + " control : '" + winInfo.Name +
                    //    "' may be truncated! It doesn's has 30% buffer for the text, and its reserver buffer is"
                    //    + percentInfo + ".  The Propose LineNumber is " + planLine + ". \n The fake text is" + Generate30String(winInfo.Name);
                    return (new UIComplianceResultBase(result, r1.ToString(), this.Name));
                }
            }
            return null;
        }

        /// <summary>
        /// Truncation check for Win32 Controls
        /// </summary>
        /// <param name="winInfo"></param>
        /// <returns></returns>
        public UIComplianceResultBase CheckWin32Control(ElementInformation winInfo)
        {
            // Check Control truncation according to its type
            //SizeF controlSize = new SizeF(control.Extended.Width, control.Extended.Height);
            ResultType result = ResultType.Pass;
            switch (winInfo.ControlType)
            {
                // Managed Controls
                case ControlType.Button:
                    result = Win32LabelTruncationCheck(winInfo);
                    break;
                case ControlType.Text:
                case ControlType.RadioButton:
                case ControlType.CheckBox:
                    result = CommonTruncationCheck(winInfo);
                    break;
            }

            if (result == ResultType.Fail)
            {
                string message = "The " + winInfo.ControlType.ToString() + " control : '" + winInfo.Name + "' is 30% truncated!";
                return (new UIComplianceResultBase(result, message, this.Name));
            }
            return null;
        }


        private ResultType CommonTruncationCheck(ElementInformation winInfo)
        {
            if (winInfo.ProposeSize == null)
                return ResultType.Info;

            string text = GetRidof(winInfo.Name);
            SulpHurSize proposeSize = winInfo.ProposeSize;
            SulpHurSize unit = winInfo.TextUnit;
            int pLines = proposeSize.Height / unit.Height;
            planLine = pLines;
            if (pLines <= 1)
            {
                SulpHurSize resultSize = winInfo.TextSize1;
                int rLines = resultSize.Height / unit.Height;
                float per = 1 - (float)resultSize.Width / proposeSize.Width;
                if (per < errorLevel)
                //if (proposeSize.Width < resultSize.Width * 1.3)
                {
                    // xxx temp: Write to log file
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Control Type : {0} ; Control's text : {1} ; \r\n", winInfo.ControlType.ToString(), winInfo.Name);
                    sb.AppendFormat("Result Size - width * 1.3 : {0}; \r\n", resultSize.Width * 1.3);
                    //WriteLineToLog(sb.ToString());
                    percentInfo = per * 100 + "%";
                    planLine = 1;
                    return ResultType.Fail;
                }
                else if (per >= errorLevel && per < warningLevel)
                {
                    // xxx temp: Write to log file
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Control Type : {0} ; Control's text : {1} ; \r\n", winInfo.ControlType.ToString(), winInfo.Name);
                    sb.AppendFormat("Result Size - width * 1.3 : {0}; \r\n", resultSize.Width * 1.3);
                    //WriteLineToLog(sb.ToString());
                    percentInfo = per * 100 + "%";
                    planLine = 1;
                    return ResultType.Warning;
                }
            }
            else
            {
                SulpHurSize resultSize = winInfo.TextSize1;
                int rLines = resultSize.Height / unit.Height;
                int len1 = text.Length;
                string copyText = text;
                int i1 = 0;
                if (!text.Contains("\n"))
                {
                    while (true)
                    {
                        //text += (char)('A' + (i1%57));
                        text += copyText[i1++];
                        if (i1 >= copyText.Length)
                            return ResultType.Pass;
                        SulpHurSize resultSize2 = winInfo.TextSize2;
                        resultLine = resultSize2.Height / unit.Height;
                        float per2 = (float)(text.Length - len1) / text.Length;
                        if (resultLine > pLines || per2 > 0.27)
                            break;
                    }
                    float per = (float)(text.Length - len1) / text.Length;
                    if (per < errorLevel)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Control Type : {0} ; Control's text : {1} ; \r\n", winInfo.ControlType.ToString(), winInfo.Name);
                        sb.AppendFormat("Reserve percentage is : {0}%; \r\n", per * 100);
                        //WriteLineToLog(sb.ToString());
                        percentInfo = per * 100 + "%";
                        return ResultType.Fail;
                    }
                    else if (per >= errorLevel && per < warningLevel)
                    {
                        // xxx temp: Write to log file
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Control Type : {0} ; Control's text : {1} ; \r\n", winInfo.ControlType.ToString(), winInfo.Name);
                        sb.AppendFormat("Result Size - width * 1.3 : {0}; \r\n", resultSize.Width * 1.3);
                        //WriteLineToLog(sb.ToString());
                        percentInfo = per * 100 + "%";
                        return ResultType.Warning;
                    }
                }
                else
                {
                    string tString = Generate30String(text);
                    SulpHurSize resultSize2 = winInfo.TextSize2;
                    resultLine = resultSize2.Height / unit.Height;
                    if (resultLine > pLines)
                    {
                        StringBuilder sb = new StringBuilder();
                        return ResultType.Fail;
                    }

                }
            }


            return ResultType.Pass;
        }

        private ResultType Win32LabelTruncationCheck(ElementInformation winInfo)
        {
            return ResultType.Pass;
        }

        public string Generate30String(string text)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder line = new StringBuilder();

            foreach (char i1 in text)
            {
                if (i1 == '\r' || i1 == '\n')// "\r\n"
                {
                    string l1 = line.ToString();
                    int aStr = (int)(l1.Length * errorLevel);
                    string a1 = l1.Substring(0, aStr);
                    sb.Append(a1);
                    sb.Append(i1);
                    line.Remove(0, line.Length);
                }
                else
                {
                    sb.Append(i1);
                    line.Append(i1);
                }
            }
            return sb.ToString();
        }

        public string GetRidof(string text)
        {
            StringBuilder sb = new StringBuilder();
            bool has = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (has)
                {
                    sb.Append(text[i]);
                    continue;
                }

                if (text[i] != '&')
                    sb.Append(text[i]);
                else
                    has = true;
            }
            return sb.ToString();
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}

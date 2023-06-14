using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

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

        public Truncation30Rule()
        {
        }

        /// <summary>
        /// Rule entry point
        /// </summary>
        /// <param name="Controls"></param>
        /// <returns></returns>
        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            if (lan == "ENU")
            {
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

                        //ignore group used as split
                        if (winInfo.ControlType == ControlType.Group && winInfo.Height <= 10)
                        {
                            continue;
                        }

                        //ignore image button
                        if (winInfo.IsImageButton == true)
                        {
                            continue;
                        }

                        //ignore hidden control in client settings page
                        if (winInfo.Name == "Item_Antimalware_EnableHeuristics" && winInfo.AutomationId == "1974924")
                        {
                            continue;
                        }

                        // Check each control and add the result to container
                        UIComplianceResultBase tempResult = null;
                        if (winInfo.IsManagedControlProperty)
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
                    Trace.WriteLine(ex);
                    return results;
                }
            }
            else
            {
                results.Add(new UIComplianceResultBase(ResultType.Warning, "Text 30% Buffer Truncation Rule Only works on ENU. Others will set to warning.\n", ruleName));
            }
            return results;
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
               //v-danpgu: Filter False alarm of 30% truncation rule
        bool IsDegitOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }
        public UIComplianceResultBase CheckManagedControl(ElementInformation winInfo)
        {
            ResultType result = ResultType.Pass;
            result = CommonTruncationCheck(winInfo);

            if (result == ResultType.Fail || result == ResultType.Warning)
            {

                //v-danpgu: filter only number text
                if (winInfo.ControlType == ControlType.Text)
                {
                    if (IsDegitOnly(winInfo.Name))
                    {
                        string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] contains only numbers and might be false alarm, need more investigation."
                            , CommonRuleUtility.TruncateControlFullName(winInfo.Name), winInfo.ControlType, winInfo.BoundingRectangle);
                        return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                    }
                }
                //v-danpgu: filter group with checkbox
                if (winInfo.ControlType == ControlType.Group && ((winInfo.Name.Contains(".")) || (winInfo.Name.Contains("。"))) || winInfo.AutomationId == "groupBoxSupSchedule")
                {
                    string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] might have a checkbox and truncation might be false alarm, need more investigation."
                        , CommonRuleUtility.TruncateControlFullName(winInfo.Name), winInfo.ControlType, winInfo.BoundingRectangle);
                    return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                }
                //v-danpgu: group control with hidden control
                if ((winInfo.ControlType == ControlType.Text))
                {
                    if (IsDegitOnly(winInfo.AutomationId))
                    {
                        string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] might have a hidden control and truncation might be false alarm, need more investigation."
                            , CommonRuleUtility.TruncateControlFullName(winInfo.Name), winInfo.ControlType, winInfo.BoundingRectangle);
                        return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                    }
                }
                if (winInfo.ControlType == ControlType.Group && winInfo.AutomationId == "groupBoxSeperator")
                {
                    string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] might have a group box separator and truncation might be false alarm, need more investigation."
                        , CommonRuleUtility.TruncateControlFullName(winInfo.Name), winInfo.ControlType, winInfo.BoundingRectangle);
                    return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                }
                else
                {
                    if (!winInfo.Name.Contains("\n"))
                    {
                        string message = "The " + winInfo.ControlType.ToString() + " control : '" + CommonRuleUtility.TruncateControlFullName(GetRidof(winInfo.Name)) +
                            "' may be truncated! It doesn's has 30% buffer for the text, and its reserver buffer is "
                            + percentInfo + ".  If add 30% buffer the expected LineNumber should be " + planLine + ".";
                        return (new UIComplianceResultBase(result, message, this.Name));
                    }
                    else
                    {
                        StringBuilder r1 = new StringBuilder();
                        r1.AppendLine("The " + winInfo.ControlType.ToString() + " control may be truncated: '");
                        r1.AppendLine(CommonRuleUtility.TruncateControlFullName(winInfo.Name));
                        r1.AppendLine();
                        r1.AppendLine("It doesn's has 30% buffer for the text, and its reserver buffer is " + percentInfo + ". ");
                        r1.AppendLine(" If add 30% buffer the expected LineNumber should be " + planLine);
                        r1.AppendLine("The Result LineNumber is " + resultLine);
                        r1.AppendLine();
                        r1.AppendLine("The fake text is:");
                        r1.AppendLine(Generate30String(CommonRuleUtility.TruncateControlFullName(winInfo.Name)));

                        //string message = "The " + winInfo.ControlType.ToString() + " control : '" + winInfo.Name +
                        //    "' may be truncated! It doesn's has 30% buffer for the text, and its reserver buffer is"
                        //    + percentInfo + ".  The Propose LineNumber is " + planLine + ". \n The fake text is" + Generate30String(winInfo.Name);
                        return (new UIComplianceResultBase(result, r1.ToString(), this.Name));
                    }
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
            //v-danpgu: filter group with checkbox
            if (winInfo.ControlType == ControlType.Group && text.Contains("."))
            {
                return ResultType.Warning;
            }
            else
            {
                if (pLines <= 1)
                {
                    SulpHurSize resultSize = winInfo.TextSize1;
                    int rLines = resultSize.Height / unit.Height;

                    if (resultSize.Width == 0)
                    {
                        return ResultType.Pass;
                    }
                    float per = (float)proposeSize.Width / resultSize.Width - 1; //float per = 1 - (float)resultSize.Width / proposeSize.Width;  //
                    planLine = (int)(resultSize.Width * 1.3 / proposeSize.Width) + 1;
                    double per1 = Math.Round(per, 3);

                    if (per < errorLevel)
                    //if (proposeSize.Width < resultSize.Width * 1.3)
                    {
                        // xxx temp: Write to log file
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Control Type : {0} ; Control's text : {1} ; \r\n", winInfo.ControlType.ToString(), CommonRuleUtility.TruncateControlFullName(winInfo.Name));
                        sb.AppendFormat("Result Size - width * 1.3 : {0}; \r\n", resultSize.Width * 1.3);
                        //WriteLineToLog(sb.ToString());
                        percentInfo = per1 * 100 + "%";
                        pLines = 1;
                        return ResultType.Fail;
                    }
                    else if (per >= errorLevel && per < warningLevel)
                    {
                        // xxx temp: Write to log file
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Control Type : {0} ; Control's text : {1} ; \r\n", winInfo.ControlType.ToString(), CommonRuleUtility.TruncateControlFullName(winInfo.Name));
                        sb.AppendFormat("Result Size - width * 1.3 : {0}; \r\n", resultSize.Width * 1.3);
                        //WriteLineToLog(sb.ToString());
                        percentInfo = per1 * 100 + "%";
                        pLines = 1;
                        return ResultType.Warning;
                    }
                }
                else
                {
                    SulpHurSize resultSize = winInfo.TextSize1;
                    int rLines = resultSize.Height / unit.Height;
                    int len1 = text.Length;
                    //string copyText = text;
                    //int i1 = 0;
                    if (!text.Contains("\n"))
                    {
                        //while (true)
                        //{
                        //    //text += (char)('A' + (i1%57));
                        //    text += copyText[i1++];
                        //    if (i1 >= copyText.Length)
                        //        return ResultType.Pass;
                        //    SulpHurSize resultSize2 = winInfo.TextSize2;
                        //    resultLine = resultSize2.Height / unit.Height;
                        //    float per2 = (float)(text.Length - len1) / text.Length;
                        //    if (resultLine > pLines || per2 > 0.27)
                        //        break;
                        //}


                        //float textsize = resultSize.Height * resultSize.Width;
                        //float wincontrolsize = proposeSize.Height * proposeSize.Width;
                        //v-danpgu: use length to calcuate the buffer
                        SizeF l1 = TextRenderer.MeasureText(text, SystemFonts.DefaultFont);
                        planLine = (int)(l1.Width * 1.3) / proposeSize.Width + 1;
                        float textsize = l1.Width;
                        float wincontrolsize = pLines * proposeSize.Width;
                        if (resultSize.Width == 0)
                        {
                            return ResultType.Pass;
                        }
                        float per = (float)(wincontrolsize / textsize) - 1; //float per = 1 - (textsize / wincontrolsize); //(float)(text.Length - len1) / text.Length;
                        double per1 = Math.Round(per, 3);
                        if (per < errorLevel)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("Control Type : {0} ; Control's text : {1} ; \r\n", winInfo.ControlType.ToString(), winInfo.Name);
                            sb.AppendFormat("Reserve percentage is : {0}%; \r\n", per * 100);
                            //WriteLineToLog(sb.ToString());
                            percentInfo = per1 * 100 + "%";
                            return ResultType.Fail;
                        }
                        else if (per >= errorLevel && per < warningLevel)
                        {
                            // xxx temp: Write to log file
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("Control Type : {0} ; Control's text : {1} ; \r\n", winInfo.ControlType.ToString(), winInfo.Name);
                            sb.AppendFormat("Result Size - width * 1.3 : {0}; \r\n", resultSize.Width * 1.3);
                            //WriteLineToLog(sb.ToString());
                            percentInfo = per1 * 100 + "%";
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

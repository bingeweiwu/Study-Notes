using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;
using System.Windows.Forms;

namespace MS.Internal.SulpHur.CMRules
{
    public class TruncationRule : UIComplianceRuleBase
    {
        const string ruleName = "Text Truncation Rule";
        const string ruleDescrition = "The rule can verify whether there is any text truncation in the controls";

        #region Control's Margin
        private static System.Windows.Forms.Padding managePushButtonDefaultMargin = new System.Windows.Forms.Padding(8, 5, 6, 0);
        private static System.Windows.Forms.Padding manageLabelDefaultMargin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        private static System.Windows.Forms.Padding manageRadioDefaultMargin = new System.Windows.Forms.Padding(17, 2, 8, 0);
        private static System.Windows.Forms.Padding manageCheckDefaultMargin = new System.Windows.Forms.Padding(17, 2, 8, 0);
        private static System.Windows.Forms.Padding managedGroupBoxDefaultmargin = new System.Windows.Forms.Padding(10, 0, 25, 0);

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

        public TruncationRule()
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
            try
            {
                //List<ElementInformation> myWindows = new List<ElementInformation>(Windows);

                // Get Parent Window
                IEnumerator<ElementInformation> enumerator = Controls.GetEnumerator();
                enumerator.MoveNext();
                ElementInformation Parent = enumerator.Current;

                // Check each controls of that window
                foreach (ElementInformation winInfo in Controls)
                {
                    //ignore group used as split
                    if (winInfo.ControlType == ControlType.Group && winInfo.Height <= 10) {
                        continue;
                    }

                    //only check follow controls
                    List<ControlType> truncationCheckControlType = new List<ControlType>() { 
                        ControlType.Text,
                        ControlType.Button,
                        ControlType.RadioButton,
                        ControlType.CheckBox,
                        ControlType.Group
                    };
                    if (!truncationCheckControlType.Contains(winInfo.ControlType))
                        continue;

                    if (winInfo.Name == null || winInfo.Name.Length == 0||winInfo.IsImageButton)
                        continue;

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
                    //v-edy: bug485732:-result ID 5078785, Add button rule to truncation rule
                    if (ControlType.Button == winInfo.ControlType)
                    {
                        CheckButtonVerticalTruncation(winInfo, results);
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
                    UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Pass, "No truncation found in window : " + CommonRuleUtility.TruncateControlFullName(Parent.Name), ruleName);
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
                string mCopyDLLog = "c:\\telescope\\Truncation.txt";
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
        
        //v-danpgu: Filter the hidden control, controlID is only number
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
            // Check Control truncation according to its type
            //SizeF controlSize = new SizeF(control.Extended.Width, control.Extended.Height);
            bool result = false;

            result = CommonTruncationCheck(winInfo);

            if (result)
            {
                #region False alarm
                if (winInfo.ControlType == ControlType.Button && winInfo.Name.Contains("...")) {
                    return null;
                }
                #endregion
                //v-edy: bug484964:-result ID 5077451, string contains '\n' need double check
                if (winInfo.Name.Contains('\n'))
                {
                    string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] have multiple lines, please double check."
                        , winInfo.Name, winInfo.ControlType, winInfo.BoundingRectangle);
                    return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                }
                //v-edy: bug484964:-result ID 5143772/6268567, checkbox or radiobox not meet rule, need double check
                if (winInfo.ControlType == ControlType.CheckBox || winInfo.ControlType == ControlType.RadioButton)
                {
                    string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] is checkbox or radiobox, not meet truncation rule, please double check."
                        , winInfo.Name, winInfo.ControlType, winInfo.BoundingRectangle);
                    return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                }

                //v-danpgu: filter group control with check box 
                if (winInfo.ControlType == ControlType.Group && ((winInfo.Name.Contains(".")) || (winInfo.Name.Contains("。"))) || winInfo.AutomationId == "groupBoxSupSchedule" || (winInfo.AutomationId == "labelLine" && winInfo.Name == "检测信号发现" && winInfo.ControlType == ControlType.Text)
                   )
                {
                    string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] might have a checkbox and truncation might be false alarm, need more investigation."
                        , winInfo.Name, winInfo.ControlType, winInfo.BoundingRectangle);
                    return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                }
                //v-danpgu: filter group control with hidden control
                if ((winInfo.ControlType == ControlType.Text))
                {
                    if (IsDegitOnly(winInfo.AutomationId))
                    {
                        string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] might have a hidden control and truncation might be false alarm, need more investigation."
                            , winInfo.Name, winInfo.ControlType, winInfo.BoundingRectangle);
                        return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                    }
                }
                //v-danpgu: filter seperation line
                if ((winInfo.AutomationId == "labelLine" && winInfo.Name == "检测信号发现" && winInfo.ControlType == ControlType.Text) || winInfo.ControlType == ControlType.Group && winInfo.AutomationId == "groupBoxSeperator")
                {
                    string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] might be used as a separator and truncation might be false alarm, need more investigation."
                        , winInfo.Name, winInfo.ControlType, winInfo.BoundingRectangle);
                    return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                }
                else
                {
                    string message = string.Format("[Name:{0}, ControlType:{1}, Rectangle:{2}] is truncated."
                , winInfo.Name, winInfo.ControlType, winInfo.BoundingRectangle);
                    //string message = "The " + winInfo.ControlType.ToString() + " control : '" + winInfo.Name + "' is truncated!";
                    return (new UIComplianceResultBase(ResultType.Fail, message, this.Name));
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
            bool result = false;
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


            if (result)
            {
                //v-edy: bug484964:-result ID 5077451, string contains '\n' need double check
                if (winInfo.Name.Contains('\n'))
                {
                    string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] have multiple lines, please double check."
                        , winInfo.Name, winInfo.ControlType, winInfo.BoundingRectangle);
                    return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                }
                //v-edy: bug484964:-result ID 5143772/6268567, checkbox or radiobox not meet rule, need double check
                if (winInfo.ControlType == ControlType.CheckBox || winInfo.ControlType == ControlType.RadioButton)
                {
                    string messageOfGroup = string.Format("[Name:{0}, ControlType:{1}, Retangle:{2}] is checkbox or radiobox, not meet truncation rule, please double check."
                        , winInfo.Name, winInfo.ControlType, winInfo.BoundingRectangle);
                    return (new UIComplianceResultBase(ResultType.Warning, messageOfGroup, this.Name));
                }

                string message = string.Format("[Name:{0}, ControlType:{1}, Rectangle:{2}] is truncated."
                                , winInfo.Name, winInfo.ControlType, winInfo.BoundingRectangle);
                return (new UIComplianceResultBase(ResultType.Fail, message, this.Name));
            }
            return null;
        }

        private bool CommonTruncationCheck(ElementInformation winInfo)
        {
            string text = winInfo.Name;

            //v-edy: bug484964:-result ID 5077451, string contains '\n' need double check
            if (text.Contains('\n'))
                return true;

            if (winInfo.ProposeSize == null)
                return false;

            SulpHurSize proposeSize = winInfo.ProposeSize;

            #region Group Box Title
            //miyan: this is special treat for group box title. 
            // because the BoundingRectangle will get the rect of all group box
            // and the text truncation check is just the title

            //v-edy: bug484964:-result ID 5143772/6268567, CheckBox/RadioButton also need 18 pixel leave
            if (winInfo.ControlType == ControlType.Group || winInfo.ControlType == ControlType.CheckBox || winInfo.ControlType == ControlType.RadioButton)
            {
               //v-danpgu:17 is the least length we need to leave in order to avoid truncation
                //Decided by creating a group control and type in the title box
                //measure the length right before one line turns to 2 lines.
                //It was proposeSize.Width = (int)(winInfo.Width - 35) before I made the change.
                proposeSize.Width = (int)(winInfo.Width - 18);
            }

            #endregion
            SulpHurSize resultSize = winInfo.TextSize1;

            int mostH = 0;
            for (char a = 'A'; a < 'Z'; a++)
            {
                SulpHurSize unit = winInfo.TextUnit;
                if (unit.Height > mostH)
                    mostH = unit.Height;
            }
            
            //if (proposeSize.Width < resultSize.Width || (resultSize.Height - proposeSize.Height) > mostH / 3)
            if (proposeSize.Width < resultSize.Width || proposeSize.Height < resultSize.Height)
            {
                // xxx temp: Write to log file
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Control Type : {0} ; Control's text : {1} ; \r\n", winInfo.ControlType.ToString(), winInfo.Name);
                sb.AppendFormat("Propose Size - width : {0}, Height : {1} ; \r\n", proposeSize.Width, proposeSize.Height);
                sb.AppendFormat("Result Size - width : {0}, Height : {1} ; \r\n", resultSize.Width, resultSize.Height);
                ////WriteLineToLog(sb.ToString());

                return true;
            }
            return false;
        }

        private bool Win32LabelTruncationCheck(ElementInformation winInfo)
        {
            //return ManagedLabelHasTruncation(graph, textSize, controlSize);
            return false;
        }

        //v-edy: bug485732:-result ID 5078785, Add button rule to truncation rule
        public void CheckButtonVerticalTruncation(ElementInformation button, List<UIComplianceResultBase> results)
        {
            double width = button.Width;
            double height = button.Height;

            //v-edy: bug485732:-result ID 5078785, button on Pane also need check
            if ((button.Parent.ControlType != ControlType.Window && button.Parent.ControlType != ControlType.Pane) ||
               ((button.AutomationId == "Minimize" || button.AutomationId == "Maximize" ||
               button.AutomationId == "Close") &&
               button.Parent.ControlType == ControlType.TitleBar))
                return;

            #region for bug386472 no need to check width for button
            //if (height == 23)
            //{
            //    return;
            //}
            //else
            {
                //The original comparison was at height 23,Let's change it to 20
                string wInfo = string.Format("The height of button should be set to >=20. Current button size is [{0}, {1}]", width, height);
                UIComplianceResultBase tempResult = new UIComplianceResultBase(ResultType.Warning, wInfo, ruleName);
                if ((height < 20))
                {
                    tempResult.Type = ResultType.Fail;
                }
                if (height >= 20)
                {
                    tempResult.Type = ResultType.Warning;
                }
                tempResult.AddRelatedControls(button);
                results.Add(tempResult);
            }
            #endregion
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}

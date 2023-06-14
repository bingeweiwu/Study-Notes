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
                    if (winInfo.ControlType != ControlType.Text && winInfo.ControlType != ControlType.Button &&
                        winInfo.ControlType != ControlType.RadioButton && winInfo.ControlType != ControlType.CheckBox &&
                        winInfo.ControlType != ControlType.Group)
                        continue;

                    if (winInfo.Name == null || winInfo.Name.Length == 0)
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
                    UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Pass, "No truncation found in window : " + Parent.Name, ruleName);
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
        public UIComplianceResultBase CheckManagedControl(ElementInformation winInfo)
        {




            // Check Control truncation according to its type
            //SizeF controlSize = new SizeF(control.Extended.Width, control.Extended.Height);
            bool result = false;

            result = CommonTruncationCheck(winInfo);

            if (result)
            {
                string message = "The " + winInfo.ControlType.ToString() + " control : '" + winInfo.Name + "' is truncated!";
                return (new UIComplianceResultBase(ResultType.Fail, message, this.Name));
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
                string message = "The " + winInfo.ControlType.ToString() + " control : '" + winInfo.Name + "' is truncated!";
                return (new UIComplianceResultBase(ResultType.Fail, message, this.Name));
            }
            return null;
        }

        private bool CommonTruncationCheck(ElementInformation winInfo)
        {
            string text = winInfo.Name;

            if (winInfo.ProposeSize == null)
                return false;

            SulpHurSize proposeSize = winInfo.ProposeSize;

            #region Group Box Title
            //miyan: this is special treat for group box title. 
            // because the BoundingRectangle will get the rect of all group box
            // and the text truncation check is just the title
            if (winInfo.ControlType == ControlType.Group)
            {
                proposeSize.Width = (int)(winInfo.BoundingRectangle.Width - 35);
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

            if (proposeSize.Width < resultSize.Width || (resultSize.Height - proposeSize.Height) > mostH / 3)
            //if (proposeSize.Width < resultSize.Width || proposeSize.Height < resultSize.Height)
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

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;

namespace MS.Internal.SulpHur.CMRules
{
    public class CMAccessKeyRule : UIComplianceRuleBase
    {
        public override string Name
        {
            get { return @"Access Key Rule"; }
        }

        public override string Description
        {
            get { return @"This rule looks for and reports forbidden and duplicated access keys."; }
        }

        public CMAccessKeyRule()
        {
            ruleProperties = new AccessKeyRuleProperties();

            ruleProperties.ForbiddenKeys.Add('i');
            ruleProperties.ForbiddenKeys.Add('l');
            ruleProperties.ForbiddenKeys.Add('.');
            ruleProperties.ForbiddenKeys.Add(':');
            ruleProperties.ForbiddenKeys.Add('g');
            ruleProperties.ForbiddenKeys.Add('j');
            ruleProperties.ForbiddenKeys.Add('p');
            ruleProperties.ForbiddenKeys.Add('q');
            ruleProperties.ForbiddenKeys.Add('y');
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> controls)
        {
            if (!ruleProperties.ExemptedControls.Contains(OK))
            {
                ruleProperties.ExemptedControls.Add(OK);
            }
            if (!ruleProperties.ExemptedControls.Contains(Cancel))
            {
                ruleProperties.ExemptedControls.Add(Cancel);
            }
            if (!ruleProperties.ExemptedControls.Contains(Help))
            {
                ruleProperties.ExemptedControls.Add(Help);
            }

            StringBuilder sb = new StringBuilder();
            string r111 = string.Empty;
            foreach (ElementInformation c111 in controls)
            {
                r111 = PrintTabOrder(c111, 0, sb);
                break;
            }
            WriteToDailyCopyLog(r111);
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            Dictionary<char, ElementInformation> accessKeys = new Dictionary<char, ElementInformation>();
            Dictionary<IntPtr, char> preKeys = new Dictionary<IntPtr, char>();

            foreach (ElementInformation control in controls)
            {
                if (control.ControlType == ControlType.Text)
                {
                    char key = GetAccesskey(control.Name);
                    if (ruleProperties.ForbiddenKeys.Contains(key))
                    {
                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, "Forbidden access key '" + key + "' found at control \"" + control.Name + "\".", Name);
                        result.AddRelatedControls(control);
                        results.Add(result);
                    }
                }
                else if (IsButtonControl(control) && string.IsNullOrEmpty(control.AcceleratorKey))
                {
                    if (control.ControlType == ControlType.Button)
                    {
                        try
                        {
                            ManagedElementInformation mControl = control as ManagedElementInformation;
                            if (mControl.IsImageButton) continue;
                            //Image image = (Image)mControl.GetProperty("Image");
                            //if (image != null)
                            //    continue;
                        }
                        catch
                        {
                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail,
                                string.Format("The {0} control doesn's has shortcut key", control.ControlType), Name);
                            result.AddRelatedControls(control);
                            results.Add(result);
                            continue;
                        }
                    }
                    else
                    {
                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail,
                            string.Format("The {0} control doesn's has shortcut key", control.ControlType), Name);
                        result.AddRelatedControls(control);
                        results.Add(result);
                        continue;
                    }
                }
                if (IsSearchableListview(control))
                    continue;
                if (IsReadOnly(control))
                    continue;
                else if (IsAccessKeyControl(control))
                {
                    if (string.IsNullOrEmpty(control.AcceleratorKey))
                    {
                        if (control is ManagedElementInformation)
                        {
                            ManagedElementInformation mControl = control as ManagedElementInformation;

                            #region Exception
                            //if (!IsEnable(mControl))
                            //    continue;
                            ManagedElementInformation managedInfo = (ManagedElementInformation)control;

                            if (string.IsNullOrEmpty(control.Name) && IsControlContainText(control))
                            {
                                string str1 = "The control doesn't have a shortcut key,"
                                + " because its text is empty in static scan time, plz check it in run time to make sure whether is a bug.";
                                UIComplianceResultBase r2 = new UIComplianceResultBase(ResultType.Warning,
                                    str1, Name);
                                r2.AddRelatedControls(control);
                                results.Add(r2);
                                continue;
                            }
                            #endregion
                            //end Exception

                            ElementInformation lable1 = CheckPreviousIndexControl(managedInfo);
                            char k1;
                            if (lable1 != null)
                            {
                                if (string.IsNullOrEmpty(lable1.Name))
                                {
                                    string str1 = "The control doesn't have a shortcut key,"
                                    + " because the text of its previous label is empty, plz check it in run time to make sure whether is a bug.";
                                    UIComplianceResultBase r2 = new UIComplianceResultBase(ResultType.Warning,
                                        str1, Name);
                                    r2.AddRelatedControls(control);
                                    r2.AddRelatedControls(lable1);
                                    results.Add(r2);
                                    continue;
                                }
                                k1 = GetAccesskey(lable1.Name);
                            }
                            else
                            {
                                k1 = char.MinValue;
                            }

                            if (k1 != char.MinValue)
                            {
                                preKeys.Add((IntPtr)managedInfo.NativeWindowHandle, k1);
                                continue;
                            }

                            //if(lable1 != null)
                            if (CheckPreviousIndexControlIsAccessControl(managedInfo))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            UIComplianceResultBase r3 = new UIComplianceResultBase(ResultType.Warning,
                                string.Format("The {0} control is a native control", control.ControlType), Name);
                            r3.AddRelatedControls(control);
                            results.Add(r3); continue;
                        }

                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail,
                            string.Format("The {0} control doesn's has shortcut key", control.ControlType), Name);
                        result.AddRelatedControls(control);
                        results.Add(result);
                    }
                }
            }


            bool warnInvisible = false;
            foreach (ElementInformation control in controls)
            {
                if (IsAccessKeyControl(control))
                {
                    if (!control.IsOffscreen)
                    {
                        warnInvisible = true;
                    }
                    else
                    {
                        string shortcut = control.AcceleratorKey;
                        char key = char.MinValue;

                        if (!string.IsNullOrEmpty(shortcut))
                        {
                            int indexOfPlus = shortcut.IndexOf('+');
                            if (indexOfPlus > 0)
                                key = char.ToLower(shortcut[indexOfPlus + 1]);
                        }
                        else
                        {
                            key = char.ToLower(GetAccesskey(control.Name));
                            if (key == char.MinValue && preKeys.ContainsKey((IntPtr)control.NativeWindowHandle))
                            {
                                key = preKeys[(IntPtr)control.NativeWindowHandle];
                            }
                        }

                        if (key != char.MinValue)
                        {
                            if (accessKeys.ContainsKey(key))
                            {
                                if (!ControlUnderRadioButtonGroups(accessKeys[key], control))
                                {
                                    UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, "The following controls have the same access key '" + key + "':\n\"" + accessKeys[key].Name + "\"\n\"" + control.Name + "\"", Name);
                                    result.AddRelatedControls(accessKeys[key]);
                                    result.AddRelatedControls(control);
                                    results.Add(result);
                                }
                            }
                            else
                            {
                                accessKeys.Add(key, control);
                            }
                        }
                        else if (ruleProperties.WarnNoAccessKey)
                        {
                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, "\"" + control.Name + "\" does not have an access key.", Name);
                            result.AddRelatedControls(control);
                            results.Add(result);
                        }
                    }
                }
            }

            if (warnInvisible)
            {
                results.Add(new UIComplianceResultBase(ResultType.Warning, "The page contains hidden control(s) and they were not examined.", Name));
            }

            if (results.Count == 0)
            {
                results.Add(new UIComplianceResultBase(ResultType.Pass, "No duplicate access keys found.", Name));
            }

            return results;
        }

        private bool IsButtonControl(ElementInformation control)
        {
            if (ruleProperties.ExemptedControls.Contains(control.Name))
                return false;

            return (control.ControlType == ControlType.CheckBox
                    || control.ControlType == ControlType.Button
                    || control.ControlType == ControlType.RadioButton);
        }

        private bool IsAccessKeyControl(ElementInformation control)
        {
            if (ruleProperties.ExemptedControls.Contains(control.Name))
                return false;

            return (control.ControlType == ControlType.CheckBox
                    || control.ControlType == ControlType.ComboBox
                    || control.ControlType == ControlType.Calendar
                    || control.ControlType == ControlType.SplitButton
                    || control.ControlType == ControlType.Edit
                    || control.ControlType == ControlType.List
                    || control.ControlType == ControlType.DataGrid
                    || control.ControlType == ControlType.NumericUpDown
                    || control.ControlType == ControlType.Button
                    || control.ControlType == ControlType.RadioButton
                    || control.ControlType == ControlType.Spinner
                    || control.ControlType == ControlType.Tree);
        }

        //Also check children
        private bool IsAccessKeyControlEx(ElementInformation control)
        {
            if (!ruleProperties.ExemptedControls.Contains(control.Name))
            {
                bool c1 = (control.ControlType == ControlType.CheckBox
                        || control.ControlType == ControlType.ComboBox
                        || control.ControlType == ControlType.Calendar
                        || control.ControlType == ControlType.SplitButton
                        || control.ControlType == ControlType.Edit
                        || control.ControlType == ControlType.List
                        || control.ControlType == ControlType.DataGrid
                        || control.ControlType == ControlType.NumericUpDown
                        || control.ControlType == ControlType.Button
                        || control.ControlType == ControlType.RadioButton
                        || control.ControlType == ControlType.Spinner
                        || control.ControlType == ControlType.Tree);
                if (c1)
                    return true;
                ElementInformation f1 = control.FirstChild;
                while (f1 != null)
                {
                    bool c2 = IsAccessKeyControlEx(f1);
                    if (c2 == true)
                        return true;
                    f1 = f1.NextSibling;
                }
            }
            return false;
        }

        private bool IsControlContainText(ElementInformation control)
        {
            if (ruleProperties.ExemptedControls.Contains(control.Name))
                return false;

            return (control.ControlType == ControlType.CheckBox
                    || control.ControlType == ControlType.Button
                    || control.ControlType == ControlType.RadioButton);
        }

        private bool IsReadOnly(ManagedElementInformation control)
        {
            return control.IsReadOnly;
        }

        private bool IsReadOnly(ElementInformation control)
        {
            if (control is ManagedElementInformation)
            {
                try
                {
                    ManagedElementInformation mControl = control as ManagedElementInformation;
                    return mControl.IsReadOnly;
                }
                catch { }
                return false;
            }
            else
            {
                return false;
            }
        }

        private bool IsEnable(ManagedElementInformation control)
        {
            return control.IsEnabled;
        }

        private char GetAccesskey(string caption)
        {
            char key;
            if (caption.IndexOf('&') != -1)
            {
                key = caption[caption.IndexOf('&') + 1];
            }
            else
            {
                return char.MinValue;
            }
            return key;
        }

        private char CheckPreviousIndexControl(ICollection<ElementInformation> controls, ManagedElementInformation control)
        {
            ElementInformation pInfo = GetPreviousIndexControl(controls, control);
            if (pInfo == null)
                return char.MinValue;
            if (pInfo.ControlType == ControlType.Text)
            {
                char ac = GetAccesskey(pInfo.Name);
                if (ac != char.MinValue)
                    return ac;
            }
            return char.MinValue;
        }

        private ElementInformation CheckPreviousIndexControl(ElementInformation control)
        {
            try
            {
                ManagedElementInformation cInfo = control as ManagedElementInformation;
                if (cInfo == null)
                    return null;
                int tIndex = cInfo.TabIndex;
                ElementInformation first = control;
                while (first.PreviousSibling != null)
                {
                    first = first.PreviousSibling;
                    //if (IsAccessKeyControl(first) && isRoot)
                    //    return char.MinValue;
                }

                List<ManagedElementInformation> MinControl = new List<ManagedElementInformation>();
                while (first != null)
                {
                    if (first is ManagedElementInformation)
                    {
                        ManagedElementInformation fmInfo = first as ManagedElementInformation;
                        int pIndex = fmInfo.TabIndex;
                        if (pIndex <= tIndex - 1)
                        {
                            MinControl.Add(fmInfo);
                        }
                    }
                    first = first.NextSibling;
                }

                MinControl.Sort(delegate(ManagedElementInformation a, ManagedElementInformation b)
                {
                    return b.TabIndex - a.TabIndex;
                }
                );

                foreach (ManagedElementInformation a1 in MinControl)
                {
                    if (a1.ControlType == ControlType.Text)
                        return a1;
                    //return GetAccesskey(a1.text);
                    if (IsAccessKeyControlEx(a1))
                        return null;
                }


                if (control.Parent != null)
                {
                    return CheckPreviousIndexControl(control.Parent);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool CheckPreviousIndexControlIsAccessControl(ElementInformation control)
        {
            try
            {
                ManagedElementInformation cInfo = control as ManagedElementInformation;
                if (cInfo == null)
                    return false;
                int tIndex = cInfo.TabIndex;
                ElementInformation first = control;
                while (first.PreviousSibling != null)
                {
                    first = first.PreviousSibling;
                }
                List<ManagedElementInformation> MinControl = new List<ManagedElementInformation>();
                while (first != null)
                {
                    if (first is ManagedElementInformation)
                    {
                        ManagedElementInformation fmInfo = first as ManagedElementInformation;
                        int pIndex = fmInfo.TabIndex;
                        if (pIndex <= tIndex - 1)
                        {
                            MinControl.Add(fmInfo);
                            //if(IsAccessKeyControl(fmInfo))
                            //{
                            //    return true;
                            //}
                        }
                    }
                    first = first.NextSibling;
                }

                MinControl.Sort(delegate(ManagedElementInformation a, ManagedElementInformation b)
                {
                    return b.TabIndex - a.TabIndex;
                }
                );

                foreach (ManagedElementInformation a1 in MinControl)
                {
                    if (a1.ControlType == ControlType.Text)
                    {
                        if (!CheckIsHyperLink(a1))
                            return false;
                        return true;
                    }
                    else if (IsAccessKeyControlEx(a1))
                        return true;
                    else
                        continue;
                }


                if (control.Parent != null)
                {
                    return CheckPreviousIndexControlIsAccessControl(control.Parent);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private ElementInformation GetPreviousIndexControl(ICollection<ElementInformation> controls, ManagedElementInformation control)
        {
            int index = control.TabIndex;
            object index2 = control.TabIndex;
            foreach (ElementInformation c1 in controls)
            {
                if (c1 is ManagedElementInformation)
                {
                    ManagedElementInformation mc1 = c1 as ManagedElementInformation;
                    if (mc1.TabIndex == index - 1 && mc1.ControlType == ControlType.Text)
                        return mc1;
                }
            }
            return null;
        }

        private string PrintTabOrder(ElementInformation root, int level, StringBuilder log)
        {
            try
            {
                for (int i = 0; i < level; i++)
                    log.Append("   ");
                object s1 = (root as ManagedElementInformation).TabIndex;
                log.AppendLine(string.Format("{0}  {1}", root.ControlType.ToString(), s1.ToString()));
                ElementInformation child = root.FirstChild;
                while (child != null)
                {
                    PrintTabOrder(child, level + 1, log);
                    child = child.NextSibling;
                }
                return log.ToString();
            }
            catch
            {
                log.AppendLine(string.Format("{0}", root.ControlType.ToString()));
                ElementInformation child = root.FirstChild;
                while (child != null)
                {
                    PrintTabOrder(child, level + 1, log);
                    child = child.NextSibling;
                }
                return log.ToString();
            }
        }

        private bool IsSearchableListview(ElementInformation control)
        {
            try
            {
                if (control.ControlType != ControlType.DataGrid)
                    return false;
                WriteToDailyCopyLog(control.Parent.ControlType.ToString());
                if (control.Parent != null)
                {
                    if (control.Parent.ControlType == ControlType.Unknown || control.Parent.Parent.ControlType == ControlType.Unknown)
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
            //if (control.Parent.BoundingRectangle.Width == control.BoundingRectangle.Width
            //    && control.Parent.BoundingRectangle.Height == control.BoundingRectangle.Height)
            //    return true;
            //return false;
        }

        private bool ControlUnderRadioButtonGroups(ElementInformation c1, ElementInformation c2)
        {
            try
            {
                List<ElementInformation> Parents = new List<ElementInformation>();
                ElementInformation p1 = c1.Parent;
                while (p1 != null)
                {
                    Parents.Add(p1);
                    p1 = p1.Parent;
                }
                Parents.Reverse();

                if (Parents.Count == 0)
                    return false;

                List<ElementInformation> Parents2 = new List<ElementInformation>();
                ElementInformation p2 = c2.Parent;
                while (p2 != null)
                {
                    Parents2.Add(p2);
                    p2 = p2.Parent;
                }
                Parents2.Reverse();
                if (Parents2.Count == 0)
                    return false;

                int i = 0;
                while (Parents[i] == Parents2[i])
                {
                    i++;
                }
                ElementInformation share = Parents2[i - 1];

                ElementInformation child = share.FirstChild;
                int count = 0;
                while (child != null)
                {
                    if (child.ControlType == ControlType.RadioButton)
                        count++;
                    child = child.NextSibling;
                }

                if (count >= 2)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }


        }

        bool CheckIsHyperLink(ElementInformation control)
        {
            if (control.Name.Contains(">>"))
                return true;
            if (control.Name.ToLower().Contains("information"))
                return true;
            return false;
        }

        public static void WriteToDailyCopyLog(string line)
        {
            try
            {
                string mCopyDLLog = "c:\\telescope\\AccessKey.txt";
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

        private AccessKeyRuleProperties ruleProperties;

        private static string _OK = null;
        private static string OK
        {
            get
            {
                if (string.IsNullOrEmpty(_OK))
                {
                    //_OK = Resources.Common.OK.Parsed();
                    return "OK";
                }
                return _OK;
            }
        }

        private static string _Cancel = null;
        private static string Cancel
        {
            get
            {
                if (string.IsNullOrEmpty(_Cancel))
                {
                    //_Cancel = Resources.Common.Cancel.Parsed();
                    return "Cancel";
                }
                return _Cancel;
            }
        }

        private static string _Help = null;
        private static string Help
        {
            get
            {
                if (string.IsNullOrEmpty(_Help))
                {
                    //_Help = Resources.Common.Help.Parsed();
                    return "Help";
                }
                return _Help;
            }
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }

    public class AccessKeyRuleProperties
	{
		public List<char> ForbiddenKeys
		{
			get { return forbiddenKeys; }
			set { forbiddenKeys = value; }
		}
		private List<char> forbiddenKeys = new List<char>();

		public List<string> ExemptedControls
		{
			get { return exemptedControls; }
			set { exemptedControls = value; }
		}
		private List<string> exemptedControls = new List<string>();

		public bool WarnNoAccessKey
		{
			get { return warnNoAccessKey; }
			set { warnNoAccessKey = value; }
		}
		private bool warnNoAccessKey = false;
	}
}

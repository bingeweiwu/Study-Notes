using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;

namespace MS.Internal.SulpHur.CMRules
{
    public class CriticalAccessKeyRule : UIComplianceRuleBase
    {
        public override string Name
        {
            get { return @"Critical Access Key Rule"; }
        }

        public override string Description
        {
            get { return @"This rule looks for and reports forbidden and duplicated access keys with specified controls(Button, RadioButton,CheckBoxes)."; }
        }

        public CriticalAccessKeyRule()
        {
            ruleProperties = new AccessKeyRuleProperties();

            //To aid the visibility of access keys, avoid assigning them to the following characters
            //Letters that are only one pixel wide
            //Letters with descenders
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

        List<ControlType> criticalControls = new List<ControlType>() {ControlType.Button,ControlType.RadioButton,ControlType.CheckBox };
        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            Dictionary<char, ElementInformation> accessKeys = new Dictionary<char, ElementInformation>();
            Dictionary<char, ElementInformation> accessKeys1 = new Dictionary<char, ElementInformation>();
            Dictionary<IntPtr, char> preKeys = new Dictionary<IntPtr, char>();

            List<ElementInformation> filteredControls = new List<ElementInformation>();
            List<ElementInformation> ipAddressControls = new List<ElementInformation>();
            foreach (ElementInformation control in controls)
            {
                //Filter children of comboBox
                if (!control.Ancestors.Any(c => c.ControlType == ControlType.ComboBox))
                {
                    filteredControls.Add(control);
                }

                if (CommonRuleUtility.IsIPAddressControl(control))
                {
                    ipAddressControls.Add(control);
                }
            }

            //Filter children of IPAddress control
            foreach (ElementInformation ei in ipAddressControls)
            {
                foreach (ElementInformation eiC in ei.Children)
                {
                    filteredControls.Remove(eiC);
                }
            }

            UIComplianceResultBase r3 = new UIComplianceResultBase(ResultType.Warning, "Access Key for native control not checked.", Name);
            bool hasNative = false;
            bool hasMiss = false;
            string mislogMessage = "Follow Controls miss short cut.\n";
            UIComplianceResultBase misShortcutResult = new UIComplianceResultBase(ResultType.Fail, mislogMessage, Name);
            bool warnInvisible = false; 
            string hotkeylist = string.Empty;
            string hotkeyControlInfo = string.Empty;
            string suggestkeylist = string.Empty;
            //v-yiwzha Get the Hotkey list
            foreach (ElementInformation control in filteredControls)
            {
                if (IsAccessKeyControl(control))
                {
                    if (control.IsOffscreen)
                    {
                        warnInvisible = true;
                    }
                    else
                    {
                        string shortcut = control.AccessKey;
                        char key = char.MinValue;

                        if (!string.IsNullOrEmpty(shortcut))
                        {
                            int indexOfPlus = shortcut.IndexOf('+');
                            if (indexOfPlus > 0)
                                key = char.ToLower(shortcut[indexOfPlus + 1]);
                        }
                        else
                        {
                            if (control.Name != null)
                            {
                                key = char.ToLower(GetAccesskey(control.Name));
                                if (key == char.MinValue && preKeys.ContainsKey((IntPtr)control.NativeWindowHandle))
                                {
                                    key = preKeys[(IntPtr)control.NativeWindowHandle];
                                }
                            }
                        }

                        if (key != char.MinValue)
                        {
                            if (!accessKeys.ContainsKey(key))
                            {
                                accessKeys.Add(key, control);
                                hotkeylist = hotkeylist + "'" + key + "',";
                                hotkeyControlInfo = hotkeyControlInfo + "'" + key + "' in control \"" + control.Name + "\" \n";
                            }
                            else
                            {
                                hotkeyControlInfo = hotkeyControlInfo + "'" + key + "' in control \"" + control.Name + "\" \n";
                            }
                        }
                    }
                }
            }
            foreach (ElementInformation control in filteredControls)
            {
                if (!criticalControls.Contains(control.ControlType)) {
                    continue;
                }
                //v-danpgu: Filter Access Key No needed
                //if (control.AutomationId == "okButton" || control.AutomationId == "cancelButton" || control.AutomationId == "_btnCancel" || control.AutomationId == "buttonImage"
                //    || control.AutomationId == "buttonNew" || control.AutomationId == "buttonDelete" || control.AutomationId == "buttonMoveUp" || control.AutomationId == "buttonMoveDown"
                //    || control.AutomationId == "buttonProperties" || control.AutomationId == "buttonPropertyCriteria" || control.AutomationId == "buttonNewCriteria" || control.AutomationId == "buttonDeleteCriteria"
                //    || control.AutomationId == "buttonGroup" || control.AutomationId == "buttonUnGroup" || control.AutomationId == "buttonOR" || control.AutomationId == "buttonNot" || control.AutomationId == "_infoCollapseButton"
                //    || control.AutomationId == "_viewReportCollapseButton" || (control.ControlType == ControlType.Button && control.Name == "确定" || control.Name == "取消" || control.Name == "是" || control.Name == "否"))
                //    hasMiss = false;

                //v-danpgu: Filter Image buttons if IsImageButton = true
                if (control.IsImageButton == true || control.AutomationId == "buttonOK" || control.AutomationId == "buttonCancel" || control.AutomationId == "buttonSelect" || control.AutomationId == "okButton" || control.AutomationId == "cancelButton" || control.AutomationId == "_btnCancel" || control.AutomationId == "buttonImage"
                    || control.AutomationId == "buttonNew" || control.AutomationId == "_infoCollapseButton"|| control.AutomationId == "_viewReportCollapseButton" || (control.ControlType == ControlType.Button && control.Name == "确定" || control.Name == "取消" || control.Name == "是" || control.Name == "否"))
                    hasMiss = false;
                else
                {
                    #region check accesskey for button control
                    if (IsButtonControl(control) && string.IsNullOrEmpty(control.AccessKey))
                    {
                        if (control.ControlType == ControlType.Button)
                        {
                            try
                            {
                                ElementInformation mControl = control;
                                if (mControl.IsImageButton) continue;
                            }
                            catch
                            {
                                hasMiss = true;
                                misShortcutResult.AddRelatedControls(control);
                                continue;
                            }
                        }
                        else
                        {
                            hasMiss = true;
                            misShortcutResult.AddRelatedControls(control);
                            continue;
                        }
                    }
                    #endregion

                    if (control.IsReadOnly)
                        continue;
                    else if (IsAccessKeyControl(control) && string.IsNullOrEmpty(control.AccessKey))
                    {
                        if (control.IsManagedControlProperty)
                        {
                            #region Check AccessKey
                            ElementInformation lable1 = CheckPreviousIndexControl(control);
                            char k1;
                            if (lable1 != null)
                            {
                                if (string.IsNullOrEmpty(lable1.Name))
                                {
                                    string str1 = "The control doesn't have a shortcut key,because the text of its previous label is empty";
                                    UIComplianceResultBase r2 = new UIComplianceResultBase(ResultType.Warning, str1, Name);
                                    r2.AddRelatedControls(control);
                                    r2.AddRelatedControls(lable1);
                                    results.Add(r2);
                                    continue;
                                }
                                if (lable1.Name != null)
                                {
                                    if (string.IsNullOrEmpty(lable1.AccessKey))
                                    {
                                        k1 = GetAccesskey(lable1.Name);
                                    }
                                    else
                                    {
                                        int indexOfPlus = lable1.AccessKey.IndexOf('+');
                                        if (indexOfPlus > 0)
                                            k1 = char.ToLower(lable1.AccessKey[indexOfPlus + 1]);
                                        else
                                            k1 = char.MinValue;
                                    }
                                }
                                else
                                {
                                    k1 = char.MinValue;
                                }
                            }
                            else
                            {
                                k1 = char.MinValue;
                            }

                            if (k1 != char.MinValue)
                            {
                                preKeys.Add((IntPtr)control.NativeWindowHandle, k1);
                                continue;
                            }

                            if (CheckPreviousIndexControlIsAccessControl(control))
                            {
                                continue;
                            }
                            #endregion
                        }
                        else
                        {
                            r3.AddRelatedControls(control);
                            hasNative = true;
                            continue;
                        }

                        hasMiss = true;
                        misShortcutResult.AddRelatedControls(control);
                    }
                    char suggestkey = char.MinValue;
                    if (control.Name != null && misShortcutResult.Controls.Contains(control))
                    {
                        for (int i = 0; i < control.Name.Length; i++)
                        {
                            char c = control.Name.ToLower()[i];
                            //v-yiwzha: filter the DBCS language with unicode
                            if ((c >= 0x4e00 && c <= 0x9fbf) || (c >= 0x1100 && c <= 0x11ff) || (c >= 0x3040 && c <= 0x309f) || (c >= 0x30a0 && c <= 0x30ff))
                            {
                                suggestkey = '+';
                                break;
                            }
                            //v-yiwzha give suggestkey, make sure suggestkey are not in current page && is an englisth lettle && not in suggestkeylist
                            if (!accessKeys.ContainsKey(c) && c <= 'z' && c >= 'a' && !suggestkeylist.Contains(c))
                            {
                                suggestkey = c;
                                break;
                            }
                            //v-yiwzha not find available suggestkey
                            if (i == control.Name.Length - 1)
                            {
                                mislogMessage += " In Control \"" + control.Name + "\" No available hotkey to sign, need to check manually\n";
                            }
                        }
                    }
                    if (suggestkey != char.MinValue)
                    {
                        if (suggestkey == '+')
                        {
                            mislogMessage = "Follow Controls miss short cut. But the ui is DBCS language, same hotkey with ENU, please verify ENU language for more information.";
                        }
                        else
                        {
                            mislogMessage += " Suggestion: Use '" + suggestkey + "' in \"" + control.Name + "\"\n";
                            //v-yiwzha add suggestkey to suggestkeylist
                            suggestkeylist += suggestkey;
                        }
                    }
                }
            }
            if (mislogMessage != "Follow Controls miss short cut. But the ui is DBCS language, same hotkey with ENU, please verify ENU language for more information.")
            { 
                mislogMessage += " More Control Infomation:\n Following are all hotkeys in this page: " + hotkeylist + "\nHotkeys In Control:\n" + hotkeyControlInfo;
            }
            misShortcutResult.Message = mislogMessage;
            if (hasNative) results.Add(r3);
            if (hasMiss) results.Add(misShortcutResult);          

            string logMessage = string.Empty;
            ResultType rt= ResultType.Fail;
            foreach (ElementInformation control in filteredControls)
            {
                if (IsAccessKeyControl(control))
                {
                    if (control.IsOffscreen)
                    {
                        warnInvisible = true;
                    }
                    else
                    {
                        string shortcut = control.AccessKey;
                        char key = char.MinValue;
                        char suggestkey = char.MinValue;
                        if (!string.IsNullOrEmpty(shortcut))
                        {
                            int indexOfPlus = shortcut.IndexOf('+');
                            if (indexOfPlus > 0)
                                key = char.ToLower(shortcut[indexOfPlus + 1]);
                        }
                        else
                        {
                            if (control.Name != null)
                            {
                                key = char.ToLower(GetAccesskey(control.Name));
                                if (key == char.MinValue && preKeys.ContainsKey((IntPtr)control.NativeWindowHandle))
                                {
                                    key = preKeys[(IntPtr)control.NativeWindowHandle];
                                }
                            }
                        }

                        if (control.Name != null)
                        {
                            for (int i = 0; i < control.Name.Length; i++)
                            {
                                char c = control.Name.ToLower()[i];
                                //v-yiwzha: filter the DBCS language with unicode
                                if ((c >= 0x4e00 && c <= 0x9fbf) || (c >= 0x1100 && c <= 0x11ff) || (c >= 0x3040 && c <= 0x309f) || (c >= 0x30a0 && c <= 0x30ff))
                                {
                                    suggestkey = '+';
                                    break;
                                }
                                //v-yiwzha give suggestkey, make sure suggestkey are not in current page && is an englisth lettle && not in suggestkeylist
                                if (!accessKeys.ContainsKey(c) && c <= 'z' && c >= 'a' && !suggestkeylist.Contains(c))
                                {
                                    suggestkey = c;
                                    break;
                                }
                                //v-yiwzha not find available suggestkey
                                if (i == control.Name.Length - 1)
                                {
                                    logMessage = "No available hotkey to sign, need to check manually";
                                }
                            }
                        }


                        if (key != char.MinValue)
                        {
                            if (accessKeys1.ContainsKey(key))
                            {
                                if (!ControlUnderRadioButtonGroups(accessKeys1[key], control))
                                {
                                    if (logMessage != "No available hotkey to sign, need to check manually")
                                    {
                                        if (suggestkey == '+')
                                        {
                                            logMessage = "The following controls have the same access key '" + key + "': \"" + accessKeys1[key].Name + "\" \"" + control.Name + "\".\nBut the ui is DBCS language, same hotkey with ENU, please verify ENU language for more information.";
                                            rt = ResultType.Warning;
                                        }
                                        else
                                        {
                                            logMessage = "The following controls have the same access key '" + key + "': \"" + accessKeys1[key].Name + "\" \"" + control.Name + "\".\nSuggestion: Use '" + suggestkey + "' in \"" + control.Name + "\"" + "\nMore Control Infomation:\n" + " Following are all hotkeys in this page: " + hotkeylist + "\nHotkeys In Control:\n" + hotkeyControlInfo;
                                            //v-yiwzha add suggestkey to suggestkeylist
                                            suggestkeylist += suggestkey;
                                        }  
                                    }
                                    else
                                    {
                                        logMessage = "The following controls have the same access key '" + key + "': \"" + accessKeys1[key].Name + "\" \"" + control.Name + "\".\nBut in control \"" + control.Name + "\" " + logMessage + ".\nMore Control Infomation:\n" + " Following are all hotkeys in this page: " + hotkeylist + "\nHotkeys In Control:\n" + hotkeyControlInfo;
                                    }
                                    UIComplianceResultBase result = new UIComplianceResultBase(rt, logMessage, Name);
                                    result.AddRelatedControls(accessKeys1[key]);
                                    result.AddRelatedControls(control);
                                    results.Add(result);
                                }
                            }
                            else
                            {
                                accessKeys1.Add(key, control);
                            }
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
                results.Add(new UIComplianceResultBase(ResultType.Pass, "No access keys issue found.", Name));
            }

            return results;
        }

        private bool IsButtonControl(ElementInformation control)
        {
            if (control.IsDefaultShortcutButton)
                return false;

            return (control.ControlType == ControlType.CheckBox
                    || control.ControlType == ControlType.Button
                    || control.ControlType == ControlType.RadioButton);
        }

        private bool IsAccessKeyControl(ElementInformation control)
        {
            if (control.IsDefaultShortcutButton)
                return false;

            return (control.ControlType == ControlType.CheckBox
                    || control.ControlType == ControlType.ComboBox
                    || control.ControlType == ControlType.Calendar
                    || control.ControlType == ControlType.Edit
                    || control.ControlType == ControlType.List
                    || control.ControlType == ControlType.DataGrid
                    || CommonRuleUtility.IsNumericUpandDown(control)
                    || CommonRuleUtility.IsIPAddressControl(control)
                    || control.ControlType == ControlType.Button
                    || control.ControlType == ControlType.RadioButton
                    || control.ControlType == ControlType.Tree);
        }

        //Also check children
        private bool IsAccessKeyControlEx(ElementInformation control)
        {
            if (!control.IsDefaultShortcutButton)
            {
                bool c1 = (control.ControlType == ControlType.CheckBox
                        || control.ControlType == ControlType.ComboBox
                        || control.ControlType == ControlType.Calendar
                        || control.ControlType == ControlType.SplitButton
                        || control.ControlType == ControlType.Edit
                        || control.ControlType == ControlType.List
                        || control.ControlType == ControlType.DataGrid
                        || CommonRuleUtility.IsNumericUpandDown(control)
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
            if (control.IsDefaultShortcutButton)
                return false;

            return (control.ControlType == ControlType.CheckBox
                    || control.ControlType == ControlType.Button
                    || control.ControlType == ControlType.RadioButton);
        }

        private char GetAccesskey(string caption)
        {
            char key;
            int indexOfAnd = caption.IndexOf('&');
            int length = caption.Length;
            if (indexOfAnd != -1 && caption.Length >= 2 && indexOfAnd < length - 1)
            {
                key = caption[indexOfAnd + 1];
            }
            else
            {
                return char.MinValue;
            }
            return key;
        }

        private char CheckPreviousIndexControl(ICollection<ElementInformation> controls, ElementInformation control)
        {
            ElementInformation pInfo = GetPreviousIndexControl(controls, control);
            if (pInfo == null)
                return char.MinValue;
            if (pInfo.ControlType == ControlType.Text)
            {
                if (pInfo.Name != null)
                {
                    char ac = GetAccesskey(pInfo.Name);
                    if (ac != char.MinValue)
                        return ac;
                }
            }
            return char.MinValue;
        }

        private ElementInformation CheckPreviousIndexControl(ElementInformation control)
        {
            try
            {
                int tIndex = control.TabIndex;
                ElementInformation first = control;
                while (first.PreviousSibling != null)
                {
                    first = first.PreviousSibling;
                }

                List<ElementInformation> MinControl = new List<ElementInformation>();
                while (first != null)
                {
                    if (first.IsManagedControlProperty)
                    {
                        int pIndex = first.TabIndex;
                        if (pIndex <= tIndex - 1)
                        {
                            MinControl.Add(first);
                        }
                    }
                    first = first.NextSibling;
                }

                MinControl.Sort(delegate(ElementInformation a, ElementInformation b)
                {
                    return b.TabIndex - a.TabIndex;
                }
                );

                foreach (ElementInformation a1 in MinControl)
                {
                    if (a1.ControlType == ControlType.Text)
                        return a1;
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
                ElementInformation cInfo = control;
                if (cInfo == null)
                    return false;
                int tIndex = cInfo.TabIndex;
                ElementInformation first = control;
                while (first.PreviousSibling != null)
                {
                    first = first.PreviousSibling;
                }
                List<ElementInformation> MinControl = new List<ElementInformation>();
                while (first != null)
                {
                    if (first.IsManagedControlProperty)
                    {
                        ElementInformation fmInfo = first;
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

                MinControl.Sort(delegate(ElementInformation a, ElementInformation b)
                {
                    return b.TabIndex - a.TabIndex;
                }
                );

                foreach (ElementInformation a1 in MinControl)
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

        private ElementInformation GetPreviousIndexControl(ICollection<ElementInformation> controls, ElementInformation control)
        {
            int index = control.TabIndex;
            object index2 = control.TabIndex;
            foreach (ElementInformation c1 in controls)
            {
                if (c1.IsManagedControlProperty)
                {
                    ElementInformation mc1 = c1;
                    if (mc1.TabIndex == index - 1 && mc1.ControlType == ControlType.Text)
                        return mc1;
                }
            }
            return null;
        }

        private bool ControlUnderRadioButtonGroups(ElementInformation c1, ElementInformation c2)
        {
            if (c1.ControlType == ControlType.RadioButton && c2.ControlType == ControlType.RadioButton &&
                c1.Parent == c2.Parent && c1.Name == c2.Name)
            {
                return true;
            }
            return false;
            //try
            //{
            //    List<ElementInformation> Parents = new List<ElementInformation>();
            //    ElementInformation p1 = c1.Parent;
            //    while (p1 != null)
            //    {
            //        Parents.Add(p1);
            //        p1 = p1.Parent;
            //    }
            //    Parents.Reverse();

            //    if (Parents.Count == 0)
            //        return false;

            //    List<ElementInformation> Parents2 = new List<ElementInformation>();
            //    ElementInformation p2 = c2.Parent;
            //    while (p2 != null)
            //    {
            //        Parents2.Add(p2);
            //        p2 = p2.Parent;
            //    }
            //    Parents2.Reverse();
            //    if (Parents2.Count == 0)
            //        return false;

            //    int i = 0;
            //    while (Parents[i] == Parents2[i])
            //    {
            //        i++;
            //    }
            //    ElementInformation share = Parents2[i - 1];

            //    ElementInformation child = share.FirstChild;
            //    int count = 0;
            //    while (child != null)
            //    {
            //        if (child.ControlType == ControlType.RadioButton)
            //            count++;
            //        child = child.NextSibling;
            //    }

            //    if (count >= 2)
            //        return true;
            //    return false;
            //}
            //catch
            //{
            //    return false;
            //}


        }

        bool CheckIsHyperLink(ElementInformation control)
        {
            if (control.Name.Contains(">>"))
                return true;
            if (control.Name.ToLower().Contains("information"))
                return true;
            return false;
        }

        private AccessKeyRuleProperties ruleProperties;

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}

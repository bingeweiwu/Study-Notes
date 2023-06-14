using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;

namespace MS.Internal.SulpHur.CMRules
{
    public class CMAccessKeyDupExceptRule : UIComplianceRuleBase
    {
        public override string Name
        {
            get { return @"Access Key non Duplicate Rule"; }
        }

        public override string Description
        {
            get { return @"This rule looks for all access key rules except duplicate access keys."; }
        }

        public CMAccessKeyDupExceptRule()
        {
            ruleProperties = new AccessKeyDuplicateExceptRuleProperties();

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

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> controls)
        {

            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            if (lan != "ENU")
            {
                results.Add(new UIComplianceResultBase(ResultType.Warning, $"{Name} only works on ENU. Others will set to warning.", Name));
                return results;
            }
            if (this.isOSidentity == false)
            {
                results.Add(new UIComplianceResultBase(ResultType.Warning, "This UI is already verified in different OS type.\n", Name));
                return results;
            }
            else
            {
                Dictionary<char, ElementInformation> accessKeys = new Dictionary<char, ElementInformation>();
                Dictionary<char, ElementInformation> accessKeys1 = new Dictionary<char, ElementInformation>();
                Dictionary<char, ElementInformation> suggestaccessKeys = new Dictionary<char, ElementInformation>();
                Dictionary<IntPtr, char> preKeys = new Dictionary<IntPtr, char>();
                int acccontrolcount = 0;
                List<ElementInformation> filteredControls = new List<ElementInformation>();
                List<ElementInformation> ipAddressControls = new List<ElementInformation>();
                foreach (ElementInformation control in controls)
                {

                    if (IsAccessKeyControl(control))
                    {
                        acccontrolcount++;
                    }

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

                //387756 need to change it for UI pages with only 1 input control
                if (acccontrolcount < 2)
                {
                    results.Add(new UIComplianceResultBase(ResultType.Pass, "No access keys issue found.", Name));
                    return results;
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
                string forbiddenkeylist = " 1234567890,.<>()-=+×÷*/{}[]|!%:;'\"\\";
                string unpreferredkeyp5list = "il";
                string unpreferredkeyp2list = "ąęğşçÇýþÿ¿";
                string unpreferredkeyp4list = "gjpqy";
                string unpreferredkeyp3list = "ÀÁÂÃÄÅÆÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàâæžéříìîðúùûëåäüöøáïščěêóãłńźżıñőòôõ";
                string unpreferredkeyp1list = "ansp";
                string unpreferredkeylist = unpreferredkeyp1list + unpreferredkeyp2list + unpreferredkeyp3list + unpreferredkeyp4list + unpreferredkeyp5list;
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
                int controlPosition = 0;
                if (lan == "ENU")
                {
                    foreach (ElementInformation control in filteredControls)
                    {
                        //v-yiwzha: Filter Access Key No needed
                        if (control.IsImageButton == true || control.AutomationId == "buttonOK" || control.AutomationId == "buttonCancel"
                            || control.AutomationId == "buttonSelect" || control.AutomationId == "okButton" || control.AutomationId == "cancelButton"
                            || control.AutomationId == "_btnCancel" || control.AutomationId == "buttonImage" || control.AutomationId == "buttonNew"
                            || control.AutomationId == "_infoCollapseButton" || control.AutomationId == "_viewReportCollapseButton"
                            || control.AutomationId == "txtBoxSearch" || control.AutomationId == "comboBoxCollectionType" || control.AutomationId == "treeViewControl"
                            || control.AutomationId == "listView" || control.AutomationId == "groupListView" || control.AutomationId == "treeViewPlatforms"
                            || control.AutomationId == "treeViewSteps" || control.ControlType == ControlType.ComboBox
                            || (control.ControlType == ControlType.Button && control.Name == "确定" || control.Name == "取消" || control.Name == "是" || control.Name == "否"))
                        {
                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, "These buttons might not need access key, need more investigation", Name);
                            result.AddRelatedControls(control);
                            results.Add(result);
                            continue;
                        }
                        //v-danpgu: Filter Pseudo loc string
                        //v-edy: Logic forbidden
                        if (!string.IsNullOrEmpty(control.Name) && control.Name.StartsWith("["))
                        {
                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, "This is a Pseudo Loc string, might not need access key, need more investigation", Name);
                            result.AddRelatedControls(control);
                            results.Add(result);
                            continue;
                        }
                        else
                        {
                            #region check forbidden accessKey in text control
                            if (control.ControlType == ControlType.Text)
                            {
                                if (!string.IsNullOrEmpty(control.Name))
                                {
                                    char key = GetAccesskey(control.Name);
                                    if (ruleProperties.ForbiddenKeys.Contains(key))
                                    {
                                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, "Forbidden access key '" + key + "' found at control \"" + CommonRuleUtility.TruncateControlFullName(control.Name) + "\".", Name);
                                        result.AddRelatedControls(control);
                                        results.Add(result);
                                    }
                                }
                            }
                            #endregion

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

                            //Comment for bug:396106
                            //if (control.IsReadOnly)
                            //    continue;
                            else if (IsAccessKeyControl(control) && string.IsNullOrEmpty(control.AccessKey))
                            {
#if DEBUG
                                if (control.AutomationId == "textBoxWebAppURL")
                                {
                                    int stop = 1;
                                }
#endif
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
                                controlPosition += 1;
                                // v-yiwzha: find previous control name when current control name is null.
                                if (string.IsNullOrEmpty(control.Name))
                                {
                                    ElementInformation lable1 = CheckPreviousIndexControl(control);
                                    if (lable1 != null)
                                    {
                                        if (!string.IsNullOrEmpty(lable1.Name))
                                        {
                                            control.Name = lable1.Name;
                                            mislogMessage += controlPosition;
                                            mislogMessage += ": Could not find control name for highlight control.";
                                            mislogMessage += ", possible control name: \"";
                                            mislogMessage += control.Name;
                                            mislogMessage += "\"";
                                        }
                                        else
                                        {
                                            mislogMessage += controlPosition;
                                            mislogMessage += ": Could not find control name and possible control name for highlight control.";
                                        }
                                    }
                                    else
                                    {
                                        mislogMessage += controlPosition;
                                        mislogMessage += ": Could not find previous control for highlight control.";
                                    }
                                }
                            }
                            char suggestkey = char.MinValue;
                            if (control.Name != null && misShortcutResult.Controls.Contains(control))
                            {
                                string unpreferredList = string.Empty;
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
                                    if (!accessKeys.ContainsKey(c) && !suggestkeylist.Contains(c) && !forbiddenkeylist.Contains(c) && !unpreferredkeylist.Contains(c))
                                    {
                                        suggestkey = c;
                                        break;
                                    }
                                    //v-yiwzha use unpreferred key to sign
                                    if (!accessKeys.ContainsKey(c) && !suggestkeylist.Contains(c) && !forbiddenkeylist.Contains(c) && unpreferredkeylist.Contains(c))
                                    {
                                        unpreferredList += c;
                                    }
                                    if (i == control.Name.Length - 1 && unpreferredList != string.Empty)
                                    {
                                        int priority = 0;
                                        for (int j = 0; j < unpreferredList.Length; j++)
                                        {
                                            if (unpreferredkeyp5list.Contains(unpreferredList[j]))
                                            {
                                                if (priority < 5)
                                                {
                                                    priority = 5;
                                                    suggestkey = unpreferredList[j];
                                                }
                                            }
                                            if (unpreferredkeyp4list.Contains(unpreferredList[j]))
                                            {
                                                if (priority < 4)
                                                {
                                                    priority = 4;
                                                    suggestkey = unpreferredList[j];
                                                }
                                            }
                                            if (unpreferredkeyp3list.Contains(unpreferredList[j]))
                                            {
                                                if (priority < 3)
                                                {
                                                    priority = 3;
                                                    suggestkey = unpreferredList[j];
                                                }
                                            }
                                            if (unpreferredkeyp2list.Contains(unpreferredList[j]))
                                            {
                                                if (priority < 2)
                                                {
                                                    priority = 2;
                                                    suggestkey = unpreferredList[j];
                                                }
                                            }
                                            if (unpreferredkeyp1list.Contains(unpreferredList[j]))
                                            {
                                                if (priority < 1)
                                                {
                                                    priority = 1;
                                                    suggestkey = unpreferredList[j];
                                                }
                                            }
                                        }
                                        mislogMessage += " Unpreferred";
                                    }
                                    //v-yiwzha not find available suggestkey
                                    if (i == control.Name.Length - 1 && unpreferredList == string.Empty)
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
                                    break;
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
                }
                else
                {
                    results.Add(new UIComplianceResultBase(ResultType.Warning, "Hotkey Miss Rule Only works on ENU. Others will set to warning.\n", Name));
                }
                string logMessage = string.Empty;
                ResultType rt = ResultType.Fail;
                suggestkeylist = string.Empty;
                #region Commentout
                //foreach (ElementInformation control in filteredControls)
                //{
                //    if (IsAccessKeyControl(control))
                //    {
                //        if (control.IsOffscreen)
                //        {
                //            warnInvisible = true;
                //        }
                //        else
                //        {
                //            string shortcut = control.AccessKey;
                //            char key = char.MinValue;
                //            char suggestkey = char.MinValue;
                //            if (!string.IsNullOrEmpty(shortcut))
                //            {
                //                int indexOfPlus = shortcut.IndexOf('+');
                //                if (indexOfPlus > 0)
                //                    key = char.ToLower(shortcut[indexOfPlus + 1]);
                //            }
                //            else
                //            {
                //                if (control.Name != null)
                //                {
                //                    key = char.ToLower(GetAccesskey(control.Name));
                //                    if (key == char.MinValue && preKeys.ContainsKey((IntPtr)control.NativeWindowHandle))
                //                    {
                //                        key = preKeys[(IntPtr)control.NativeWindowHandle];
                //                    }
                //                }
                //            }
                //            // v-yiwzha: find previous control name when current control name is null.
                //            if (control.Name == null || control.Name == "")
                //            {
                //                ElementInformation lable1 = CheckPreviousIndexControl(control);
                //                if (lable1 != null)
                //                {
                //                    if (lable1.Name != null)
                //                    {
                //                        control.Name = lable1.Name;
                //                    }
                //                }
                //            }
                //            if (control.Name != null)
                //            {
                //                string unpreferredList = string.Empty;
                //                for (int i = 0; i < control.Name.Length; i++)
                //                {
                //                    char c = control.Name.ToLower()[i];
                //                    //v-yiwzha: filter the DBCS language with unicode
                //                    if ((c >= 0x4e00 && c <= 0x9fbf) || (c >= 0x1100 && c <= 0x11ff) || (c >= 0x3040 && c <= 0x309f) || (c >= 0x30a0 && c <= 0x30ff))
                //                    {
                //                        suggestkey = '+';
                //                        logMessage = string.Empty;
                //                        break;
                //                    }
                //                    //v-yiwzha give suggestkey, make sure suggestkey are not in current page && is an englisth lettle && not in suggestkeylist
                //                    if (!accessKeys.ContainsKey(c) && !suggestkeylist.Contains(c) && !forbiddenkeylist.Contains(c) && !unpreferredkeylist.Contains(c))
                //                    {
                //                        suggestkey = c;
                //                        logMessage = string.Empty;
                //                        break;
                //                    }
                //                    //v-yiwzha used not-prefered key to sign
                //                    if (!accessKeys.ContainsKey(c) && !suggestkeylist.Contains(c) && !forbiddenkeylist.Contains(c) && unpreferredkeylist.Contains(c))
                //                    {
                //                        unpreferredList += c;
                //                    }
                //                    if (i == control.Name.Length - 1 && unpreferredList != string.Empty)
                //                    {
                //                        int priority = 0;
                //                        for (int j = 0; j < unpreferredList.Length; j++)
                //                        {
                //                            if (unpreferredkeyp5list.Contains(unpreferredList[j]))
                //                            {
                //                                if (priority < 5)
                //                                {
                //                                    priority = 5;
                //                                    suggestkey = unpreferredList[j];
                //                                }
                //                            }
                //                            if (unpreferredkeyp4list.Contains(unpreferredList[j]))
                //                            {
                //                                if (priority < 4)
                //                                {
                //                                    priority = 4;
                //                                    suggestkey = unpreferredList[j];
                //                                }
                //                            }
                //                            if (unpreferredkeyp3list.Contains(unpreferredList[j]))
                //                            {
                //                                if (priority < 3)
                //                                {
                //                                    priority = 3;
                //                                    suggestkey = unpreferredList[j];
                //                                }
                //                            }
                //                            if (unpreferredkeyp2list.Contains(unpreferredList[j]))
                //                            {
                //                                if (priority < 2)
                //                                {
                //                                    priority = 2;
                //                                    suggestkey = unpreferredList[j];
                //                                }
                //                            }
                //                            if (unpreferredkeyp1list.Contains(unpreferredList[j]))
                //                            {
                //                                if (priority < 1)
                //                                {
                //                                    priority = 1;
                //                                    suggestkey = unpreferredList[j];
                //                                }
                //                            }
                //                        }
                //                        logMessage = " The suggest key is unpreferred P" + priority + " since all preferred keys are assigned.";
                //                    }
                //                    //v-yiwzha not find available suggestkey
                //                    if (i == control.Name.Length - 1 && unpreferredList == string.Empty)
                //                    {
                //                        logMessage = "No available hotkey to sign, need to check manually";
                //                    }
                //                }
                //            }
                //            string comments = string.Empty;
                //            if (key != char.MinValue)
                //            {
                //                if (accessKeys1.ContainsKey(key))
                //                {
                //                    string changedControlName = "Both2CommonControls";
                //                    ElementInformation changedControl = null;
                //                    //v-yiwzha Bug 469622: filter all Common controls, Bug 469623: assign HK from long one
                //                    if (control.AutomationId != "_btnPrevious" && control.AutomationId != "_btnNext" && control.AutomationId != "_btnFinish" && control.AutomationId != "_btnCancel" && control.AutomationId != "okButton" && control.AutomationId != "cancelButton" && control.AutomationId != "applyButton")
                //                    {
                //                        changedControlName = control.Name;
                //                        changedControl = control;
                //                        if (accessKeys1[key].AutomationId != "_btnPrevious" && accessKeys1[key].AutomationId != "_btnNext" && accessKeys1[key].AutomationId != "_btnFinish" && accessKeys1[key].AutomationId != "_btnCancel" && accessKeys1[key].AutomationId != "okButton" && accessKeys1[key].AutomationId != "cancelButton" && accessKeys1[key].AutomationId != "applyButton" && accessKeys1[key].Name.Length > control.Name.Length)
                //                        {
                //                            if (accessKeys1[key].Name != null)
                //                            {
                //                                string unpreferredList = string.Empty;
                //                                for (int i = 0; i < accessKeys1[key].Name.Length; i++)
                //                                {
                //                                    char c = accessKeys1[key].Name.ToLower()[i];
                //                                    //v-yiwzha: filter the DBCS language with unicode
                //                                    if ((c >= 0x4e00 && c <= 0x9fbf) || (c >= 0x1100 && c <= 0x11ff) || (c >= 0x3040 && c <= 0x309f) || (c >= 0x30a0 && c <= 0x30ff))
                //                                    {
                //                                        suggestkey = '+';
                //                                        logMessage = string.Empty;
                //                                        break;
                //                                    }
                //                                    //v-yiwzha give suggestkey, make sure suggestkey are not in current page && is an englisth lettle && not in suggestkeylist
                //                                    if (!accessKeys.ContainsKey(c) && !suggestkeylist.Contains(c) && !forbiddenkeylist.Contains(c) && !unpreferredkeylist.Contains(c))
                //                                    {
                //                                        suggestkey = c;
                //                                        logMessage = string.Empty;
                //                                        break;
                //                                    }
                //                                    //v-yiwzha used not-prefered key to sign
                //                                    if (!accessKeys.ContainsKey(c) && !suggestkeylist.Contains(c) && !forbiddenkeylist.Contains(c) && unpreferredkeylist.Contains(c))
                //                                    {
                //                                        unpreferredList += c;
                //                                    }
                //                                    if (i == accessKeys1[key].Name.Length - 1 && unpreferredList != string.Empty)
                //                                    {
                //                                        int priority = 0;
                //                                        for (int j = 0; j < unpreferredList.Length; j++)
                //                                        {
                //                                            if (unpreferredkeyp5list.Contains(unpreferredList[j]))
                //                                            {
                //                                                if (priority < 5)
                //                                                {
                //                                                    priority = 5;
                //                                                    suggestkey = unpreferredList[j];
                //                                                }
                //                                            }
                //                                            if (unpreferredkeyp4list.Contains(unpreferredList[j]))
                //                                            {
                //                                                if (priority < 4)
                //                                                {
                //                                                    priority = 4;
                //                                                    suggestkey = unpreferredList[j];
                //                                                }
                //                                            }
                //                                            if (unpreferredkeyp3list.Contains(unpreferredList[j]))
                //                                            {
                //                                                if (priority < 3)
                //                                                {
                //                                                    priority = 3;
                //                                                    suggestkey = unpreferredList[j];
                //                                                }
                //                                            }
                //                                            if (unpreferredkeyp2list.Contains(unpreferredList[j]))
                //                                            {
                //                                                if (priority < 2)
                //                                                {
                //                                                    priority = 2;
                //                                                    suggestkey = unpreferredList[j];
                //                                                }
                //                                            }
                //                                            if (unpreferredkeyp1list.Contains(unpreferredList[j]))
                //                                            {
                //                                                if (priority < 1)
                //                                                {
                //                                                    priority = 1;
                //                                                    suggestkey = unpreferredList[j];
                //                                                }
                //                                            }
                //                                        }
                //                                        logMessage = " The suggest key is unpreferred P" + priority + " since all preferred keys are assigned.";
                //                                    }
                //                                    //v-yiwzha not find available suggestkey
                //                                    if (i == accessKeys1[key].Name.Length - 1 && unpreferredList == string.Empty)
                //                                    {
                //                                        logMessage = "No available hotkey to sign, need to check manually";
                //                                    }
                //                                }
                //                            }
                //                            changedControlName = accessKeys1[key].Name;
                //                            changedControl = accessKeys1[key];
                //                        }
                //                    }
                //                    else if (accessKeys1[key].AutomationId != "_btnPrevious" && accessKeys1[key].AutomationId != "_btnNext" && accessKeys1[key].AutomationId != "_btnFinish" && accessKeys1[key].AutomationId != "_btnCancel" && accessKeys1[key].AutomationId != "okButton" && accessKeys1[key].AutomationId != "cancelButton" && accessKeys1[key].AutomationId != "applyButton")
                //                    {
                //                        if (accessKeys1[key].Name != null)
                //                        {
                //                            string unpreferredList = string.Empty;
                //                            for (int i = 0; i < accessKeys1[key].Name.Length; i++)
                //                            {
                //                                char c = accessKeys1[key].Name.ToLower()[i];
                //                                //v-yiwzha: filter the DBCS language with unicode
                //                                if ((c >= 0x4e00 && c <= 0x9fbf) || (c >= 0x1100 && c <= 0x11ff) || (c >= 0x3040 && c <= 0x309f) || (c >= 0x30a0 && c <= 0x30ff))
                //                                {
                //                                    suggestkey = '+';
                //                                    logMessage = string.Empty;
                //                                    break;
                //                                }
                //                                //v-yiwzha give suggestkey, make sure suggestkey are not in current page && is an englisth lettle && not in suggestkeylist
                //                                if (!accessKeys.ContainsKey(c) && !suggestkeylist.Contains(c) && !forbiddenkeylist.Contains(c) && !unpreferredkeylist.Contains(c))
                //                                {
                //                                    suggestkey = c;
                //                                    logMessage = string.Empty;
                //                                    break;
                //                                }
                //                                //v-yiwzha used not-prefered key to sign
                //                                if (!accessKeys.ContainsKey(c) && !suggestkeylist.Contains(c) && !forbiddenkeylist.Contains(c) && unpreferredkeylist.Contains(c))
                //                                {
                //                                    unpreferredList += c;
                //                                }
                //                                if (i == accessKeys1[key].Name.Length - 1 && unpreferredList != string.Empty)
                //                                {
                //                                    int priority = 0;
                //                                    for (int j = 0; j < unpreferredList.Length; j++)
                //                                    {
                //                                        if (unpreferredkeyp5list.Contains(unpreferredList[j]))
                //                                        {
                //                                            if (priority < 5)
                //                                            {
                //                                                priority = 5;
                //                                                suggestkey = unpreferredList[j];
                //                                            }
                //                                        }
                //                                        if (unpreferredkeyp4list.Contains(unpreferredList[j]))
                //                                        {
                //                                            if (priority < 4)
                //                                            {
                //                                                priority = 4;
                //                                                suggestkey = unpreferredList[j];
                //                                            }
                //                                        }
                //                                        if (unpreferredkeyp3list.Contains(unpreferredList[j]))
                //                                        {
                //                                            if (priority < 3)
                //                                            {
                //                                                priority = 3;
                //                                                suggestkey = unpreferredList[j];
                //                                            }
                //                                        }
                //                                        if (unpreferredkeyp2list.Contains(unpreferredList[j]))
                //                                        {
                //                                            if (priority < 2)
                //                                            {
                //                                                priority = 2;
                //                                                suggestkey = unpreferredList[j];
                //                                            }
                //                                        }
                //                                        if (unpreferredkeyp1list.Contains(unpreferredList[j]))
                //                                        {
                //                                            if (priority < 1)
                //                                            {
                //                                                priority = 1;
                //                                                suggestkey = unpreferredList[j];
                //                                            }
                //                                        }
                //                                    }
                //                                    logMessage = " The suggest key is unpreferred P" + priority + " since all preferred keys are assigned.";
                //                                }
                //                                //v-yiwzha not find available suggestkey
                //                                if (i == accessKeys1[key].Name.Length - 1 && unpreferredList == string.Empty)
                //                                {
                //                                    logMessage = "No available hotkey to sign, need to check manually";
                //                                }
                //                            }
                //                        }
                //                        changedControlName = accessKeys1[key].Name;
                //                        changedControl = accessKeys1[key];
                //                    }

                //                    if (changedControlName == "Both2CommonControls")
                //                    {
                //                        comments = " both 2 duplicate controls are Common Controls. ";
                //                    }
                //                    if (changedControlName == "" || changedControlName == null)
                //                    {
                //                        comments = " Could not find previous control or control name for both controls. ";
                //                    }
                //                    else if (control.Name == "" || control.Name == null)
                //                    {
                //                        comments = " Could not find previous control or control name for second control. ";
                //                    }
                //                    else if (accessKeys1[key].Name == "" || accessKeys1[key].Name == null)
                //                    {
                //                        comments = " Could not find previous control or control name for first control. ";
                //                    }
                //                    if (!ControlUnderRadioButtonGroups(accessKeys1[key], control))
                //                    {
                //                        if (logMessage != "No available hotkey to sign, need to check manually")
                //                        {
                //                            if (comments.Contains("both 2 duplicate controls are Common Controls."))
                //                            {
                //                                logMessage = "The following controls have the same access key '" + key + "': \"" + accessKeys1[key].Name + "\" \"" + control.Name + "\"." + " But" + comments + "No available controls could be assigned.";
                //                            }
                //                            else
                //                            {
                //                                if (suggestkey == '+')
                //                                {
                //                                    logMessage = "The following controls have the same access key '" + key + "': \"" + accessKeys1[key].Name + "\" \"" + control.Name + "\"." + comments + "\nBut the ui is DBCS language, same hotkey with ENU, please verify ENU language for more information.";
                //                                    rt = ResultType.Warning;
                //                                }
                //                                else
                //                                {
                //                                    for (int i = 0; i < suggestkeylist.Length; i++)
                //                                    {
                //                                        if (suggestaccessKeys[suggestkeylist[i]] == changedControl)
                //                                        {
                //                                            suggestkey = suggestkeylist[i];
                //                                        }
                //                                    }
                //                                    logMessage = "The following controls have the same access key '" + key + "': \"" + accessKeys1[key].Name + "\" \"" + control.Name + "\"." + comments + "\nSuggestion: Use '" + suggestkey + "' in \"" + changedControlName + "\"" + logMessage + "\nMore Control Infomation:\n" + " Following are all hotkeys in this page: " + hotkeylist + "\nHotkeys In Control:\n" + hotkeyControlInfo;
                //                                    //v-yiwzha add suggestkey to suggestkeylist
                //                                    suggestkeylist += suggestkey;
                //                                    if (!suggestaccessKeys.ContainsKey(suggestkey))
                //                                    {
                //                                        suggestaccessKeys.Add(suggestkey, changedControl);
                //                                    }
                //                                }
                //                            }
                //                        }
                //                        else
                //                        {
                //                            if (comments.Contains("Could not find previous control or control name for both controls."))
                //                            {
                //                                logMessage = "The following controls have the same access key '" + key + "': \"" + accessKeys1[key].Name + "\" \"" + control.Name + "\"." + comments + "\nBut in both controls. " + logMessage + ".\nMore Control Infomation:\n" + " Following are all hotkeys in this page: " + hotkeylist + "\nHotkeys In Control:\n" + hotkeyControlInfo;
                //                            }
                //                            logMessage = "The following controls have the same access key '" + key + "': \"" + accessKeys1[key].Name + "\" \"" + control.Name + "\"." + comments + "\nBut in control \"" + changedControlName + "\" " + logMessage + ".\nMore Control Infomation:\n" + " Following are all hotkeys in this page: " + hotkeylist + "\nHotkeys In Control:\n" + hotkeyControlInfo;
                //                        }
                //                        UIComplianceResultBase result = new UIComplianceResultBase(rt, logMessage, Name);
                //                        result.AddRelatedControls(accessKeys1[key]);
                //                        result.AddRelatedControls(control);
                //                        results.Add(result);
                //                    }
                //                }
                //                else
                //                {
                //                    accessKeys1.Add(key, control);
                //                    if (accessKeys1[key].Name == null || accessKeys1[key].Name == "")
                //                    {
                //                        ElementInformation lable = CheckPreviousIndexControl(accessKeys1[key]);
                //                        if (lable != null)
                //                        {
                //                            if (lable.Name != null)
                //                            {
                //                                accessKeys1[key].Name = lable.Name;
                //                            }
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                #endregion

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

                MinControl.Sort(delegate (ElementInformation a, ElementInformation b)
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

                MinControl.Sort(delegate (ElementInformation a, ElementInformation b)
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

        private AccessKeyDuplicateExceptRuleProperties ruleProperties;

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }

    public class AccessKeyDuplicateExceptRuleProperties
    {
        public List<char> ForbiddenKeys
        {
            get { return forbiddenKeys; }
            set { forbiddenKeys = value; }
        }
        private List<char> forbiddenKeys = new List<char>();

        public bool WarnNoAccessKey
        {
            get { return warnNoAccessKey; }
            set { warnNoAccessKey = value; }
        }
        private bool warnNoAccessKey = false;
    }
}

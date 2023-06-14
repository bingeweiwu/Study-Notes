using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Windows.Automation;
using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml.Serialization;
using UIA3 = interop.UIAutomationCore;
using MS.Internal.SulpHur.Utilities;

namespace MS.Internal.SulpHur.UICompliance
{
    [DataContract]
    public class ElementInformation
    {
        public ElementInformation() { }

        /// <summary>
        /// remove the invalid char from string
        /// </summary>
        /// <param name="tmp">the string you want to change</param>
        /// <returns></returns>
        public static string ReplaceLowOrderASCIICharacters(string tmp)
        {
            StringBuilder info = new StringBuilder();
            foreach (char cc in tmp)
            {
                int ss = (int)cc;
                if (((ss >= 0) && (ss < 8)) || ((ss >= 11) && (ss <= 12)) || ((ss >= 14) && (ss < 32)))
                    continue;
                else info.Append(cc);
            }
            return info.ToString();
        }

        public ElementInformation(UIA3.IUIAutomationElement uia3Ae)
        {
            this.AcceleratorKey = uia3Ae.CurrentAcceleratorKey;
            if (string.IsNullOrEmpty(uia3Ae.CurrentAccessKey))
            {
                this.AccessKey = string.Empty;
            }
            else
            {
                this.AccessKey = ReplaceLowOrderASCIICharacters(uia3Ae.CurrentAccessKey);
            }
            this.AutomationId = uia3Ae.CurrentAutomationId;
            this.BoundingRectangle = uia3Ae.CurrentBoundingRectangle.ToWinRect();
            this.ClassName = uia3Ae.CurrentClassName;
            this.ControlType = ParseControlType(uia3Ae);
            this.FrameworkId = uia3Ae.CurrentFrameworkId;
            this.HasKeyboardFocus = uia3Ae.CurrentHasKeyboardFocus.ToBool();
            this.HelpText = uia3Ae.CurrentHelpText;
            this.IsContentElement = uia3Ae.CurrentIsContentElement.ToBool();
            this.IsControlElement = uia3Ae.CurrentIsControlElement.ToBool();
            this.IsEnabled = uia3Ae.CurrentIsEnabled.ToBool();
            this.IsKeyboardFocusable = uia3Ae.CurrentIsKeyboardFocusable.ToBool();
            this.IsOffscreen = uia3Ae.CurrentIsOffscreen.ToBool();
            this.IsPassword = uia3Ae.CurrentIsPassword.ToBool();
            this.IsRequiredForForm = uia3Ae.CurrentIsRequiredForForm.ToBool();
            this.ItemStatus = uia3Ae.CurrentItemStatus;
            this.ItemType = uia3Ae.CurrentItemType;
            this.LocalizedControlType = uia3Ae.CurrentLocalizedControlType;
            this.Name = uia3Ae.CurrentName;
            this.NativeWindowHandle = uia3Ae.CurrentNativeWindowHandle.ToInt32();
            this.Orientation = uia3Ae.CurrentOrientation.ToUia2OrientationType();
            this.ProcessId = uia3Ae.CurrentProcessId;

            if (this.ControlType == ControlType.Button && exemptedbutton.Contains(this.Name))
            {
                this.IsDefaultShortcutButton = true;
            }
            else
            {
                this.IsDefaultShortcutButton = false;
            }

            if (needNativePropertyTypes.Contains(this.ControlType))
            {
                SetMeasureTestAttributes(uia3Ae);
            }

            this.IsManagedControlProperty = false;
            #region set managedAttributes
            if (this.FrameworkId.IndexOf("wpf", StringComparison.InvariantCultureIgnoreCase) != -1
                || this.FrameworkId.IndexOf("winform", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                this.IsManagedControlProperty = true;
                try
                {
                    IntPtr handle = uia3Ae.CurrentNativeWindowHandle;
                    this.TabIndex = (int)Native.GetProperty(handle, "TabIndex");
                }
                catch
                {

                }

                try
                {
                    if (ControlType.Button == this.ControlType)
                    {
                        IntPtr handle = uia3Ae.CurrentNativeWindowHandle;
                        Image img = (Image)Native.GetProperty(handle, "Image");
                        if (img != null)
                        {
                            this.IsImageButton = true;
                        }
                        else
                        {
                            this.IsImageButton = false;
                        }
                    }
                }
                catch
                {
                    this.IsImageButton = false;
                }

                try
                {
                    IntPtr handle = uia3Ae.CurrentNativeWindowHandle;
                    object objTabStop = Native.GetProperty(handle, "TabStop");
                    if (objTabStop == null)
                    {
                        this.TabStop = true;
                    }
                    else
                    {
                        this.TabStop = (bool)objTabStop;
                    }
                }
                catch
                {

                }

                try
                {
                    IntPtr handle = uia3Ae.CurrentNativeWindowHandle;
                    this.IsReadOnly = (bool)Native.GetProperty(handle, "ReadOnly");
                }
                catch
                {

                }
            }
            #endregion
        }

        List<string> exemptedbutton = new List<string>() { Resources.Cancel.Parsed(), Resources.OK.Parsed(), Resources.Help.Parsed() };

        #region public attributes

        [DataMember]
        public string AcceleratorKey { get; set; }
        [DataMember]
        public string AccessKey { get; set; }
        [DataMember]
        public string AutomationId { get; set; }
        [DataMember]
        public System.Windows.Rect BoundingRectangle { get; set; }

        public int X
        {
            get
            {
                return (int)BoundingRectangle.X;
            }
        }

        public int Y
        {
            get
            {
                return (int)BoundingRectangle.Y;
            }
        }

        public int Top
        {
            get
            {
                return (int)BoundingRectangle.Top;
            }
        }

        public int Bottom
        {
            get
            {
                return (int)BoundingRectangle.Bottom;
            }
        }

        public int Width
        {
            get
            {
                return (int)BoundingRectangle.Width;
            }
        }

        public int Height
        {
            get
            {
                return (int)BoundingRectangle.Height;
            }
        }

        public int Left
        {
            get
            {
                return (int)BoundingRectangle.Left;
            }
        }

        public int Right
        {
            get
            {
                return (int)BoundingRectangle.Right;
            }
        }

        [DataMember]
        public string ClassName { get; set; }
        [DataMember]
        public ControlType ControlType { get; set; }
        [DataMember]
        public string FrameworkId { get; set; }
        [DataMember]
        public bool HasKeyboardFocus { get; set; }
        [DataMember]
        public string HelpText { get; set; }
        [DataMember]
        public bool IsContentElement { get; set; }
        [DataMember]
        public bool IsControlElement { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public bool IsKeyboardFocusable { get; set; }
        [DataMember]
        public bool IsOffscreen { get; set; }
        [DataMember]
        public bool IsPassword { get; set; }
        [DataMember]
        public bool IsRequiredForForm { get; set; }
        [DataMember]
        public string ItemStatus { get; set; }
        [DataMember]
        public string ItemType { get; set; }

        [DataMember]
        public bool IsDefaultShortcutButton { get; set; }

        [DataMember]
        public int TabIndex
        {
            get;
            set;
        }

        [DataMember]
        public bool TabStop
        {
            get;
            set;
        }

        [DataMember]
        public bool IsImageButton
        {
            get;
            set;
        }

        [DataMember]
        public bool IsReadOnly
        {
            get;
            set;
        }

        [DataMember]
        public bool IsManagedControlProperty
        {
            get;
            set;
        }

        [DataMember]
        public string LocalizedControlType { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int NativeWindowHandle { get; set; }
        [DataMember]
        public OrientationType Orientation { get; set; }
        [DataMember]
        public int ProcessId { get; set; }

        #endregion

        #region Measure text

        [DataMember]
        public SulpHurSize ProposeSize { get; set; }

        [DataMember]
        public SulpHurSize TextUnit { get; set; }

        [DataMember]
        public SulpHurSize TextSize1 { get; set; }

        [DataMember]
        public SulpHurSize TextSize2 { get; set; }

        //private static System.Windows.Forms.Padding managePushButtonDefaultMargin = new System.Windows.Forms.Padding(8, 5, 6, 0);
        //private static System.Windows.Forms.Padding manageLabelDefaultMargin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        //private static System.Windows.Forms.Padding manageRadioDefaultMargin = new System.Windows.Forms.Padding(17, 2, 8, 0);
        //private static System.Windows.Forms.Padding manageCheckDefaultMargin = new System.Windows.Forms.Padding(17, 2, 8, 0);
        private static System.Windows.Forms.Padding ZeroMargin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        #endregion

        #region Collection and Navigation APIs

        [DataMember]
        public List<ElementInformation> Children { get; set; }

        public List<ElementInformation> Siblings { get; set; }

        public List<ElementInformation> Ancestors { get; set; }

        public List<ElementInformation> Descendants { get; set; }

        public ElementInformation FirstChild { get; set; }

        public ElementInformation LastChild { get; set; }

        public ElementInformation NextSibling { get; set; }

        public ElementInformation PreviousSibling { get; set; }

        public ElementInformation Parent { get; set; }

        public int treeLevel;

        #endregion

        #region private and protected method
        protected bool IsManagedControl(UIA3.IUIAutomationElement window)
        {
            if (window.CurrentFrameworkId.IndexOf("wpf", StringComparison.InvariantCultureIgnoreCase) != -1
                || window.CurrentFrameworkId.IndexOf("winform", StringComparison.InvariantCultureIgnoreCase) != -1)
                return true;
            return false;
        }

        private ControlType[] needNativePropertyTypes = { ControlType.Button, ControlType.Text, ControlType.RadioButton, ControlType.CheckBox, ControlType.Group };
        protected ControlType ParseControlType(UIA3.IUIAutomationElement control)
        {
            ControlType retControlType = ControlType.Unknown;

            try
            {
                int currentControlType = control.CurrentControlType;
                switch (currentControlType)
                {
                    case UIA3.UIA_ControlTypeIds.UIA_ButtonControlTypeId:
                        retControlType = ControlType.Button;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_CalendarControlTypeId:
                        retControlType = ControlType.Calendar;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_CheckBoxControlTypeId:
                        retControlType = ControlType.CheckBox;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_ComboBoxControlTypeId:
                        if ((bool)control.GetCurrentPropertyValue(UIA3.UIA_PropertyIds.UIA_IsRangeValuePatternAvailablePropertyId))
                            retControlType = ControlType.NumericUpDown;
                        else
                            retControlType = ControlType.ComboBox;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_DataGridControlTypeId:
                        retControlType = ControlType.DataGrid;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_DocumentControlTypeId:
                        retControlType = ControlType.Document;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_EditControlTypeId:
                        retControlType = ControlType.Edit;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_GroupControlTypeId:
                        retControlType = ControlType.Group;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_HeaderItemControlTypeId:
                        retControlType = ControlType.HeaderItem;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_HyperlinkControlTypeId:
                        retControlType = ControlType.Hyperlink;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_ImageControlTypeId:
                        retControlType = ControlType.Image;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_ListControlTypeId:
                        retControlType = ControlType.List;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_ListItemControlTypeId:
                        retControlType = ControlType.ListItem;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_MenuControlTypeId:
                        retControlType = ControlType.Menu;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_MenuBarControlTypeId:
                        retControlType = ControlType.MenuBar;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_MenuItemControlTypeId:
                        retControlType = ControlType.MenuItem;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_PaneControlTypeId:
                        retControlType = ControlType.Pane;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_ProgressBarControlTypeId:
                        retControlType = ControlType.ProgressBar;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_RadioButtonControlTypeId:
                        retControlType = ControlType.RadioButton;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_ScrollBarControlTypeId:
                        retControlType = ControlType.ScrollBar;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_SeparatorControlTypeId:
                        retControlType = ControlType.Separator;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_SliderControlTypeId:
                        retControlType = ControlType.Slider;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_SpinnerControlTypeId:
                        retControlType = ControlType.Spinner;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_SplitButtonControlTypeId:
                        retControlType = ControlType.SplitButton;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_StatusBarControlTypeId:
                        retControlType = ControlType.StatusBar;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_TabControlTypeId:
                        retControlType = ControlType.Tab;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_TabItemControlTypeId:
                        retControlType = ControlType.TabItem;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_TableControlTypeId:
                        retControlType = ControlType.Table;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_TextControlTypeId:
                        retControlType = ControlType.Text;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_ThumbControlTypeId:
                        retControlType = ControlType.Thumb;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_TitleBarControlTypeId:
                        retControlType = ControlType.TitleBar;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_ToolBarControlTypeId:
                        retControlType = ControlType.ToolBar;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_ToolTipControlTypeId:
                        retControlType = ControlType.ToolTip;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_TreeControlTypeId:
                        retControlType = ControlType.Tree;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_TreeItemControlTypeId:
                        retControlType = ControlType.TreeItem;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_WindowControlTypeId:
                        retControlType = ControlType.Window;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_HeaderControlTypeId:
                        retControlType = ControlType.Header;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_DataItemControlTypeId:
                        retControlType = ControlType.DataItem;
                        break;
                    case UIA3.UIA_ControlTypeIds.UIA_CustomControlTypeId:
                        retControlType = ControlType.Custom;
                        break;
                    default:
                        retControlType = ControlType.Unknown;
                        break;
                }
            }
            catch
            {
                retControlType = ControlType.Unknown;
            }

            // Unkown : Not finished, Add more controls here
            return retControlType;
        }

        protected string GetRidof(string text)
        {
            StringBuilder sb = new StringBuilder();
            bool has = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\r' || text[i] == '\n') continue;
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

        protected string Generate30String(string text)
        {
            float errorLevel = 0.25f;
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
        private void SetMeasureTestAttributes(UIA3.IUIAutomationElement uia3Ae)
        {
            try
            {
                IntPtr handle = (IntPtr)this.NativeWindowHandle;
                Graphics graph = Graphics.FromHwnd(handle);
                if (graph == null) return;
                graph.PageUnit = GraphicsUnit.Pixel;

                TextFormatFlags flag = TextFormatFlags.Default | TextFormatFlags.WordBreak | TextFormatFlags.HidePrefix;

                Font font = null;
                System.Windows.Forms.Padding margin = ZeroMargin;

                if (IsManagedControl(uia3Ae))
                {
                    //object obj = Native.GetProperty(handle, "AutoSize");
                    //if (obj != null)
                    //{
                    //    try
                    //    {
                    //        if ((bool)obj)
                    //        {
                    //return;
                    //        }
                    //    }
                    //    catch (InvalidCastException)
                    //    {
                    //        return;
                    //    }
                    //}
                    font = (Font)Native.GetProperty(handle, "Font");
                    if (font == null)
                        return;

                    //margin = (System.Windows.Forms.Padding)Native.GetProperty(handle, "Padding");
                    //switch (this.ControlType)
                    //{
                    //    // Managed Controls
                    //    case ControlType.Button:
                    //        margin = managePushButtonDefaultMargin;
                    //        break;
                    //    case ControlType.Text:
                    //        margin = ZeroMargin;
                    //        break;
                    //    case ControlType.RadioButton:
                    //        margin = manageRadioDefaultMargin;
                    //        break;
                    //    case ControlType.CheckBox:
                    //        margin = manageCheckDefaultMargin;
                    //        break;
                    //}
                }
                else
                {
                    IntPtr hFont = Native.SendMessage(handle, Native.WM_GETFONT, (IntPtr)null, (IntPtr)null);
                    font = Font.FromHfont(hFont);
                    if (font == null) return;

                    //margin = (System.Windows.Forms.Padding)Native.GetProperty(handle, "Padding");
                }
                margin = (System.Windows.Forms.Padding)Native.GetProperty(handle, "Padding");

                System.Drawing.Size proposeSize = new System.Drawing.Size((int)(this.BoundingRectangle.Width - (margin.Left + margin.Right)), (int)(this.BoundingRectangle.Height - (margin.Top + margin.Bottom)));

                this.ProposeSize = new SulpHurSize(proposeSize);
                this.TextUnit = new SulpHurSize(TextRenderer.MeasureText(graph, "H", font, proposeSize, flag));

                int mostH = 0;
                for (char a = 'A'; a < 'Z'; a++)
                {
                    System.Drawing.Size unit = TextRenderer.MeasureText(graph, a.ToString(), font, proposeSize, flag);
                    if (unit.Height > mostH)
                        mostH = unit.Height;
                }

                string text = GetRidof(this.Name);
                this.TextSize1 = new SulpHurSize(TextRenderer.MeasureText(graph, text, font, proposeSize, flag));

                //text not contains '\n'
                text = Generate30String(text);
                this.TextSize2 = new SulpHurSize(TextRenderer.MeasureText(graph, text, font, proposeSize, flag));
            }
            catch (Exception e)
            {
                Trace.WriteLine("Fail to set MeasureText.Size:(example:ProposeSize)" + e.Message);
            }
        }
        #endregion
    }
}

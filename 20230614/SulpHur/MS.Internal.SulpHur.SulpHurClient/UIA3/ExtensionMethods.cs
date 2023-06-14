using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using interop.UIAutomationCore;
using System.Windows.Automation;
using System.Diagnostics;

namespace MS.Internal.SulpHur.SulpHurClient.UIA3
{
    public static class ExtensionMethods
    {
        //public static IUIAutomationElement ToUIA3Element(this AutomationElement uia2Element)
        //{
        //    IUIAutomationElement uia3Element = null;
        //    IUIAutomation automation = null;
        //    try
        //    {
        //        automation = new CUIAutomation();
        //        uia3Element = automation.ElementFromHandle(new IntPtr(uia2Element.Current.NativeWindowHandle));
        //    }
        //    catch (Exception ex)
        //    {
        //        //write log
        //        Trace.WriteLine(ex);
        //    }

        //    return uia3Element;
        //}

        //public static AutomationElement ToUIA2Element(this IUIAutomationElement uia3Element)
        //{
        //    AutomationElement uia2Element = null;
        //    try
        //    {
        //        uia2Element = AutomationElement.FromHandle(uia3Element.CurrentNativeWindowHandle);
        //    }
        //    catch (Exception ex)
        //    {
        //        //write log
        //        Trace.WriteLine(ex);
        //    }

        //    return uia2Element;
        //}
        //操作UIAutomationElement
        public static IUIAutomationElement GetParent(this IUIAutomationElement uia3Element)
        {
            IUIAutomationElement parent = null;
            try
            {
                IUIAutomation automation = new CUIAutomation();
                IUIAutomationTreeWalker treeWalker = automation.RawViewWalker;
                parent = treeWalker.GetParentElement(uia3Element);

                return parent;
            }
            catch
            {
                return null;
            }
        }
        public static bool TryGetElementByAutomationId(this IUIAutomationElement root, string path, out IUIAutomationElement aeResult)
        {
            aeResult = null;
            try
            {
                aeResult = root.GetElementByAutomationId(path);
                if (aeResult == null)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
        public static IUIAutomationElement GetElementByAutomationId(this IUIAutomationElement root, string path)
        {
            string[] nodeArr = path.Split('\\');
            IUIAutomationElement aeChild = root;
            foreach (string node in nodeArr)
            {
                if (!string.IsNullOrEmpty(node))
                {
                    aeChild = aeChild.FindFirst(interop.UIAutomationCore.TreeScope.TreeScope_Children, UIA3Automation.RawInstance.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, node));
                }
            }

            return aeChild.Equals(root) ? null : aeChild;
        }
        public static bool TryGetElementByClassName(this IUIAutomationElement root, string path, out IUIAutomationElement aeResult)
        {
            aeResult = null;
            try
            {
                aeResult = root.GetElementByClassName(path);
                if (aeResult == null)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
        public static IUIAutomationElement GetElementByClassName(this IUIAutomationElement root, string path)
        {
            string[] nodeArr = path.Split('\\');

            IUIAutomationElement aeChild = root;
            foreach (string node in nodeArr)
            {
                if (!string.IsNullOrEmpty(node))
                {
                    aeChild = aeChild.FindFirst(interop.UIAutomationCore.TreeScope.TreeScope_Children, UIA3Automation.RawInstance.CreatePropertyCondition(UIA_PropertyIds.UIA_ClassNamePropertyId, node));
                }
            }

            return aeChild;
        }
        public static IUIAutomationElementArray FindAllChildren(this IUIAutomationElement parent)
        {
            return parent.FindAll(interop.UIAutomationCore.TreeScope.TreeScope_Children, UIA3Automation.RawInstance.CreateTrueCondition());
        }
        public static IUIAutomationElement FirstChild(this IUIAutomationElement parent)
        {
            return parent.FindFirst(interop.UIAutomationCore.TreeScope.TreeScope_Children, UIA3Automation.RawInstance.CreateTrueCondition());
        }
        public static IUIAutomationElement FirstChild(this IUIAutomationElement parent, int depth)
        {
            IUIAutomationElement firstChild = parent;
            for (int i = 0; i < depth; i++)
            {
                firstChild = firstChild.FindFirst(interop.UIAutomationCore.TreeScope.TreeScope_Children, UIA3Automation.RawInstance.CreateTrueCondition());
            }
            return firstChild;
        }
        public static IUIAutomationElement GetChild(this IUIAutomationElement parent, int index)
        {
            return parent.FindAllChildren().GetElement(index);
        }
        public static IUIAutomationElement GetChild(this IUIAutomationElement parent, string automationId)
        {
            return parent.FindFirst(interop.UIAutomationCore.TreeScope.TreeScope_Children, UIA3Automation.RawInstance.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, automationId));
        }
    }
}

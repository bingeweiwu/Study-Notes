using interop.UIAutomationCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MS.Internal.SulpHur.SulpHurClient;
using MS.Internal.SulpHur.SulpHurClient.UIA3;
using MS.Internal.SulpHur.UICompliance;
using MS.Internal.SulpHur.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace UnitTest.SulphurClient
{
    [TestClass]
    public class GenerateXMLTest
    {
        //private static log4net.ILog log;

        //[TestInitialize]
        //public void TestInitialize()
        //{
        //    MyLog.Init();
        //    log = log4net.LogManager.GetLogger("HelloLog4");
        //    log.Debug("Hello log4");
        //}

        [TestMethod]
        public void GenXml()
        {
            Thread.Sleep(3000);
            //top page
            IntPtr hWnd = NativeMethods.GetForegroundWindow();

            //根节点
            IUIAutomationElement root = UIA3Automation.RawInstance.ElementFromHandle(hWnd);

            //所有一层子节点
            IUIAutomationElementArray uia3ObjectArray = root.FindAllChildren();

            for (int i = 0; i < uia3ObjectArray.Length; i++)
            {
                if (i!=1)
                {
                    continue;
                }

                IUIAutomationElement uia3Ae = uia3ObjectArray.GetElement(i);
                tagRECT rect = uia3Ae.CurrentBoundingRectangle;
                string realText = GetText(AutomationElement.FromPoint(new System.Windows.Point(rect.left + 1, rect.top + 1)));
                //log.Debug($"location:{rect.left}|{rect.right}|{rect.top}|{rect.bottom}");
                string s = getStringOnly((rect.left + rect.right) / 2, (rect.top + rect.bottom) / 2);
                //log.Debug(s);

                //log.Debug(getStringOnly(rect.left+1, rect.top+1));
            }
        }


        public static string getStringOnly(int x,int y)
        {
            string sourceText = string.Empty;

            AutomationElement ae = AutomationElement.FromPoint(new System.Windows.Point(x, y));
            AutomationElement.AutomationElementInformation current = ae.Current;
            string res = current.Name;

            AutomationElement.AutomationElementInformation cached = ae.Cached;

            FieldInfo[] Fields = typeof(AutomationElement).GetFields();
            foreach (var item in Fields)
            {
                //log.Debug($"{item.Name}==> {Newtonsoft.Json.JsonConvert.SerializeObject(item.GetValue(ae))}");
            }

            //log.Debug(current.GetType().GetProperty("Name").GetValue(current, null).ToString());
            return res;
        }

        public static string GetText(AutomationElement element)
        {
            object patternObj;
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
            {
                var valuePattern = (ValuePattern)patternObj;
                return valuePattern.Current.Value;
            }
            else if (element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
            {
                var textPattern = (TextPattern)patternObj;
                return textPattern.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
            }
            else
            {
                return element.Current.Name;
            }
        }
    }
}

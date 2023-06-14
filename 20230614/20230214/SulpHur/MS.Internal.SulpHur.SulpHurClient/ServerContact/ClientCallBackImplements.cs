using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MS.Internal.SulpHur.Utilities;
using MS.Internal.SulpHur.UICompliance;
using System.Diagnostics;
using MS.Internal.SulpHur.SulpHurClient.Monitors;
using interop.UIAutomationCore;
using MS.Internal.SulpHur.SulpHurClient.UIA3;

namespace MS.Internal.SulpHur.SulpHurClient.ServerContact
{
    public class ClientCallBackImplements : ISulpHurWCFServiceCallback
    {
        public UICompliance.ForegroundData getForegroundData()
        {
            UICompliance.ForegroundData data = new UICompliance.ForegroundData();
            #region get page assembly and full type name
            IUIAutomationElement objPageControl = null;
            IntPtr handle = NativeMethods.GetForegroundWindow();
            IUIAutomationElement foregroundWindowElement = UIA3Automation.RawInstance.ElementFromHandle(handle);

            if (!Utility.IsProductProcess(foregroundWindowElement)) return new ForegroundData();
            ElementChangeMonitor elementChangeMonitor = new ElementChangeMonitor(foregroundWindowElement);
            ElementInformation eiTree = Utility.ReadTree(foregroundWindowElement.CurrentNativeWindowHandle, elementChangeMonitor);
            data.Ei = eiTree;

            if (Utility.TryGetPageControl(foregroundWindowElement, out objPageControl))
            {
                if (objPageControl != null)
                {
                    string assemblyAndTypeName = string.Empty;
                    IntPtr hPageControl = objPageControl.CurrentNativeWindowHandle;
                    if (objPageControl.CurrentFrameworkId.Equals("WPF", StringComparison.OrdinalIgnoreCase))
                    {
                        assemblyAndTypeName = ManagedInjectorWrapper.Injector.GetWPFType(hPageControl);
                    }
                    else
                    {
                        assemblyAndTypeName = ManagedInjectorWrapper.Injector.GetWinFormType(hPageControl);
                    }

                    if (!string.IsNullOrEmpty(assemblyAndTypeName))
                    {
                        string[] splitArr = assemblyAndTypeName.Split('$');
                        data.AssemblyName = splitArr[0];
                        data.AssemblyType = splitArr[1];
                    }
                }
            }
            #endregion

            Bitmap screenshot = ImageCapturer.TakeImage(foregroundWindowElement.CurrentNativeWindowHandle);
            data.Image = screenshot;
            return data;
        }


        //private bool IsCMDialog(AutomationElement root)
        //{
        //    int processID;
        //    string className;
        //    Process process = null;
        //    try
        //    {
        //        processID = root.Current.ProcessId;
        //        className = root.Current.ClassName;
        //        process = Process.GetProcessById(processID);

        //        //#32770 for ignore system dialog
        //        if (process.ProcessName.Equals("Microsoft.ConfigurationManagement", StringComparison.OrdinalIgnoreCase)
        //            && className != "#32770")
        //        {
        //            return true;
        //        }

        //        return false;
        //    }
        //    catch (ElementNotAvailableException)
        //    {
        //        return false;
        //    }
        //    catch (ArgumentException)
        //    {
        //        //Trace.WriteLine("Process is unavailable now.");
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Trace.WriteLine(string.Format("Unexpected Exception: {0}", ex));
        //        return false;
        //    }
        //}

        public IAsyncResult BegingetForegroundData(AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public UICompliance.ForegroundData EndgetForegroundData(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public UICompliance.CapturedData capturedData()
        {
            UICompliance.CapturedData data = new UICompliance.CapturedData();
            IntPtr handle = NativeMethods.GetForegroundWindow();
            IUIAutomationElement foregroundWindowElement = UIA3Automation.RawInstance.ElementFromHandle(handle);
            if (!Utility.IsProductProcess(foregroundWindowElement)) return new CapturedData();
            ElementChangeMonitor elementChangeMonitor = new ElementChangeMonitor(foregroundWindowElement);
            ElementInformation eiTree = Utility.ReadTree(foregroundWindowElement.CurrentNativeWindowHandle, elementChangeMonitor);
            data.Ei = eiTree;
            Bitmap screenshot = ImageCapturer.TakeImage(foregroundWindowElement.CurrentNativeWindowHandle);
            data.Image = screenshot;
            return data;
        }

        public IAsyncResult BegincapturedData(AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public UICompliance.CapturedData EndcapturedData(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
    }
}

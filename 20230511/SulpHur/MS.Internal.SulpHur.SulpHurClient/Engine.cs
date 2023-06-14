using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.Utilities;
using MS.Internal.SulpHur.UICompliance;
using System.Windows.Automation;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using MS.Internal.SulpHur.Utilities.Exceptions;
using System.Windows;
using MS.Internal.SulpHur.SulpHurClient.Monitors;
using MS.Internal.SulpHur.SulpHurClient.UIA3;
using interop.UIAutomationCore;

namespace MS.Internal.SulpHur.SulpHurClient
{
    public class Engine
    {
        private static Engine instance;
        private UIA3AutomationEventHandler windowOpenedHandler = null;
        private UIA3AutomationEventHandler windowClosedHandler = null;
        private UIA3StructureChangedEventHandler structureChangedHandler = null;
        private Dictionary<string, IUIAutomationElement> openedWindowDic = new Dictionary<string, IUIAutomationElement>();
        private Dictionary<string, IUIAutomationElement> openedWindowDic_Off = new Dictionary<string, IUIAutomationElement>();
        public event EventHandler CaptureBegin;
        public event EventHandler<CaptureEndEventArgs> CaptureEnd;
        public event EventHandler CaptureError;
        //public event EventHandler WindowOpened;
        public event EventHandler WindowClosed;


        //IsStarted
        private bool isStarted = false;
        public bool IsStarted
        {
            get
            {
                return isStarted;
            }
        }

        public static Engine Instance
        {
            get
            {
                if (instance == null)
                    instance = new Engine();

                return instance;
            }
        }
        private Engine()
        {
            //start to send data
            App app = App.Current as App;
            DataCenter.Instance.SettingView = app.SettingView;
            //only send local data when client is connected with server
            if (ServerContact.ServerContacter.Instance.IsConnected)
            {
                DataCenter.Instance.StartToLoadLocalData();
            }
            DataCenter.Instance.StartToSendData();
        }


        public void Start()
        {

            #region fix the client doesn't capture after it turn off while the dialog is opened
            if (openedWindowDic_Off.Count > 0)
            {
                Trace.WriteLine("Check opened WindowDic");
                foreach (string key in openedWindowDic_Off.Keys)
                {
                    try
                    {
                        if (Utility.IsProductProcess(openedWindowDic_Off[key]))
                        {
                            Trace.WriteLine("Check True");
                            structureChangedHandler = new UIA3StructureChangedEventHandler(OnStructureChanged);
                            UIA3Automation.AddStructureChangedEventHandler(openedWindowDic_Off[key],
                                                           interop.UIAutomationCore.TreeScope.TreeScope_Subtree,
                                                           structureChangedHandler);
                            openedWindowDic.Add(key, openedWindowDic_Off[key]);
                        }
                    }
                    catch
                    {
                        Trace.WriteLine("The Thread is not available ");
                    }
                }
            }
            openedWindowDic_Off.Clear();
            #endregion

            EventFilter.Instance.FilteredStructureChangedEvent += new FilteredEventHandler(OnFilteredStructureChanged);
            //window opened event
            windowOpenedHandler = new UIA3AutomationEventHandler(OnWindowOpened);
            UIA3Automation.AddAutomationEventHandler(UIA_EventIds.UIA_Window_WindowOpenedEventId,
                                                     UIA3Automation.RawInstance.GetRootElement(),
                                                     interop.UIAutomationCore.TreeScope.TreeScope_Subtree,
                                                     windowOpenedHandler);
            //window closed event
            windowClosedHandler = new UIA3AutomationEventHandler(OnWindowClosed);
            UIA3Automation.AddAutomationEventHandler(UIA_EventIds.UIA_Window_WindowClosedEventId,
                                                        UIA3Automation.RawInstance.GetRootElement(),
                                                        interop.UIAutomationCore.TreeScope.TreeScope_Subtree,
                                                        windowClosedHandler);
            //set flag
            this.isStarted = true;
        }
        public void Stop()
        {
            try
            {
                //remove window opened event
                UIA3Automation.RemoveAutomationEventHandler(UIA_EventIds.UIA_Window_WindowOpenedEventId, UIA3Automation.RawInstance.GetRootElement(), this.windowOpenedHandler);
                //remove window closed event
                UIA3Automation.RemoveAutomationEventHandler(UIA_EventIds.UIA_Window_WindowClosedEventId, UIA3Automation.RawInstance.GetRootElement(), this.windowClosedHandler);

                //remove all structure changed event listener for opened window
                Trace.WriteLine(string.Format("openedWindowDic.Count: {0}", openedWindowDic.Count));

                foreach (string key in openedWindowDic.Keys)
                {
                    UIA3Automation.RemoveStructureChangedEventHandler(openedWindowDic[key], structureChangedHandler);
                    openedWindowDic_Off.Add(key, openedWindowDic[key]);
                }
                openedWindowDic.Clear();
                //set flag
                this.isStarted = false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private void OnWindowOpened(IUIAutomationElement element, UIA3AutomationEventArgs e)
        {
            if (element == null)
            {
                return;
            }
            try
            {
                // validate
                if (!Utility.ValidateWindowOpenedEvent(element))
                    return;
                string strRuntimeId = element.GetRuntimeId().ToRuntimeIdString();
                // miss processed window
                if (openedWindowDic.ContainsKey(strRuntimeId))
                    return;
                // cache processing window
                openedWindowDic.Add(strRuntimeId, element);
                // log
                Trace.WriteLine(string.Format("OnWindowOpened: {0}", strRuntimeId));
                // capture
                this.CaptureUI();

                // monitor structure changed event of the window
                structureChangedHandler = new UIA3StructureChangedEventHandler(OnStructureChanged);
                UIA3Automation.AddStructureChangedEventHandler(element,
                                                               interop.UIAutomationCore.TreeScope.TreeScope_Subtree,
                                                               structureChangedHandler);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        private void OnWindowClosed(IUIAutomationElement element, UIA3AutomationEventArgs e)
        {
            string strRuntimeId = element.GetRuntimeId().ToRuntimeIdString();
            if (openedWindowDic.ContainsKey(strRuntimeId))
            {
                Trace.WriteLine(string.Format("OnWindowClosed.runtimeId: {0}", strRuntimeId));
                UIA3Automation.RemoveStructureChangedEventHandler(element, structureChangedHandler);
                openedWindowDic.Remove(strRuntimeId);
                if (this.WindowClosed != null)
                {
                    this.WindowClosed(this, null);
                }
            }
        }
        private void OnStructureChanged(IUIAutomationElement element, UIA3StructureChangedEventArgs e)
        {
            if (element == null)
                return;

            try
            {
                //validate
                if (!Utility.ValidateStructureChangedEvent(element))
                    return;

                //try trigger
                EventFilter.Instance.TryTriggerStructureChangedEvent();
            }
            catch (COMException)
            {
                Trace.WriteLine("COMException when processing OnStructureChanged.");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        private void OnFilteredStructureChanged()
        {
            this.CaptureUI();
        }


        public void CaptureUI()
        {
            //begin event
            if (this.CaptureBegin != null)
            {
                UploadStateManager.Instance.UpdateState(UploadState.Processing, null);
                this.CaptureBegin(this, null);
            }

            Trace.WriteLine("Start CaptureUI >>>>>>>>>>>>>>>>>>>>>>");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                IUIAutomationElement objWndElement = null;
                try
                {
                    //start global change monitor
                    GlobalChangeMonitor.Instance.Reset();

                    //get foreground window
                    IntPtr hWnd = NativeMethods.GetForegroundWindow();
                    objWndElement = UIA3Automation.RawInstance.ElementFromHandle(hWnd);
                    //miss unrelated window and CM console and windows dialog (such as, open file dialog, save as dialog...)
                    if (!Utility.IsValidWindow(objWndElement))
                    {
                        if (!(App.UIAutoFlag && App.UIAutoMainInterfaceCaptureFlag) || Utility.IsSulphurClientProcess(objWndElement))
                        {
                            //no need to capture
                            Trace.WriteLine("Ignore window which is not meet the requirement.");
                            if (this.CaptureEnd != null)
                            {
                                UploadStateManager.Instance.UpdateState(UploadState.Abandon, "Ignore window which is not meet the requirement.");
                            }
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                //read info
                PageData pageData = this.ReadInfomationToDataQueue(objWndElement);
                Trace.WriteLine(string.Format("End CaptureUI <<<<<<<<<<<<<<<<<<<<<<< (Time elapsed: {0})", sw.ElapsedMilliseconds));

                //end event
                if (this.CaptureEnd != null)
                {
                    CaptureEndEventArgs eArgs = new CaptureEndEventArgs(pageData.winPageInfo.AssemblyInfoList);
                    this.CaptureEnd(this, eArgs);
                }
            }
            catch
            {
                // error event
                if (this.CaptureError != null)
                {
                    this.CaptureError(this, null);
                }
            }
        }
        private PageData ReadInfomationToDataQueue(IUIAutomationElement objWndElement)
        {
            PageData retPageData = null;
            ElementInformation eiTree = null;
            WindowPageInfo winPageInfo = new WindowPageInfo();
            ElementChangeMonitor elementChangeMonitor = null;
            int tries = 0;
            bool checkLoadingStatus = true;

            Bitmap screenshot = null;

            //stopwatch
            Stopwatch sw = new Stopwatch();

            retry:
            try
            {
                tries++;
                sw.Restart();

                //monitor UI changed during read tree info period
                elementChangeMonitor = new ElementChangeMonitor(objWndElement);

                //screenshot
                screenshot = ImageCapturer.TakeImage(objWndElement.CurrentNativeWindowHandle);

                //read element tree
                IntPtr hWnd = objWndElement.CurrentNativeWindowHandle;


                eiTree = Utility.ReadTree(hWnd, elementChangeMonitor, checkLoadingStatus);
                sw.Stop();
                Trace.WriteLine(string.Format("Utility.ReadTree: {0}", sw.ElapsedMilliseconds));
                winPageInfo.ei = eiTree;

                //get page assembly and full type name
                #region get page assembly and full type name
                List<IUIAutomationElement> objPageControlList = null;
                IUIAutomationElement objPageIdentifier = null;
                if (Utility.TryGetPageControlList(objWndElement, out objPageControlList, out objPageIdentifier))
                {
                    if (objPageControlList != null && objPageControlList.Count > 0)
                    {
                        List<AssemblyInfo> assemblyInfoList = new List<AssemblyInfo>();

                        foreach (IUIAutomationElement objPageControl in objPageControlList)
                        {
                            string assemblyAndTypeName = string.Empty;

                            IntPtr hPageControl = IntPtr.Zero;

                            try
                            {
                                hPageControl = objPageControl.CurrentNativeWindowHandle;
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(string.Format("objPageControl is null? ", ex.Message));
                            }
                            try
                            {
                                if (objPageControl.CurrentFrameworkId.Equals("WPF", StringComparison.OrdinalIgnoreCase))
                                {
                                    assemblyAndTypeName = ManagedInjectorWrapper.Injector.GetWPFType(hPageControl);
                                }
                                else
                                {
                                    assemblyAndTypeName = ManagedInjectorWrapper.Injector.GetWinFormType(hPageControl);
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("can't get the assembly,ex:" + ex.Message);
                            }

                            Trace.WriteLine(string.Format("assemblyAndTypeName: {0}", assemblyAndTypeName));

                            if (!string.IsNullOrEmpty(assemblyAndTypeName) && !assemblyAndTypeName.Contains("InjectorWrapper.dll"))
                            {
                                string[] splitArr = assemblyAndTypeName.Split('$');
                                AssemblyInfo assemblyInfo = null;
                                if (objPageControl == objPageIdentifier)
                                {
                                    assemblyInfo = new AssemblyInfo(splitArr[0], splitArr[1], true);
                                }
                                else
                                {
                                    assemblyInfo = new AssemblyInfo(splitArr[0], splitArr[1], false);
                                }
                                assemblyInfoList.Add(assemblyInfo);
                            }
                        }
                        winPageInfo.AssemblyInfoList = assemblyInfoList;
                    }
                }
                else
                {
                    Trace.WriteLine("No page control found.");
                }
                #endregion

                //get UI tracking route
                string launchedFrom = string.Empty;
                string windowHierarchy = string.Empty;
                Utility.TryGetUITrackingRoute(objWndElement, out launchedFrom, out windowHierarchy);
                Trace.WriteLine(string.Format("launchedFrom: {0}", launchedFrom));
                Trace.WriteLine(string.Format("windowHierarchy: {0}", windowHierarchy));
                winPageInfo.LaunchedFrom = launchedFrom;
                winPageInfo.WindowHierarchy = windowHierarchy;
            }
            catch (SulpHurClientUIChangedException)
            {
                Trace.WriteLine("UI Changed during read tree info, abandon this UI.");
                throw;
            }
            catch (SulpHurClientUINotCompelteLoading)
            {
                sw.Stop();
                // to enhance capture info accuracy
                if (tries > 1)
                {
                    Trace.WriteLine(string.Format("UI not complete loading, abandon this UI. Time elapsed:{0}", sw.ElapsedMilliseconds));
                    throw;
                }
                else
                {
                    Trace.WriteLine(string.Format("UI not complete loading, wait for 0.5 second, and try to read it again. Time elapsed:{0}", sw.ElapsedMilliseconds));
                    System.Threading.Thread.Sleep(500);
                    Trace.WriteLine(string.Format("{0} re-tries to read tree info.", tries));
                    checkLoadingStatus = false;
                    goto retry;
                }
            }
            catch (COMException)
            {
                Trace.WriteLine("COMException during read tree info, abandon this UI.");
                throw;
            }
            catch (Exception notExpectedExcep)
            {
                Trace.WriteLine("****Not Expected Exception found, please investigate****");
                Trace.WriteLine(notExpectedExcep.ToString());
                throw;
            }

            //abandon UI if it is changed
            Trace.WriteLine(string.Format("elementChangeMonitor.IsChanged: {0}", elementChangeMonitor.IsChanged));
            Trace.WriteLine(string.Format("GlobalChangeMonitor.Instance.IsChanged: {0}", GlobalChangeMonitor.Instance.IsChanged));

            if (elementChangeMonitor.IsChanged || GlobalChangeMonitor.Instance.IsChanged)
            {
                Trace.WriteLine("UI Changed when take image, abandon this UI.");
                //Trace.WriteLine("UI Changed when take image.");
                throw new Exception();
            }

            if (screenshot != null)
            {
                retPageData = new PageData();
                retPageData.bitMap = screenshot;
                retPageData.winPageInfo = winPageInfo;
                retPageData.buildLanguage = Utility.GetPID();
                retPageData.productVersion = new Version(Utility.ConsoleVersion);
                Trace.WriteLine(string.Format("ServerContacter.additionInformation.ProductLanguage:{0}", retPageData.buildLanguage));
                Trace.WriteLine(string.Format("ServerContacter.additionInformation.ProductVersion:{0}", retPageData.productVersion));
                lock (DataCenter._queueLocker)
                {
                    DataCenter.Instance.PageDataQueue.Enqueue(retPageData);
                }
            }

            return retPageData;
        }
    }

    public class CaptureEndEventArgs : EventArgs
    {
        public List<AssemblyInfo> AssemblyInfoList { get; set; }

        public CaptureEndEventArgs(List<AssemblyInfo> assemblyInfoList)
        {
            this.AssemblyInfoList = assemblyInfoList;
        }
    }
}

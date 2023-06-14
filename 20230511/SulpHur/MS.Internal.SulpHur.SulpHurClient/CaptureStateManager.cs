using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using interop.UIAutomationCore;
using MS.Internal.SulpHur.SulpHurClient.Monitors;
using MS.Internal.SulpHur.Utilities;
using MS.Internal.SulpHur.SulpHurClient.UIA3;
using MS.Internal.SulpHur.UICompliance;
using System.Threading;

namespace MS.Internal.SulpHur.SulpHurClient
{
    public class CaptureStateManager: IDisposable
    {
        private static CaptureStateManager instance = null;
        private IntPtr hRelatedWindow = IntPtr.Zero;
        private StateForm stateForm = null;
        private bool isDisposed = false;
        public static CaptureStateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CaptureStateManager();
                }
                return instance;
            }
        }
        private CaptureStateManager()
        {
        }

        public void Enable()
        {
            //WindowClosed
            Engine.Instance.WindowClosed += new EventHandler(this.Engine_WindowClosed);
            //ForegroundChanged
            ForegroundMonitor.Instance.ForegroundChanged += new EventHandler(this.ForegroundMonitor_ForegroundChanged);
            //WindowMoved
            MoveSizeMonitor.Instance.WindowMoved += new EventHandler(this.MoveSizeMonitor_WindowMoved);
            //CaptureBegin
            Engine.Instance.CaptureBegin += new EventHandler(this.Engine_CaptureBegin);
            //CaptureEnd
            Engine.Instance.CaptureEnd += new EventHandler<CaptureEndEventArgs>(this.Engine_CaptureEnd);
            //CaptureError
            Engine.Instance.CaptureError += new EventHandler(this.Engine_CaptureError);
        }

        public void Disable()
        {
            //WindowClosed
            Engine.Instance.WindowClosed -= new EventHandler(this.Engine_WindowClosed);
            //ForegroundChanged
            ForegroundMonitor.Instance.ForegroundChanged -= new EventHandler(this.ForegroundMonitor_ForegroundChanged);
            //WindowMoved
            MoveSizeMonitor.Instance.WindowMoved -= new EventHandler(this.MoveSizeMonitor_WindowMoved);
            //CaptureBegin
            Engine.Instance.CaptureBegin -= new EventHandler(this.Engine_CaptureBegin);
            //CaptureEnd
            Engine.Instance.CaptureEnd -= new EventHandler<CaptureEndEventArgs>(this.Engine_CaptureEnd);
            //CaptureError
            Engine.Instance.CaptureError -= new EventHandler(this.Engine_CaptureError);
        }

        private void Engine_WindowClosed(object sender, EventArgs e)
        {
            this.CloseStateForm();
        }
        private void Engine_CaptureBegin(object sender, EventArgs e)
        {
            this.CloseStateForm();
        }
        private void Engine_CaptureEnd(object sender, CaptureEndEventArgs e)
        {
            List<AssemblyInfo> assemblyInfoList = e.AssemblyInfoList;
            if (e.AssemblyInfoList == null)
            {
                assemblyInfoList = new List<AssemblyInfo>();
            }

            this.UpdateState(CaptureState.Ok, assemblyInfoList);
        }
        private void Engine_CaptureError(object sender, EventArgs e)
        {
            this.UpdateState(CaptureState.Error, null);
        }
        private void ForegroundMonitor_ForegroundChanged(object sender, EventArgs e)
        {
            IntPtr hWnd = NativeMethods.GetForegroundWindow();
            //IUIAutomationElement objWndElement = UIA3Automation.RawInstance.ElementFromHandle(hWnd);
            //if (Utility.IsValidWindow(objWndElement) && this.hRelatedWindow.Equals(hWnd))
            if (this.hRelatedWindow.Equals(hWnd))
            {
                if (this.stateForm != null && !this.stateForm.IsVisible)
                {
                    this.stateForm.Show();
                }
            }
            else
            {
                if (this.stateForm != null && this.stateForm.IsVisible)
                {
                    this.stateForm.Hide();
                }
            }
        }
        private void MoveSizeMonitor_WindowMoved(object sender, EventArgs e)
        {
            if (this.stateForm != null)
            {
                this.UpdateState(this.stateForm.State, null);
            }
        }

        public void UpdateState(CaptureState state, List<AssemblyInfo> assemblyInfoList)
        {
            //get foreground window
            IntPtr hWnd = NativeMethods.GetForegroundWindow();
            IUIAutomationElement objWndElement = UIA3Automation.RawInstance.ElementFromHandle(hWnd);
            //validate
            if (Utility.IsValidWindow(objWndElement)||App.UIAutoFlag)
            {
                // product window
                MessageLooper.Instance.Invoke(new Action(() =>
                {
                    if (this.stateForm == null)
                    {
                        this.stateForm = new StateForm();
                        // clean up resource
                        this.stateForm.Closed += delegate(object s, EventArgs e) 
                        {
                            this.stateForm = null;
                            this.hRelatedWindow = IntPtr.Zero;
                        };
                        this.hRelatedWindow = hWnd;
                    }
                    this.stateForm.Show();
                    this.stateForm.State = state;
                    this.stateForm.Left = objWndElement.CurrentBoundingRectangle.left + (objWndElement.CurrentBoundingRectangle.right - objWndElement.CurrentBoundingRectangle.left) / 2;
                    this.stateForm.Top = objWndElement.CurrentBoundingRectangle.top;
                    if (assemblyInfoList != null)
                    {
                        string toolTip = string.Empty;
                        foreach (var assemblyInfo in assemblyInfoList)
                        {
                            string row = string.Format("{0}${1}     {2}", assemblyInfo.AssemblyName, assemblyInfo.FullTypeName, assemblyInfo.IsPageIdentifier ? "(PageIdentifier)" : "");
                            toolTip = string.Format("{0}\n{1}", row, toolTip);
                        }
                        if (string.IsNullOrEmpty(toolTip))
                        {
                            toolTip = "No assembly info in this page.";
                        }
                        this.stateForm.ToolTip = toolTip;
                    }
                }));
            }
            else
            {
                // non-product window
                this.CloseStateForm();
            }
        }
        private void CloseStateForm()
        {
            if (this.stateForm != null)
            {
                MessageLooper.Instance.Invoke(new Action(() =>
                {
                    if (this.stateForm != null)
                    {
                        this.stateForm.Close();
                    }
                }));
            }
        }

        ~CaptureStateManager()
        {
            this.Dispose(false);
        }
        #region IDisposable
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                // state form
                this.CloseStateForm();
                this.isDisposed = true;
            }
        }
        #endregion
    }
}

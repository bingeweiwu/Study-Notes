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
using System.Diagnostics;

namespace MS.Internal.SulpHur.SulpHurClient
{
    public class UploadStateManager : IDisposable
    {
        private static UploadStateManager instance = null;
        private IntPtr hRelatedWindow = IntPtr.Zero;
        private UploadStateForm stateForm = null;
        private bool isDisposed = false;
        public static UploadStateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UploadStateManager();
                }
                return instance;
            }
        }
        private UploadStateManager()
        {
        }

        public void Enable()
        {
            //UploadBegin
            DataCenter.Instance.UploadBegin += new EventHandler(this.DataCenter_UploadBegin);
            //UploadAbandon
            DataCenter.Instance.UploadAbandon += new EventHandler(this.DataCenter_UploadAbandon);
            //UploadEnd
            DataCenter.Instance.UploadEnd += new EventHandler(this.DataCenter_UploadEnd);
            //UploadError
            DataCenter.Instance.UploadError += new EventHandler<UploadErrorEventArgs>(this.DataCenter_UploadError);
        }

        public void Disable()
        {
            //UploadBegin
            DataCenter.Instance.UploadBegin -= new EventHandler(this.DataCenter_UploadBegin);
            //UploadAbandon
            DataCenter.Instance.UploadAbandon -= new EventHandler(this.DataCenter_UploadAbandon);
            //UploadEnd
            DataCenter.Instance.UploadEnd -= new EventHandler(this.DataCenter_UploadEnd);
            //UploadError
            DataCenter.Instance.UploadError -= new EventHandler<UploadErrorEventArgs>(this.DataCenter_UploadError);
        }

        private void DataCenter_UploadBegin(object sender, EventArgs e)
        {
            this.UpdateState(UploadState.Processing, null);
        }
        private void DataCenter_UploadAbandon(object sender, EventArgs e)
        {
            this.UpdateState(UploadState.Abandon, null);
        }
        private void DataCenter_UploadEnd(object sender, EventArgs e)
        {
            this.UpdateState(UploadState.Ok, null);
        }
        private void DataCenter_UploadError(object sender, UploadErrorEventArgs e)
        {
            this.UpdateState(UploadState.Error, e.exceptionMessage);
        }
        public void UpdateState(UploadState state, string toolTip)
        {
            //get foreground window
            IntPtr hWnd = NativeMethods.GetDesktopWindow();
            IUIAutomationElement objWndElement = UIA3Automation.RawInstance.ElementFromHandle(hWnd);
            //validate
            IntPtr hWndforeground = NativeMethods.GetForegroundWindow();
            IUIAutomationElement objWndElementforeground = UIA3Automation.RawInstance.ElementFromHandle(hWndforeground);
            if (state == UploadState.Ok || Utility.IsValidWindow(objWndElementforeground))
            {
                // product window
                MessageLooper.Instance.Invoke(new Action(() =>
                {
                    if (this.stateForm == null)
                    {
                        this.stateForm = new UploadStateForm();
                        // clean up resource
                        this.stateForm.Closed += delegate(object s, EventArgs e)
                        {
                            this.stateForm = null;
                            this.hRelatedWindow = IntPtr.Zero;
                        };
                        this.hRelatedWindow = hWnd;
                    }
                    this.stateForm.State = state;
                    this.stateForm.Left = (objWndElement.CurrentBoundingRectangle.left + objWndElement.CurrentBoundingRectangle.right - 50) / 2 ;
                    this.stateForm.Top = objWndElement.CurrentBoundingRectangle.top;
                    this.stateForm.Show();

                    if (toolTip != null)
                    {
                        this.stateForm.ToolTip = toolTip;
                    }
                }));
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

        ~UploadStateManager()
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

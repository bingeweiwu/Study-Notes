using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Diagnostics;
using interop.UIAutomationCore;
using MS.Internal.SulpHur.Utilities;
using MS.Internal.SulpHur.SulpHurClient.UIA3;


namespace MS.Internal.SulpHur.SulpHurClient
{
    public class ImageCapturer
    {
        public static Bitmap TakeImage(IntPtr handle)
        {
            try
            {
                IUIAutomationElement ae = UIA3Automation.RawInstance.ElementFromHandle(handle);
                return TakeImage(ae);
            }
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("Unexpected Exception when Take Image: {0}",ex));
            }
            return null;
        }

        public static Bitmap TakeImage(IUIAutomationElement ae)
        {
            System.Drawing.Rectangle rect = new Rectangle();
            try
            {
                rect = ae.CurrentBoundingRectangle.ToDrawingRect();
                Bitmap image = new Bitmap(rect.Width, rect.Height);
                Graphics imgGraphics = Graphics.FromImage(image);
                imgGraphics.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size);

                //v-danpgu: call getpid method everytime you capture the UI to make sure the build lan is correct
                //App app = App.Current as App;
                //app.GetBuildLan();
                return image;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Unexpected Exception when Take Image: {0}", ex));
                Trace.WriteLine(string.Format("X:{0},Y:{1},{2}", rect.X, rect.Y, ex));
            }
            return null;
        }
    }
}

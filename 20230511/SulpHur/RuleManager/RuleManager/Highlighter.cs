using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MS.Internal.SulpHur.UICompliance;
using System.Threading;
using System.ComponentModel;

namespace RuleManager
{
    /// <summary>
    /// A class used for highlighting sections of the screen.
    /// </summary>
    public class Highlighter : Form
    {
        /// <summary>
        /// WinForms doesn't allow us to ShowWindow with the SW_SHOWNA flag, so we have to do so ourselves.
        /// </summary>
        private static class NativeMethods
        {
            public const int SWP_NOSIZE = 0x0001;
            public const int SWP_NOMOVE = 0x0002;
            public const int SWP_NOACTIVATE = 0x0010;
            public const int SWP_SHOWWINDOW = 0x0040;

            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cX, int cY, uint uFlags);
        }

        /// <summary>
        /// Initializes the Highlighter class.
        /// </summary>
        public Highlighter()
            : this(HighlighterProperties.Default)
        {
        }

        /// <summary>
        /// Initializes the Highlighter class, with the given properties.
        /// </summary>
        /// <param name="properties">The properties to use for highlighting.</param>
        public Highlighter(HighlighterProperties properties)
        {

            this._disposed = false;
            this._properties = properties;
            this.BackColor = this._properties.Color;
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HighlighterWindowName";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Text = this.Name;
            this.TopMost = true;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (false == this._disposed)
            {
                base.Dispose(disposing);
                this._disposed = true;
            }
        }

        /// <summary>
        /// Specifies the location that should be highlighted.
        /// </summary>
        /// <param name="rectangle">A Rectangle defining the location to be highlighted.</param>
        public void SetLocation(Rectangle rectangle)
        {
            int totalBorder = _properties.BorderBuffer + _properties.BorderWidth;

            this._outerRectangle = new Rectangle(
                    new System.Drawing.Point(0, 0),
                    rectangle.Size + new System.Drawing.Size(totalBorder * 2, totalBorder * 2));

            this._innerRectangle = new Rectangle(
                    new System.Drawing.Point(this._properties.BorderWidth, this._properties.BorderWidth),
                    rectangle.Size + new System.Drawing.Size(this._properties.BorderBuffer * 2, this._properties.BorderBuffer * 2));

            // Set the region of the form
            //
            Region frmRegion = new Region(_outerRectangle);
            frmRegion.Exclude(_innerRectangle);

            this.Location = rectangle.Location - new System.Drawing.Size(totalBorder, totalBorder);
            this.Size = this._outerRectangle.Size;
            this.Region = frmRegion;
        }

        /// <summary>
        /// Displays the control to the user. 
        /// </summary>
        public new void Show()
        {
            NativeMethods.SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
        }


        #region Internal non-WinForms APIs
        /// <summary>
        /// Static Highlight method which takes a UIObject and actually draws the form
        /// </summary>
        /// <param name="uiObject">The UIObject you want to highlight</param>
        internal static void Highlight(ElementInformation ei)
        {
            Highlighter.Highlight(ConvertRect(ei.BoundingRectangle), HighlighterProperties.Default);
        }

        internal static void Highlight(Rectangle rect)
        {
            Highlighter.Highlight(rect, HighlighterProperties.Default);
        }

        internal static Rectangle ConvertRect(System.Windows.Rect target)
        {
            return new Rectangle((int)target.X, (int)target.Y, (int)target.Width, (int)target.Height);
        }

        /// <summary>
        /// Static Hightlight method which takes a rectangle and draws the form
        /// </summary>
        /// <param name="rectangle">A rectangle defining the area that you want to highlight</param>
        /// <param name="properties">Properties for the Highlighter window.</param>
        internal static void Highlight(Rectangle rectangle, HighlighterProperties properties)
        {
            // When you use WinForms it specifies its own SynchronizationContext to the AsyncOperationManager.
            // For that SynchronizationContext to work, there needs to be a thread processing messages. 
            // Unfortunately, we tear down the message processing thread when we're done (otherwise the process can't
            // exit!). Therefore we remember the original SynchronizationContext, and re-set it when WinForms
            // is done.
            //
            SynchronizationContext syncContext = AsyncOperationManager.SynchronizationContext;

            // Define the thread and thread procedure that processes the Windows messages, and automatically shuts
            // down after the specified length of time.
            //
            Thread highlighterThread = new Thread(delegate()
            {
                // System.Windows.Forms.Timer and highligher form
                //  must be created on the thread that processes messages.
                Highlighter highlighter = new Highlighter(properties);

                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

                timer.Interval = (int)highlighter.Properties.Timeout.TotalMilliseconds;
                timer.Tick += delegate(object sender, EventArgs e)
                {
                    Application.ExitThread();
                };
                timer.Enabled = true;

                highlighter.SetLocation(rectangle);

                Application.Run(highlighter);
            });

            highlighterThread.Start();
            highlighterThread.Join();

            AsyncOperationManager.SynchronizationContext = syncContext;
        }
        #endregion

        /// <summary>
        /// Override the 'OnPaint' method to provide our imlpementation.
        /// </summary>
        /// <param name="e">The PaintEventArgs associated with this event.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // need to tweak the rectangles to paint the border correctly
            System.Drawing.Rectangle tmpOuterRectangle = new System.Drawing.Rectangle(this._outerRectangle.Left, this._outerRectangle.Top, this._outerRectangle.Width - 1, this._outerRectangle.Height - 1);
            System.Drawing.Rectangle tmpInnerRectangle = new System.Drawing.Rectangle(this._innerRectangle.Left - 1, this._innerRectangle.Top - 1, this._innerRectangle.Width + 1, this._innerRectangle.Height + 1);

            // draw the border
            e.Graphics.DrawRectangle(new Pen(this.ForeColor), tmpInnerRectangle);
            e.Graphics.DrawRectangle(new Pen(this.ForeColor), tmpOuterRectangle);
        }

        /// <summary>
        /// The HighlighterProperties associated with this Highlighter instance.
        /// </summary>
        /// <value>The HighlighterProperties associated with this Highlighter instance.</value>
        protected HighlighterProperties Properties
        {
            get { return this._properties; }
        }

        /// <summary>
        /// The HighlighterProperties associated with this Highlighter instance.
        /// </summary>
        private HighlighterProperties _properties;

        /// <summary>
        /// true if we've been disposed, false otherwise.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The 'innerRectangle'. The highlighting window is defined as the outerRectangle with the innerRectangle
        /// 'Excluded'.
        /// </summary>
        private System.Drawing.Rectangle _innerRectangle;

        /// <summary>
        /// The 'outerRectangle'. The highlighting window is defined as the outerRectangle with the innerRectangle
        /// 'Excluded'.
        /// </summary>
        private System.Drawing.Rectangle _outerRectangle;
    }

    /// <summary>
    /// A class that defines the settable properties for the Highlighter class.
    /// </summary>
    public class HighlighterProperties
    {
        /// <summary>
        /// Initializes a new HighlighterProperties with the default properties.
        /// </summary>
        public HighlighterProperties()
        {
            this._borderWidth = 1;
            this._borderBuffer = 1;
            this._color = Color.Yellow;
            this._timeout = TimeSpan.FromSeconds(5);
        }

        #region Accessors
        /// <summary>
        /// Width of the highlight rectangle.
        /// </summary>
        /// <value>Width of the highlight rectangle.</value>
        public int BorderWidth
        {
            get
            {
                return this._borderWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw new Exception("border width");
                }
                this._borderWidth = value;
            }
        }

        /// <summary>
        /// Distance between the actual rect and the highlight rectangle.
        /// </summary>
        /// <value>Distance between the actual rect and the highlight rectangle.</value>
        public int BorderBuffer
        {
            get
            {
                return this._borderBuffer;
            }
            set
            {
                if (value < 0)
                {
                    throw new Exception("border buffer");
                }
                this._borderBuffer = value;
            }
        }

        /// <summary>
        /// The color of the window.
        /// </summary>
        /// <value>The color of the window.</value>
        public Color Color
        {
            get
            {
                return this._color;
            }

            set
            {
                this._color = value;
            }
        }

        /// <summary>
        /// The time that the Highlighter window should be shown for.
        /// </summary>
        /// <value>A TimeSpan instance that defines the length of time that the Highlighter window should be visible for.</value>
        public TimeSpan Timeout
        {
            get
            {
                // TimeSpan types are immutable, no need to copy when get'ing or set'ing.
                //
                return this._timeout;
            }
            set
            {
                // TimeSpan types are immutable, no need to copy when get'ing or set'ing.
                //
                this._timeout = value;
            }
        }

        /// <summary>
        /// The default properties for the Highlighter class.
        /// </summary>
        /// <value>The default properties for the Highlighter class.</value>
        public static HighlighterProperties Default
        {
            get
            {
                return HighlighterProperties._defaultProperties;
            }
            set
            {

                HighlighterProperties._defaultProperties = value;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private int _borderWidth;

        /// <summary>
        /// 
        /// </summary>
        private int _borderBuffer;

        /// <summary>
        /// 
        /// </summary>
        private Color _color;

        /// <summary>
        /// 
        /// </summary>
        private TimeSpan _timeout;

        /// <summary>
        /// 
        /// </summary>
        private static HighlighterProperties _defaultProperties = new HighlighterProperties();
    }
}

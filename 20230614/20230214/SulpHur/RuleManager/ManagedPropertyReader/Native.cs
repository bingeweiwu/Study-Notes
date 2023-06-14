using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace ManagedPropertyReader
{
    public class Native
    {
        public const int WM_GETFONT = 0x0031;

        public const int OBJID_CLIENT = unchecked((int)0xFFFFFFFC);

        public const int CHILDID_SELF = 0;

        public const string strAccessibleGUID = "{618736E0-3C3D-11CF-810C-00AA00389B71}";

        #region Accessibility Roles

        /// <summary>
        /// Accessibility role:  System Client
        /// </summary>
        public const int ROLE_SYSTEM_TITLEBAR = 0x1;
        public const int ROLE_SYSTEM_SCROLLBAR = 0x3;
        public const int ROLE_SYSTEM_GRIP = 0x4;
        public const int ROLE_SYSTEM_SOUND = 0x5;
        public const int ROLE_SYSTEM_CURSOR = 0x6;
        public const int ROLE_SYSTEM_CARET = 0x7;
        public const int ROLE_SYSTEM_ALERT = 0x8;
        public const int ROLE_SYSTEM_WINDOW = 0x9;
        public const int ROLE_SYSTEM_CLIENT = 0x0A;
        public const int ROLE_SYSTEM_MENUPOPUP = 0xb;
        public const int ROLE_SYSTEM_MENUITEM = 0xc;
        public const int ROLE_SYSTEM_TOOLTIP = 0xd;
        public const int ROLE_SYSTEM_APPLICATION = 0xe;
        public const int ROLE_SYSTEM_DOCUMENT = 0xf;
        public const int ROLE_SYSTEM_PANE = 0x10;
        public const int ROLE_SYSTEM_CHART = 0x11;
        public const int ROLE_SYSTEM_DIALOG = 0x12;
        public const int ROLE_SYSTEM_BORDER = 0x13;
        public const int ROLE_SYSTEM_GROUPING = 0x14;
        public const int ROLE_SYSTEM_SEPARATOR = 0x15;
        public const int ROLE_SYSTEM_TOOLBAR = 0x16;
        public const int ROLE_SYSTEM_STATUSBAR = 0x17;
        public const int ROLE_SYSTEM_TABLE = 0x18;  // propertyGridView
        public const int ROLE_SYSTEM_COLUMNHEADER = 0x19;
        public const int ROLE_SYSTEM_ROWHEADER = 0x1A;
        public const int ROLE_SYSTEM_COLUMN = 0x1b;
        public const int ROLE_SYSTEM_ROW = 0x1c;
        public const int ROLE_SYSTEM_CELL = 0x1d;
        public const int ROLE_SYSTEM_LINK = 0x1e;
        public const int ROLE_SYSTEM_HELPBALLOON = 0x1f;
        public const int ROLE_SYSTEM_CHARACTER = 0x20;
        public const int ROLE_SYSTEM_LIST = 0x21;
        public const int ROLE_SYSTEM_LISTITEM = 0x22;
        public const int ROLE_SYSTEM_OUTLINE = 0x23;  // treeview
        public const int ROLE_SYSTEM_OUTLINEITEM = 0x24;
        public const int ROLE_SYSTEM_PAGETAB = 0x25;
        public const int ROLE_SYSTEM_PROPERTYPAGE = 0x26;
        public const int ROLE_SYSTEM_INDICATOR = 0x27;
        public const int ROLE_SYSTEM_GRAPHIC = 0x28;
        public const int ROLE_SYSTEM_STATICTEXT = 0X29;
        public const int ROLE_SYSTEM_TEXT = 0x2a;
        public const int ROLE_SYSTEM_PUSHBUTTON = 0x2b;
        public const int ROLE_SYSTEM_CHECKBUTTON = 0x2c;
        public const int ROLE_SYSTEM_RADIOBUTTON = 0x2d;
        public const int ROLE_SYSTEM_COMBOBOX = 0x2e;
        public const int ROLE_SYSTEM_DROPLIST = 0x2f;
        public const int ROLE_SYSTEM_PROGRESSBAR = 0x30;
        public const int ROLE_SYSTEM_DIAL = 0x31;
        public const int ROLE_SYSTEM_HOTKEYFIELD = 0x32;
        public const int ROLE_SYSTEM_DIAGRAM = 0x35;
        public const int ROLE_SYSTEM_ANIMATION = 0x36;
        public const int ROLE_SYSTEM_EQUATION = 0x37;
        public const int ROLE_SYSTEM_BUTTONDROPDOWN = 0x38;
        public const int ROLE_SYSTEM_BUTTONMENU = 0x39;
        public const int ROLE_SYSTEM_BUTTONDROPDOWNGRID = 0x3a;
        public const int ROLE_SYSTEM_WHITESPACE = 0x3b;
        public const int ROLE_SYSTEM_CLOCK = 0x3d;
        public const int ROLE_SYSTEM_SPLITBUTTON = 0x3e;
        public const int ROLE_SYSTEM_IPADDRESS = 0x3f;
        public const int ROLE_SYSTEM_OUTLINEBUTTON = 0x40;

        /// <summary>
        /// Accessibility role:  System Slider
        /// </summary>
        public const int ROLE_SYSTEM_SLIDER = 0x33;

        /// <summary>
        /// Accessibility role:  System Spin Button
        /// </summary>
        public const int ROLE_SYSTEM_SPINBUTTON = 0x34;

        /// <summary>
        /// Accessibility role:  System Page Tab List (Tab Control)
        /// </summary>
        public const int ROLE_SYSTEM_PAGETABLIST = 0x3C;

        public const int BS_MULTILINE = 0x2000;

        #endregion Accessibility Roles

        #region MSAA Object ID

        public enum MsaaObjectID : uint
        {
            Window = 0x00000000,
            SysMenu = 0xFFFFFFFF,
            TitleBar = 0xFFFFFFFE,
            Menu = 0xFFFFFFFD,
            Client = 0xFFFFFFFC,
            VScroll = 0xFFFFFFFB,
            HScroll = 0xFFFFFFFA,
            SizeGrip = 0xFFFFFFF9,
            Caret = 0xFFFFFFF8,
            Cursor = 0xFFFFFFF7,
            Alert = 0xFFFFFFF6,
            Sound = 0xFFFFFFF5,
            NativeOM = 0xFFFFFFF0
        }

        #endregion

        #region Managed Control Agent

        public static object GetProperty(IntPtr handle, string propertyName)
        {
            return Microsoft.Tools.ManagedControlAgent.GetProperty(handle, propertyName);

        }

        #endregion

        #region Windows messages

        public const uint WM_PAINT = 0x000F;
        public const uint WM_ERASEBKGND = 0x0014;
        public const uint WM_KEYDOWN = 0x0100;
        public const uint WM_KEYUP = 0x0101;
        public const uint WM_MOUSEMOVE = 0x0200;
        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_NOTIFY = 0x4E;

        #endregion

        #region Windows API structure definition

        [Serializable]
        public struct RECT
        {
            public int Bottom;
            public int Left;
            public int Right;
            public int Top;
        }

        public struct Point
        {
            public int x;
            public int y;
        }

        public struct tagTEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;
        }

        #endregion

        #region Windows API

        [DllImport("user32.dll")]
        public extern static IntPtr SendMessage(IntPtr HWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        public extern static int ClientToScreen(IntPtr HWnd, ref System.Drawing.Point point);

        [DllImport("gdi32.dll")]
        public extern static bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll")]
        public extern static IntPtr GetWindowDC(IntPtr HWnd);

        [DllImport("user32.dll")]
        public extern static int ReleaseDC(IntPtr HWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        public extern static int ScreenToClient(IntPtr HWnd, ref Point point);

        [DllImport("gdi32", EntryPoint = "GetTextExtentPoint32")]
        public static extern int GetTextExtentPoint32(IntPtr hDC, string text, int length, ref Size size);

        [DllImport("gdi32", EntryPoint = "GetTextMetrics")]
        public static extern int GetTextMetrics(IntPtr hDC, ref tagTEXTMETRIC lpMetrics);

        [DllImport("user32.dll")]
        public extern static bool GetWindowInfo(IntPtr HWnd, ref WINDOWINFO pWindowInfo);

        [DllImport("oleacc.dll")]
        public extern static int AccessibleChildren(Accessibility.IAccessible paccContainer, int iChildStart, int cChildren, [Out] object[] rgvarChildren, out int pcObtained);

        [DllImport("oleacc.dll")]
        public extern static int AccessibleObjectFromPoint(int lx, int ly, ref Accessibility.IAccessible ppoleAcc, ref object pvarElement);

        [DllImport("oleacc.dll")]
        public extern static int AccessibleObjectFromWindow(IntPtr hwnd, uint dwId, ref Guid riid, ref Accessibility.IAccessible ppoleAcc);

        [DllImport("oleacc.dll")]
        public static extern uint GetRoleText(uint dwRole, [Out] StringBuilder lpszRole, uint cchRoleMax);

        [DllImport("oleacc.dll")]
        public static extern uint GetStateText(uint dwStateBit, [Out] StringBuilder lpszStateBit, uint cchStateBitMax);

        #endregion
    }
}

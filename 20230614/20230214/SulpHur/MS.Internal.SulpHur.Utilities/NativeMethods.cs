using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Runtime.InteropServices.ComTypes;
using mshtml;
using System.IO;
using System.Security;
using System.Runtime.ConstrainedExecution;

namespace MS.Internal.SulpHur.Utilities
{
    public sealed class NativeMethods
    {
        // Fields
        public const int BM_CLICK = 0xf5;
        public const int EM_GETTEXTEX = 0x45e;
        public const int EM_GETTEXTLENGTHEX = 0x45f;
        private static string enumChildWindowClassName;
        public const int GW_CHILD = 5;
        public const int GW_HWNDNEXT = 2;
        public const int KEYEVENTF_EXTENDEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 2;
        public const int KEYEVENTF_TAB = 9;
        public const int MA_ACTIVATE = 1;
        public const int SC_CLOSE = 0xf060;
        public const int SMTO_ABORTIFHUNG = 2;
        public const int WM_ACTIVATE = 6;
        public const int WM_CLOSE = 0x10;
        public const int WM_GETTEXT = 13;
        public const int WM_GETTEXTLENGTH = 14;
        public const int WM_SYSCOMMAND = 0x112;

        // Methods
        [DllImport("gdi32")]
        public static extern int DeleteObject(IntPtr o);

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetForegroundWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(int idAttach, int idAttachTo, bool fAttach);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void ClickDialogButton(int buttonid, IntPtr parentHwnd);
        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern bool EnumChildWindows(IntPtr hWnd, ref IntPtr lParam);
        [DllImport("user32", EntryPoint = "GetClassNameA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        internal static extern int GetClassName(IntPtr handleToWindow, StringBuilder className, int maxClassNameLength);
 


        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, ref IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumThreadWindows(int threadId, EnumThreadProc pfnEnum, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow", CharSet = CharSet.Auto, SetLastError = true)] 
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);
        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern string GetClassName(IntPtr hwnd);
        //[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //public static extern int GetClassName(IntPtr hWnd, out StringBuilder lpClassName, int nMaxCount);
        [DllImport("kernel32")]
        public static extern int GetCurrentThreadId();
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr GetDlgItem(IntPtr handleToWindow, int ControlId);
        [DllImport("User32.dll")]
        public static extern IntPtr GetParent(IntPtr hwnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr GetWindow(IntPtr handleToWindow, int wParam);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetWindowText(IntPtr handleToWindow, StringBuilder windowText, int maxTextLength);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool IsWindowEnabled(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        //[DllImport("oleacc", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        //public static extern int ObjectFromLresult(int lResult, ref Guid riid, int wParam, ref IHTMLDocument2 ppvObject);
        [DllImport("user32", EntryPoint="RegisterWindowMessageA", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        public static extern int RegisterWindowMessage(string lpString);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, StringBuilder lParam);
        [DllImport("user32", EntryPoint="SendMessageTimeoutA", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        public static extern int SendMessageTimeout(IntPtr hWnd, int msg, int wParam, int lParam, int fuFlags, int uTimeout, ref int lpdwResult);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("oleacc", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int ObjectFromLresult(int lResult, ref Guid riid, int wParam, ref IHTMLDocument2 ppvObject);
        [DllImport("ole32.dll", PreserveSig = false)]
        public static extern void CreateStreamOnHGlobal(IntPtr hGlobal, Boolean fDeleteOnRelease, [Out] out IStream pStream);
        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern HookSafeHandle SetWinEventHook(NativeStructs.EventId eventMin, NativeStructs.EventId eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, NativeStructs.SetWinEventHookFlags dwflags);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern int UnhookWinEvent(IntPtr hWinEventHook);



        public static IntPtr GetChildWindowHwnd(IntPtr parentHwnd, string className)
        {
            IntPtr hWnd = IntPtr.Zero;
            enumChildWindowClassName = className;
            EnumChildProc childProc = new EnumChildProc(NativeMethods.EnumChildWindows);
            EnumChildWindows(parentHwnd, childProc, ref hWnd);
            return hWnd;
        }
        private static bool EnumChildWindows(IntPtr hWnd, ref IntPtr lParam)
        {
            if (NativeMethods.CompareClassNames(hWnd, enumChildWindowClassName))
            {
                lParam = hWnd;
                return false;
            }
            return true;
        }
        public static bool CompareClassNames(IntPtr hWnd, string expectedClassName)
        {
            if (hWnd == IntPtr.Zero)
            {
                return false;
            }
            return NativeMethods.GetClassName(hWnd).Equals(expectedClassName);
        }
        public static string GetClassName(IntPtr hwnd)
        {
            StringBuilder className = new StringBuilder(0xff);
            if (GetClassName(hwnd, className, className.MaxCapacity) == 0)
            {
                return string.Empty;
            }
            return className.ToString();
        }
        // Nested Types
        public delegate bool EnumChildProc(IntPtr hWnd, ref IntPtr lParam);
        public delegate bool EnumThreadProc(IntPtr hwnd, IntPtr lParam);

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000100-0000-0000-C000-000000000046")]
        public interface IEnumUnknown
        {
            [PreserveSig]
            int Next([In, MarshalAs(UnmanagedType.U4)] int celt, [MarshalAs(UnmanagedType.IUnknown)] out object rgelt, [MarshalAs(UnmanagedType.U4)] out int pceltFetched);
            [PreserveSig]
            int Skip([In, MarshalAs(UnmanagedType.U4)] int celt);
            void Reset();
            void Clone(out NativeMethods.IEnumUnknown ppenum);
        }
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000011B-0000-0000-C000-000000000046")]
        public interface IOleContainer
        {
            [PreserveSig]
            int ParseDisplayName([In, MarshalAs(UnmanagedType.Interface)] object pbc, [In, MarshalAs(UnmanagedType.BStr)] string pszDisplayName, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pchEaten, [Out, MarshalAs(UnmanagedType.LPArray)] object[] ppmkOut);
            [PreserveSig]
            int EnumObjects([In, MarshalAs(UnmanagedType.U4)] NativeMethods.tagOLECONTF grfFlags, out NativeMethods.IEnumUnknown ppenum);
            [PreserveSig]
            int LockContainer(bool fLock);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [Flags]
        public enum tagOLECONTF
        {
            OLECONTF_EMBEDDINGS = 1,
            OLECONTF_LINKS = 2,
            OLECONTF_ONLYIFRUNNING = 0x10,
            OLECONTF_ONLYUSER = 8,
            OLECONTF_OTHERS = 4
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public NativeMethods.RECT rcWindow;
            public NativeMethods.RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public NativeMethods.POINT ptMinPosition;
            public NativeMethods.POINT ptMaxPosition;
            public NativeMethods.RECT rcNormalPosition;
        }
        public enum WindowShowStyle
        {
            ForceMinimized = 11,
            Hide = 0,
            Maximize = 3,
            Minimize = 6,
            Restore = 9,
            Show = 5,
            ShowDefault = 10,
            ShowMaximized = 3,
            ShowMinimized = 2,
            ShowMinNoActivate = 7,
            ShowNoActivate = 8,
            ShowNormal = 1,
            ShowNormalNoActivate = 4
        }
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7FD52380-4E07-101B-AE2D-08002B2EC713")]
    public interface IPersistStreamInit
    {
        void GetClassID([In, Out] ref Guid pClassID);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int IsDirty();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Load([In] IStream pstm);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Save([In] IStream pstm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
        void GetSizeMax([Out] long pcbSize);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int InitNew();
    }
    public class ComStreamAdapter : IStream
    {
        private Stream stream = null;
        public Stream Stream
        {
            get { return stream; }
        }
        
        
        public ComStreamAdapter(Stream stream)
        {
            this.stream = stream;
        }

        void IStream.Clone(out IStream ppstm)
        {
            ppstm = null;
        }

        public void Commit(int grfCommitFlags)
        {

        }

        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {

        }

        public void LockRegion(long libOffset, long cb, int dwLockType)
        {

        }

        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            long bytes_read = this.stream.Read(pv, 0, cb);
            if (pcbRead != IntPtr.Zero) 
                Marshal.WriteInt64(pcbRead, bytes_read);
        }

        public void Revert()
        {
        }

        void IStream.Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            long pos = stream.Seek(dlibMove, (SeekOrigin)dwOrigin);
            if (plibNewPosition != IntPtr.Zero) 
                Marshal.WriteInt64(plibNewPosition, pos);
        }

        public void SetSize(long libNewSize)
        {
            stream.SetLength(libNewSize);
        }

        public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new System.Runtime.InteropServices.ComTypes.STATSTG();
        }

        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
        }

        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            stream.Write(pv, 0, cb);
            if (pcbWritten != IntPtr.Zero)
                Marshal.WriteInt64(pcbWritten, (long)cb);

        }

        public IStream ToComStream()
        {
            IStream comStream = null;
            try
            {
                this.stream.Position = 0;
                byte[] bArr = new byte[this.stream.Length];
                this.stream.Read(bArr,0,(int)this.stream.Length);
                string result = Encoding.UTF8.GetString(bArr);
                //StreamReader sr = new StreamReader(this.stream, Encoding.UTF8);
                //string result = sr.ReadToEnd();
                Log.WriteLog(result);
                NativeMethods.CreateStreamOnHGlobal(Marshal.StringToHGlobalUni(result), true, out comStream);

                return comStream;
            }
            catch
            {
                return null;
            }
        }
    }
}

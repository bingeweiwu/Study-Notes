//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Reflection;
//using System.IO;
//using System.Diagnostics;
//using System.Windows.Forms;
//using System.Windows.Interop;
//using System.Runtime.InteropServices;

//namespace ManagedInjectorWrapper
//{
//    [Flags]
//    enum ProcessAccessFlags : uint
//    {
//        All = 0x001F0FFF,
//        Terminate = 0x00000001,
//        CreateThread = 0x00000002,
//        VMOperation = 0x00000008,
//        VMRead = 0x00000010,
//        VMWrite = 0x00000020,
//        DupHandle = 0x00000040,
//        SetInformation = 0x00000200,
//        QueryInformation = 0x00000400,
//        Synchronize = 0x00100000
//    }

//    // Thread proc, to be used with Create*Thread
//    public delegate int ThreadProc(IntPtr param);

//    public static class Injector
//    {
//        [DllImport("kernel32.dll", SetLastError = true)]
//        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

//        [DllImport("kernel32.dll")]
//        static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

//        [DllImport("kernel32.dll")]
//        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);

//        [DllImport("user32.dll", SetLastError = true)]
//        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

//        [DllImport("kernel32")]
//        public static extern IntPtr CreateRemoteThread(
//          IntPtr hProcess,
//          IntPtr lpThreadAttributes,
//          uint dwStackSize,
//          IntPtr lpStartAddress, // raw Pointer into remote process
//          IntPtr lpParameter,
//          uint dwCreationFlags,
//          out uint lpThreadId
//        );



//        // stdcall        
//        public static int MyThreadProc(IntPtr param)
//        {
//            int pid = Process.GetCurrentProcess().Id;
//            Console.WriteLine("Pid {0}: Inside my new thread!. Param={1}", pid, param.ToInt32());
//            return 1;
//        }

//        // Helper to wait for a thread to exit and print its exit code
//        public static void WaitForThreadToExit(IntPtr hThread)
//        {
//            WaitForSingleObject(hThread, unchecked((uint)-1));

//            uint exitCode;
//            GetExitCodeThread(hThread, out exitCode);
//            int pid = Process.GetCurrentProcess().Id;
//            Console.WriteLine("Pid {0}: Thread exited with code: {1}", pid, exitCode);
//        }


//        public static string GetWinFormType(IntPtr nativeHandle)
//        {
//            uint processId;
//            GetWindowThreadProcessId(nativeHandle, out processId);

//            IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, processId);

//            ThreadProc proc = new ThreadProc(ThreadStart);

//            GCHandle.Alloc(proc);
            

//            IntPtr fpProc = Marshal.GetFunctionPointerForDelegate(proc);

//            uint dwThreadId;
//            // Create a thread in the first process.
//            IntPtr hThread = CreateRemoteThread(
//                hProcess,
//                IntPtr.Zero,
//                0,
//                fpProc,
//                nativeHandle,
//                0,
//                out dwThreadId);
//            WaitForThreadToExit(hThread);

//            return "asd.dll$asd";
//            //return GetType(nativeHandle, typeof(Injector).Assembly, typeof(Injector).FullName, "InProcGetWinFormType");
//        }

//        private static int ThreadStart(IntPtr hWnd)
//        {
//            return 0;
//        }

//        public static string GetWPFType(IntPtr nativeHandle)
//        {
//            return "asd.dll$asd";
//            //return GetType(nativeHandle, typeof(Injector).Assembly, typeof(Injector).FullName, "InProcGetWPFType");
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Interop;

namespace ManagedInjectorWrapper
{
    public static class Injector
    {
        private static MethodInfo method;

        static Injector()
        {
            Assembly injectorAssembly;

            FileInfo fileInfo = new FileInfo(typeof(Injector).Assembly.Location);
            string path = fileInfo.Directory.FullName;

            if (System.Environment.Is64BitProcess == true)
            {
                path = Path.Combine(path, "ManagedInjector_x64.dll");
            }
            else
            {
                path = Path.Combine(path, "ManagedInjector_x86.dll");
            }

            injectorAssembly = Assembly.LoadFrom(path);

            Type injectorType = injectorAssembly.GetType("ManagedInjector.Injector");
            method = injectorType.GetMethod("GetControlType");
        }

        public static string GetWinFormType(IntPtr nativeHandle)
        {
            return GetType(nativeHandle, typeof(Injector).Assembly, typeof(Injector).FullName, "InProcGetWinFormType");
        }

        public static string GetWPFType(IntPtr nativeHandle)
        {
            return GetType(nativeHandle, typeof(Injector).Assembly, typeof(Injector).FullName, "InProcGetWPFType");
        }

        public static string GetType(IntPtr windowHandle, Assembly assemblyName, string className, string methodName)
        {
            return (string)method.Invoke(null, new object[] { windowHandle, assemblyName, className, methodName });
        }

        public static string InProcGetWinFormType(IntPtr nativeHandle)
        {
            Type winFormType = Control.FromHandle(nativeHandle).GetType();

            return string.Format("{0}${1}", winFormType.Module.Name, winFormType.FullName);
        }

        public static string InProcGetWPFType(IntPtr nativeHandle)
        {
            HwndSource hwndSource = HwndSource.FromHwnd(nativeHandle);
            Type visualType = hwndSource.RootVisual.GetType();

            return string.Format("{0}${1}", visualType.Module.Name, visualType.FullName);
        }

        public static string InProcGetContext()
        {
            try
            {
                //Assembly foundationAssembly = Assembly.LoadFrom("Microsoft.EnterpriseManagement.UI.Foundation.dll");
                //Assembly configurationManagementAssembly = Assembly.LoadFrom("Microsoft.ConfigurationManagement.exe");
                //Type frameworkServicesType = foundationAssembly.GetType("Microsoft.EnterpriseManagement.ConsoleFramework.FrameworkServices");
                //Type navigationViewContextType = foundationAssembly.GetType("Microsoft.EnterpriseManagement.ConsoleFramework.INavigationViewContext");
                //Type navigationModelNodeType = foundationAssembly.GetType("Microsoft.EnterpriseManagement.ConsoleFramework.NavigationModelNodeBase");
                //Type sccmResultItemNodeType = configurationManagementAssembly.GetType("Microsoft.ConfigurationManagement.AdminConsole.SccmResultItemNode");

                //MethodInfo getServiceMethod = frameworkServicesType.GetMethod("GetService", new Type[] { typeof(object) }).MakeGenericMethod(navigationViewContextType);
                //object viewContext = getServiceMethod.Invoke(null, new object[] { null });

                //object contextItems = viewContext.GetType().GetProperty("ContextItems").GetGetMethod().Invoke(viewContext, null);
                //object sccmResultItemNode = contextItems.GetType().GetProperty("Item").GetValue(contextItems, new object[] { 0 });

                //object scopeNode = sccmResultItemNodeType.GetProperty("ScopeNode").GetGetMethod().Invoke(sccmResultItemNode, null);
                //Uri location = (Uri)scopeNode.GetType().GetProperty("Location").GetGetMethod().Invoke(scopeNode, null);

                //while (scopeNode.GetType().GetProperty("ResultObject").GetGetMethod().Invoke(scopeNode, null) != null)
                //{
                //    scopeNode = scopeNode.GetType().GetProperty("Parent").GetGetMethod().Invoke(scopeNode, null);
                //    location = (Uri)scopeNode.GetType().GetProperty("Location").GetGetMethod().Invoke(scopeNode, null);
                //}

                //return location.ToString();

                Assembly foundationAssembly = Assembly.LoadFrom("Microsoft.EnterpriseManagement.UI.Foundation.dll");
                Assembly configurationManagementAssembly = Assembly.LoadFrom("Microsoft.ConfigurationManagement.exe");
                Type frameworkServicesType = foundationAssembly.GetType("Microsoft.EnterpriseManagement.ConsoleFramework.FrameworkServices");
                Type navigationViewContextType = foundationAssembly.GetType("Microsoft.EnterpriseManagement.ConsoleFramework.INavigationViewContext");
                Type navigationModelNodeType = foundationAssembly.GetType("Microsoft.EnterpriseManagement.ConsoleFramework.NavigationModelNodeBase");
                Type sccmResultItemNodeType = configurationManagementAssembly.GetType("Microsoft.ConfigurationManagement.AdminConsole.SccmResultItemNode");
                Type objectInstanceRegistryType = foundationAssembly.GetType("Microsoft.EnterpriseManagement.ConsoleFramework.IObjectInstanceRegistry");

                MethodInfo getInstanceRegistryServiceMethod = frameworkServicesType.GetMethod("GetService", new Type[] { }).MakeGenericMethod(objectInstanceRegistryType);

                object instanceRegistry = getInstanceRegistryServiceMethod.Invoke(null, null);

                object[] parameters = new object[] { new Uri("msscnav://root/Windows/Wunderbar"), null };
                objectInstanceRegistryType.GetMethod("TryFindObject").Invoke(instanceRegistry, parameters);

                MethodInfo getNavigationContextServiceMethod = frameworkServicesType.GetMethod("GetService", new Type[] { typeof(object) }).MakeGenericMethod(navigationViewContextType);


                object viewContext = getNavigationContextServiceMethod.Invoke(null, new object[] { parameters[1] });

                object contextItems = viewContext.GetType().GetProperty("ContextItems").GetGetMethod().Invoke(viewContext, null);
                object sccmResultItemNode = contextItems.GetType().GetProperty("Item").GetValue(contextItems, new object[] { 0 });

                object scopeNode = sccmResultItemNodeType.GetProperty("ScopeNode").GetGetMethod().Invoke(sccmResultItemNode, null);
                Uri location = (Uri)scopeNode.GetType().GetProperty("Location").GetGetMethod().Invoke(scopeNode, null);

                while (scopeNode.GetType().GetProperty("ResultObject").GetGetMethod().Invoke(scopeNode, null) != null)
                {
                    scopeNode = scopeNode.GetType().GetProperty("Parent").GetGetMethod().Invoke(scopeNode, null);
                    location = (Uri)scopeNode.GetType().GetProperty("Location").GetGetMethod().Invoke(scopeNode, null);
                }

                return location.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}


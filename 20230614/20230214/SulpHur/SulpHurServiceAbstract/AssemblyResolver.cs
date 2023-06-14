using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace SulpHurServiceAbstract
{
    public static class AssemblyResolver
    {
        private static List<string> binSearchPaths = new List<string>();

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in loadedAssemblies)
                if (0 == StringComparer.InvariantCultureIgnoreCase.Compare(assembly.FullName, args.Name))
                    return assembly;

            AssemblyName assemblyName = new AssemblyName(args.Name);
            string dllName = assemblyName.Name + ".dll";
            string exeName = assemblyName.Name + ".exe";
            for (int n = 0; n < binSearchPaths.Count; n++)
            {
                string dllPath = Path.Combine(binSearchPaths[n], dllName);
                string exePath = Path.Combine(binSearchPaths[n], exeName);
                if (File.Exists(dllPath))
                    return Assembly.LoadFrom(dllPath);
                if (File.Exists(exePath))
                    return Assembly.LoadFrom(exePath);
            }

            return null;
        }

        public static void Install()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
        }

        public static void Uninstall()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(AssemblyResolve);
        }

        public static void AddSearchPath(string path)
        {
            if (!path.EndsWith("\\"))
                path = path + "\\";

            if (!binSearchPaths.Contains(path))
                binSearchPaths.Add(path);
        }

        public static void ClearSearchPath()
        {
            binSearchPaths.Clear();
        }
    }
}

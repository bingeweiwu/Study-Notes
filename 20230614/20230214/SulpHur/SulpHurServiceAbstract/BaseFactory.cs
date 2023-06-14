using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace SulpHurServiceAbstract
{
    public abstract class BaseFactory
    {
        public static object Instance(string AssemblySettingName, string ClassNameSettingName, string DefaultAssemblyName, string DefaultClassName)
        {
            object instance = null;
            string assembly = "";
            string className = "";

            assembly = ConfigurationManager.AppSettings[AssemblySettingName];
            className = ConfigurationManager.AppSettings[ClassNameSettingName];
            if (assembly == null || className == null)
            {
                assembly = DefaultAssemblyName;
                className = DefaultClassName;
            }

            try
            {
                AssemblyResolver.Install();
                //AssemblyResolver.AddSearchPath(Environment.CurrentDirectory);
                //AssemblyResolver.AddSearchPath(System.AppDomain.CurrentDomain.BaseDirectory);
                Assembly ass = Assembly.LoadFrom(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, assembly));
                instance = ass.CreateInstance(className);
                AssemblyResolver.Uninstall();
                return instance;
            }
            catch (FileNotFoundException e)
            {

                throw new FileNotFoundException("File not found: " + System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, assembly));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

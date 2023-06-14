using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace MS.Internal.SulpHur.SulpHurService
{
    public class ServiceEnviroment
    {
        public static string RULEFOLDER = string.Empty;
        public static string _WinSERVICENAME = "SulpHurService";
        public static string _SERVERSHOWNAME = "NoneVisible";
    }

    #region ReadConfigurations
    public class SLSection : ConfigurationSection
    {
        [ConfigurationProperty("SLItems", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(SLConfigurationCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public SLConfigurationCollection SLItems
        {
            get
            {
                SLConfigurationCollection slItemsCollection =
                    (SLConfigurationCollection)base["SLItems"];
                return slItemsCollection;
            }
        }

    }

    public class SLConfigurationCollection : ConfigurationElementCollection
    {
        public SLConfigurationCollection()
        {
            SLConfigElement slItem = (SLConfigElement)CreateNewElement();
            Add(slItem);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SLConfigElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((SLConfigElement)element).Name;
        }

        public SLConfigElement this[int index]
        {
            get
            {
                return (SLConfigElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public SLConfigElement this[string Name]
        {
            get
            {
                return (SLConfigElement)BaseGet(Name);
            }
        }

        public int IndexOf(SLConfigElement url)
        {
            return BaseIndexOf(url);
        }

        public void Add(SLConfigElement url)
        {
            BaseAdd(url);
        }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(SLConfigElement value)
        {
            if (BaseIndexOf(value) >= 0)
                BaseRemove(value.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }
    }

    public class SLConfigElement : ConfigurationElement
    {
        public SLConfigElement(String name, String value)
        {
            this.Name = name;
            this.Value = value;
        }

        public SLConfigElement()
        {
            this.Name = "Item1";
            this.Value = "Value1";
        }

        [ConfigurationProperty("name", DefaultValue = "Name1",
            IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("value", DefaultValue = "",
            IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }
    }

    public class SLConfigurationReader
    {
        /// <summary>
        /// <?xml version="1.0" encoding="utf-8"?>
        ///<configuration>
        ///  <configSections>
        ///    <section name="CustomConfigs" type="MS.Internal.SulpHur.SulpHurService.SLSection, MS.Internal.SulpHur.SulpHurService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
        ///  </configSections>
        ///  <CustomConfigs>
        ///    <SLItems>
        ///      <remove name="Item1"></remove>
        ///     <add name="MyItem" value="xxx" />
        ///    </SLItems>
        ///  </CustomConfigs>
        ///</configuration>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ReadCustomSection(string name)
        {
            try
            {
                // Get the application configuration file.
                System.Configuration.Configuration config =
                        ConfigurationManager.OpenExeConfiguration(
                        ConfigurationUserLevel.None) as Configuration;

                // Read and display the custom section.
                SLSection myCustomSection =
                   ConfigurationManager.GetSection("CustomConfigs") as SLSection;

                if (myCustomSection == null)
                    Log.WriteServerLog("Failed to load CustomConfigs Section.",TraceLevel.Warning);
                else
                {
                    for (int i = 0; i < myCustomSection.SLItems.Count; i++)
                    {
                        if (myCustomSection.SLItems[i].Name.Equals(name))
                        {
                            return myCustomSection.SLItems[i].Value;
                        }
                    }
                }
                return null;
            }
            catch (ConfigurationErrorsException err)
            {
                Log.WriteServerLog(string.Format("ReadCustomSection(string): {0}", err.ToString()),TraceLevel.Error);
                return null;
            }
        }
    }
    #endregion


    //public class StartupConfigSection : ConfigurationSection
    //{

    //    [System.Configuration.ConfigurationProperty("Folders")]
    //    public FolderElement FolderItem
    //    {
    //        get
    //        {
    //            return ((FolderElement)(base["Folders"]));
    //        }
    //    }

    //    [System.Configuration.ConfigurationProperty("ServerName")]
    //    public ServerName ServerNameItem
    //    {
    //        get
    //        {
    //            return ((ServerName)(base["ServerName"]));
    //        }
    //    }

    //    [System.Configuration.ConfigurationProperty("VerifyThreadItem")]
    //    public VerifyThreadItem VerifyThreadItem
    //    {
    //        get
    //        {
    //            return ((VerifyThreadItem)(base["VerifyThreadItem"]));
    //        }
    //    }
    //}

    ///// <summary>
    ///// The class that holds onto each element returned by the configuration manager.
    ///// </summary>
    //public class FolderElement : ConfigurationElement
    //{
    //    //[ConfigurationProperty("sulpHurfolder", IsRequired = true)]
    //    //public string SulpHurFolder
    //    //{
    //    //    get
    //    //    {
    //    //        return ((string)(base["sulpHurfolder"]));
    //    //    }

    //    //    set
    //    //    {
    //    //        base["sulpHurfolder"] = value;
    //    //    }
    //    //}

    //    [ConfigurationProperty("rulefolder", IsRequired = true)]
    //    public string RuleFolder
    //    {
    //        get
    //        {
    //            return ((string)(base["rulefolder"]));
    //        }

    //        set
    //        {
    //            base["rulefolder"] = value;
    //        }
    //    }
    //}

    ///// <summary>
    ///// Config a server name show to user
    ///// </summary>
    //public class ServerName : ConfigurationElement 
    //{
    //    [ConfigurationProperty("showname", IsRequired = true)]
    //    public string ShowName
    //    {
    //        get
    //        {
    //            return ((string)(base["showname"]));
    //        }

    //        set
    //        {
    //            base["showname"] = value;
    //        }
    //    }
    //}

    //public class VerifyThreadItem : ConfigurationElement {
    //    [ConfigurationProperty("isenalbe", IsRequired = true)]
    //    public bool IsEnable
    //    {
    //        get
    //        {
    //            return ((bool)(base["isenalbe"]));
    //        }

    //        set
    //        {
    //            base["isenalbe"] = value;
    //        }
    //    }
    //}
}

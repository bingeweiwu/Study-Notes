using RenameLog;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MS.Internal.SulpHur.SulpHurClient.GenerateClass
{
    public class CreateClass
    {
        private static string path = @"C:\SulpHurClient\UIAutoData";
        private static string CSName = ".cs";
        static ArrayList Nodelist = new ArrayList();
        static List<xmlClass> xmlclassList = new List<xmlClass>();
        //private string testpath = @"C:\Users\haida\Desktop\xml";
        private static string deskpath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static void XmlForm(XmlNode root, string node)
        {
            try
            {
                XmlNodeList rootchild = root.ChildNodes;

                foreach (XmlNode element in rootchild)
                {
                    if (element.NodeType == XmlNodeType.Text && root.Name == node)
                    {
                        XmlNode parentNode;
                        string res2 = "";
                        string pid = "";
                        parentNode = findParentNodeByName(root, node);
                        if (parentNode != null)
                        {
                            pid = parentNode.InnerText;
                            res2 = "    Parent ID:" + pid;
                        }
                        string res = "AutomationId:" + element.InnerText + res2;
                        Nodelist.Add(res);
                        xmlClass x = new xmlClass(element.InnerText, pid);
                        xmlclassList.Add(x);
                        continue;
                    }

                    if (element.HasChildNodes)
                    {
                        XmlForm(element, node);
                    }

                }

            }
            catch (Exception)
            {

                //MessageBox.Show(ex.ToString());
            }
        }
        public static XmlNode findNodeByAutomationId(XmlNode root, String id)
        {
            XmlNodeList rootchild = findChildrensElement(root);
            XmlNode resnode = null;
            foreach (XmlNode node in rootchild)
            {
                if (resnode != null)
                {
                    break;
                }
                if (node.SelectSingleNode("AutomationId") != null)
                {
                    if (node.SelectSingleNode("AutomationId").InnerText == id)
                    {
                        resnode = node; break;
                    }
                }
                if (findChildrensElement(node) != null && findChildrensElement(node).Count != 0)
                {
                    resnode = findNodeByAutomationId(node, id);
                }
            }
            return resnode;
        }

        public static XmlNodeList findChildrensElement(XmlNode root)
        {
            //return root.SelectSingleNode("Children").SelectNodes("ElementInformation");
            XmlNodeList list = null;
            XmlNode childNode = root.SelectSingleNode("Children");
            if (childNode != null)
                list = childNode.SelectNodes("ElementInformation");
            return list;
        }

        public static XmlNode findParentNodeByName(XmlNode node, string name)
        {
            try
            {
                XmlNode prentNode = node.ParentNode.ParentNode.ParentNode.SelectSingleNode(name);
                return prentNode;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public ArrayList NodeList(string xmlPath, string node)
        {
            ArrayList nodelist = null;
            using (XmlReader reader = XmlReader.Create(xmlPath))
            {
                while (reader.Read())
                {
                    if (reader.Name == node && reader.NodeType == XmlNodeType.Element)
                        nodelist.Add(reader.ReadInnerXml());
                }
            }
            return nodelist;
        }
        public static bool IsSelectNode(string xmlPath, string node)
        {
            using (XmlReader reader = XmlReader.Create(xmlPath))
            {
                while (reader.Read())
                {
                    if (reader.Name == node && reader.NodeType == XmlNodeType.Element)
                        return true;
                }
            }
            return false;
        }
        public static bool IsSelectNodeDoc(string xmlPath, string node)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlNode result = xmlDoc.SelectSingleNode(@"//" + node);
            return result != null;
        }

        public static void NormalForm(XmlNode root)
        {
            //XmlForm(root, "AutomationId");
            CreateClassByNode(root, "AutomationId");
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="fileName">文件完整路径</param>
        /// <param name="content">内容</param>
        private static void WriteAndSave(string fileName)
        {
            //实例化一个文件流--->与写入文件相关联
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                //实例化一个StreamWriter-->与fs相关联
                using (var sw = new StreamWriter(fs))
                {
                    foreach (string con in Nodelist)
                    {
                        sw.WriteLine(con);
                    }
                    sw.Flush();
                    //关闭流
                    sw.Close();
                    fs.Close();
                    Nodelist.Clear();
                }
            }
        }

        public static XmlNode getRootIDNode(XmlNode root)
        {
            foreach (XmlNode item in root.ChildNodes)
            {
                if (item.Name == "AutomationId")
                {
                    return root;
                }
                getRootIDNode(item);
            }

            return null;
        }

        public static bool hasChild(xmlClass x)
        {
            List<int> record = new List<int>();
            for (int i = 0; i < xmlclassList.Count; i++)
            {
                if (xmlclassList[i].ParentID == x.ID)
                {
                    record.Add(i);
                }
            }
            if (record.Count > 1)
            {
                return true;
            }
            else if (record.Count == 0)
            {
                return false;
            }
            else
            {
                if (xmlclassList[record[0]].ID == "treeViewItemImage")
                {
                    return false;
                }
                return true;
            }

        }

        public static CodeTypeDeclaration getParentClass(ClassGenerator gen, xmlClass x)
        {

            return gen.GetClassByName(Getlaststring(x.ParentID));
        }

        public static void newClassOnTargetClass(ClassGenerator gen, xmlClass x, CodeTypeDeclaration targetclass)
        {
            string varname = Getlaststring(x.ID);
            gen.CreateNewCassAndFieldOnTargetClass(gen, firstUpper(varname), firstLower(varname), "Session", x.ID, targetclass);
        }
        public static string firstLower(string name)
        {
            return name.Substring(0, 1).ToString().ToLower() + name.Substring(1);
        }
        public static string firstUpper(string name)
        {

            return name.Substring(0, 1).ToString().ToUpper() + name.Substring(1);
        }
        public static void addVarOnTargetClass(ClassGenerator gen, xmlClass x, CodeTypeDeclaration targetclass)
        {
            string varname = Getlaststring(x.ID);
            gen.AddProperties(firstUpper(varname), x.ID, "Session", targetclass);
        }

        public static string Getlaststring(string str)
        {
            str = str.Replace(" ", "_");
            str = Regex.Replace(str, "[.-]", "_");
            return str.Substring(str.LastIndexOf("/", StringComparison.Ordinal) + 1);
        }
        public static void CreateClassByNode(XmlNode node, string id)
        {
            XmlForm(node, id);
            ClassGenerator gen = new ClassGenerator(Getlaststring(xmlclassList[0].ID));
            for (int i = 1; i < xmlclassList.Count; i++)
            {
                CodeTypeDeclaration p = getParentClass(gen, xmlclassList[i]);
                if (p != null)
                {
                    if (!hasChild(xmlclassList[i]))
                    {
                        addVarOnTargetClass(gen, xmlclassList[i], p);
                    }
                    else
                    {
                        newClassOnTargetClass(gen, xmlclassList[i], p);
                    }
                }
            }
            gen.GenerateCSharpCode(path + "\\" + gen.targetClass.Name + CSName);
            xmlclassList.Clear();
        }
    }
}

using MS.Internal.SulpHur.UICompliance;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace XmlExchange
{
    public class Tool
    {
        /// <summary>
        /// 将xml转换为nodlist
        /// </summary>
        /// <param name="xElement"></param>
        /// <returns></returns>
        public static List<ElementInformation> ParseTreeToList(System.Xml.Linq.XElement xElement)
        {
            ElementInformation eiRoot = FromXElement<ElementInformation>(xElement);
            eiRoot.treeLevel = 0;

            List<ElementInformation> infoList = new List<ElementInformation>();
            infoList.Add(eiRoot);

            AddChild(eiRoot, infoList);

            foreach (ElementInformation ei in infoList)
            {
                ei.Descendants = new List<ElementInformation>();
                ei.Ancestors = new List<ElementInformation>();
            }

            foreach (ElementInformation ei in infoList)
            {
                AddDescents(ei, ei.Descendants);
                AddAncestors(ei, ei.Ancestors);
            }
            return infoList;
        }

        private static T FromXElement<T>(XElement xElement)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xElement.ToString())))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(memoryStream);
            }
        }

        private static void AddChild(ElementInformation ei, List<ElementInformation> infoList)
        {
            if (ei.Children == null) return;
            if (ei.Children.Count > 0)
            {
                ei.FirstChild = ei.Children.First();
                ei.LastChild = ei.Children.Last();
            }

            for (int i = 0; i < ei.Children.Count; i++)
            {
                ElementInformation temp = ei.Children[i];
                temp.Parent = ei;
                if (temp.NativeWindowHandle != 0 || temp.ControlType == ControlType.Hyperlink)
                {
                    //This attribute not used current time
                    //temp.treeLevel = ei.treeLevel + 1;
                    AddSibling(i, temp, ei.Children);
                    infoList.Add(temp);
                    AddChild(temp, infoList);
                }
            }
        }
        internal static void AddDescents(ElementInformation ei, List<ElementInformation> descents)
        {
            foreach (ElementInformation temp in ei.Children)
            {
                descents.Add(temp);
                AddDescents(temp, descents);
            }
        }

        internal static void AddAncestors(ElementInformation ei, List<ElementInformation> ancestors)
        {
            if (ei.Parent == null) return;
            else
            {
                ancestors.Add(ei.Parent);
                AddAncestors(ei.Parent, ancestors);
            }
        }

        internal static void AddSibling(int current, ElementInformation currentEi, List<ElementInformation> siblingCollection)
        {
            for (int i = 0; i < siblingCollection.Count; i++)
            {
                if (i != current)
                {
                    if (currentEi.Siblings == null) currentEi.Siblings = new List<ElementInformation>();
                    currentEi.Siblings.Add(siblingCollection[i]);
                }

                if (i == current - 1)
                {
                    if (currentEi.PreviousSibling == null) currentEi.PreviousSibling = new ElementInformation();
                    currentEi.PreviousSibling = siblingCollection[i];
                }

                if (i == current + 1)
                {
                    if (currentEi.NextSibling == null) currentEi.NextSibling = new ElementInformation();
                    currentEi.NextSibling = siblingCollection[i];
                }
            }
        }

        public static string ChangeToNameString(string s)
        {
            string result = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (Regex.IsMatch(s[i].ToString(),"[a-zA-Z0-9_]"))
                {
                    result+=s[i];
                    continue;
                }
                else if (Regex.IsMatch(s[i].ToString()," "))
                {
                    continue;
                }
                result += "_";
            }
            return result;
        }
    }
}

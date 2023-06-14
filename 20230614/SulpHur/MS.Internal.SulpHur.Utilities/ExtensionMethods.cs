using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using System.Xml.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIA3 = interop.UIAutomationCore;

namespace MS.Internal.SulpHur.Utilities
{
    public static class ExtensionMethods
    {
        public static XElement ToXElement<T>(this object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter streamWriter = new StreamWriter(memoryStream))
                {
                    var xmlSerializer = new XmlSerializer(typeof(T));
                    xmlSerializer.Serialize(streamWriter, obj);
                    return XElement.Parse(Encoding.UTF8.GetString(memoryStream.ToArray()));
                }
            }
        }
        public static T FromXElement<T>(this XElement xElement)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xElement.ToString())))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(memoryStream);
            }
        }

        public static T FromString<T>(this string srcString)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(srcString)))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(memoryStream);
            }
        }

        public static string ToRuntimeIdString(this int[] intArr)
        {
            string retValue = string.Empty;

            //convert
            foreach (int i in intArr)
            {
                retValue += i.ToString() + ".";
            }
            //trim end
            if (retValue.Length > 0)
            {
                retValue = retValue.Remove(retValue.Length - 1, 1);
            }

            return retValue;
        }

        public static System.Windows.Rect ToWinRect(this UIA3.tagRECT uia3Rect)
        {
            System.Windows.Rect winRect = new System.Windows.Rect()
            {
                X = uia3Rect.left,
                Y = uia3Rect.top,
                Width = uia3Rect.right - uia3Rect.left,
                Height = uia3Rect.bottom - uia3Rect.top
            };

            return winRect;
        }
        public static System.Drawing.Rectangle ToDrawingRect(this UIA3.tagRECT uia3Rect)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle()
            {
                X = uia3Rect.left,
                Y = uia3Rect.top,
                Width = uia3Rect.right - uia3Rect.left,
                Height = uia3Rect.bottom - uia3Rect.top
            };

            return rect;
        }
        public static bool ToBool(this int value)
        {
            return Convert.ToBoolean(value);
        }
        public static System.Windows.Automation.OrientationType ToUia2OrientationType(this UIA3.OrientationType uia3OrientationType)
        {
            System.Windows.Automation.OrientationType orientationType = OrientationType.None;

            switch (uia3OrientationType)
            { 
                case UIA3.OrientationType.OrientationType_Horizontal:
                    orientationType = OrientationType.Horizontal;
                    break;
                case UIA3.OrientationType.OrientationType_Vertical:
                    orientationType = OrientationType.Vertical;
                    break;
                default:
                    orientationType = OrientationType.None;
                    break;
            }

            return orientationType;
        }
    }

}

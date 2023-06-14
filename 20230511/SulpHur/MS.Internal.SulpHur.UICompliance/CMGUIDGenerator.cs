using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Diagnostics;

namespace MS.Internal.SulpHur.UICompliance
{
    public class CMGUIDGenerator
    {
        string text = "";
        System.Windows.Rect root;

        public Guid GenerateGUID(ElementInformation ei, string language)
        {
            //text = "";
            root = new System.Windows.Rect(ei.BoundingRectangle.X, ei.BoundingRectangle.Y, ei.BoundingRectangle.Width, ei.BoundingRectangle.Height);

            //ElementInformation eiforGUID = null;

            //this logic causes the tootip control to be excluded
            //if (ei.AutomationId == "SmsWizardForm")
            //{
            //    eiforGUID = FindElementByID(ei, "WizardPage");
            //}
            //else if (ei.AutomationId == "SheetFramework")
            //{
            //    eiforGUID = FindElementByID(ei, "tabPages");
            //}
            //if (eiforGUID == null) eiforGUID = ei;
            //AddString(eiforGUID);

            AddString(ei);
            Guid guid = GetGuidFromString(text + language);
            //Trace.WriteLine($"生成guid使用的字符串为: {text + language},guid={guid}");

            return guid;
        }

        public Guid GenerateGUID(XElement xe, string language)
        {
            ElementInformation eiRoot = MS.Internal.SulpHur.Utilities.ExtensionMethods.FromXElement<ElementInformation>(xe);
            return GenerateGUID(eiRoot, language);
        }

        private void AddString(ElementInformation ei)
        {
            //ignore menu & tooltip 
            //if (ei.ControlType.Equals(ControlType.ToolTip) ||
            //    ei.ControlType.Equals(ControlType.Menu))
            //{
            //    return;
            //}

            //miss app name lable in App Revision History Dialog
            if (ei.AutomationId.Equals("labelAppName") && ei.ControlType.Equals(ControlType.Text))
            {
                return;
            }

            //miss container & miss error icon
            if (ei.ControlType.Equals(ControlType.Pane))
            {
                //Do nothing
            }
            else
            {
                //忽略掉tooltip的text子控件,此代码块无效
                //if (ei.ControlType.Equals(ControlType.Text) && ei.Parent.ControlType.Equals(ControlType.ToolTip))
                //{
                //    Trace.WriteLine("忽略掉tooltip的text子控件");
                //    return;
                //}
                //current element
                System.Windows.Rect rect = ei.BoundingRectangle;
                rect.Offset(-root.X, -root.Y);
                //ei.BoundingRectangle = rect;
                string accessKey = ei.AccessKey;
                if (string.IsNullOrEmpty(accessKey))
                {
                    accessKey = "";
                }

                string temp = string.Empty;
                if (!string.IsNullOrEmpty(ei.Name))
                {
                    //如果控件是气泡，生成guid忽略其位置信息
                    if (!ei.ControlType.Equals(ControlType.ToolTip) &&
                        //气泡可能还有父控件（window），忽略
                        !(ei.ControlType.Equals(ControlType.Window) && ei.FirstChild != null && ei.FirstChild.ControlType.Equals(ControlType.ToolTip))

                        )
                    {
                        temp += rect.X.ToString() + rect.Y.ToString() + rect.Width.ToString() + rect.Height.ToString() + ei.TabIndex.ToString() + accessKey;
                    }
                }
                temp += $" {ei.Name}";
                temp += $" {ei.ControlType} ";
                text += temp;

                //气泡可能还有子控件（text），忽略
                if (ei.ControlType.Equals(ControlType.ToolTip))
                {
                    return;
                }
            }

            //children
            //miss data elements
            //if(ei.ControlType.Equals(ControlType.DataGrid) ||
            //    ei.ControlType.Equals(ControlType.Tree) ||
            //    //ei.ControlType.Equals(ControlType.ComboBox) ||
            //    ei.ControlType.Equals(ControlType.Table) ||
            //    ei.ControlType.Equals(ControlType.ScrollBar))
            //{
            //    return;
            //}

            //current element children
            for (int i = 0; i < ei.Children.Count; i++)
            {
                AddString(ei.Children[i]);
            }
        }

        public Guid GetGuidFromString(string textToHash)
        {
            byte[] hashValue;

            SHA1 hash;
            bool isFipsEnforcementEnabled = false;
            // On a system with FIPS compliance set it isn't possible to create
            // an instance of SHA1Managed.  If this is the case we will use the
            // SHA1CryptoServiceProvider provider class instead.
            if (!isFipsEnforcementEnabled)
            {
                try
                {
                    hash = new SHA1Managed();
                }
                catch (InvalidOperationException)
                {
                    // mark that FIPS enforcement is enabled.
                    isFipsEnforcementEnabled = true;

                    hash = new SHA1CryptoServiceProvider();
                }
            }
            else
            {
                hash = new SHA1CryptoServiceProvider();
            }

            using (hash)
            {
                UnicodeEncoding encoding = new UnicodeEncoding();
                byte[] dataToHash;

                if (textToHash == null)
                {
                    dataToHash = encoding.GetBytes("<null>".ToString());
                }
                else
                {
                    dataToHash = encoding.GetBytes(textToHash.ToString());
                }
                hashValue = hash.ComputeHash(dataToHash);
            }

            // We can't use the byte[] constructor of guid expects a 16 byte
            // array.  Since SHA1 generates a 20 byte hash which we are
            // truncating we need to use a constructor which lets us pass in
            // only the part of the array we are using.
            return new Guid(
                (int)hashValue[3] << 24 | (int)hashValue[2] << 16 | (int)hashValue[1] << 8 | hashValue[0],
                (short)((int)hashValue[5] << 8 | hashValue[4]),
                (short)((int)hashValue[7] << 8 | hashValue[6]),
                hashValue[8],
                hashValue[9],
                hashValue[10],
                hashValue[11],
                hashValue[12],
                hashValue[13],
                hashValue[14],
                hashValue[15]);
        }

        private ElementInformation FindElementByID(ElementInformation ei, string id)
        {

            Queue<ElementInformation> queue = new Queue<ElementInformation>();
            queue.Enqueue(ei);
            ElementInformation eiTemp = null;
            while (queue.Count > 0)
            {
                eiTemp = queue.Dequeue();
                if (eiTemp.AutomationId == id)
                {
                    break;
                }
                if (eiTemp.Children != null)
                {
                    foreach (ElementInformation e in eiTemp.Children)
                    {
                        queue.Enqueue(e);
                    }
                }
            }
            return eiTemp;
        }
    }
}

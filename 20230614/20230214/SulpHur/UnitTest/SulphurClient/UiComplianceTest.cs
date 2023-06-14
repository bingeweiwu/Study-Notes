using Microsoft.VisualStudio.TestTools.UnitTesting;
using MS.Internal.SulpHur.SulpHurClient.Common;
using MS.Internal.SulpHur.UICompliance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.SulphurClient
{
    [TestClass]
    public class UiComplianceTest
    {
        [TestMethod]
        public void CMGUIDGenerator_GenerateGUID()
        {
            /*
             无气泡

[生成guid使用的字符串为: 005006000 Configuration Manager Notifications Window 9324825590  Custom 173636160 2 Total Text 9604825310 2 Total List 11624781120 Dismissible notification. Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. ListItem 176816160 Dismissible notification. Image 4568399480 Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. Text 4606824240 Dismiss Button 4112852160 More info Text 4112852160 More info Hyperlink 4112852160 More info Text 4115252160 1 day ago Text 46012824240 Options Button 11174478960 Dismissible notification. New custom console extensions are available:
    WebView2 extension ListItem 1718016160 Dismissible notification. Image 45180241320 New custom console extensions are available:
    WebView2 extension Text 46018024240 Dismiss Button 41224176160 Install custom console extensions Text 41224176160 Install custom console extensions Hyperlink 41224176160 Install custom console extensions Text 4124852160 1 day ago Text 46022424240 Options Button ENU,guid=a3422a35-dc36-5b8c-8bb9-a07b344f1465]

气泡1
[生成guid使用的字符串为: 005006000 Configuration Manager Notifications Window -63157972280  Window 9324825590  Custom 173636160 2 Total Text 9604825310 2 Total List 11624781120 Dismissible notification. Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. ListItem 176816160 Dismissible notification. Image 4568399480 Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. Text 4606824240 Dismiss Button 4112852160 More info Text 4112852160 More info Hyperlink 4112852160 More info Text 4115252160 1 day ago Text 46012824240 Options Button 11174478960 Dismissible notification. New custom console extensions are available:
    WebView2 extension ListItem 1718016160 Dismissible notification. Image 45180241320 New custom console extensions are available:
    WebView2 extension Text 46018024240 Dismiss Button 41224176160 Install custom console extensions Text 41224176160 Install custom console extensions Hyperlink 41224176160 Install custom console extensions Text 4124852160 1 day ago Text 46022424240 Options Button ENU,guid=1c99fd4b-e7b0-f5a3-ea23-05c61967bf92]

气泡2
[生成guid使用的字符串为: 005006000 Configuration Manager Notifications Window -63154972280  Window 9324825590  Custom 173636160 2 Total Text 9604825310 2 Total List 11624781120 Dismissible notification. Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. ListItem 176816160 Dismissible notification. Image 4568399480 Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. Text 4606824240 Dismiss Button 4112852160 More info Text 4112852160 More info Hyperlink 4112852160 More info Text 4115252160 1 day ago Text 46012824240 Options Button 11174478960 Dismissible notification. New custom console extensions are available:
    WebView2 extension ListItem 1718016160 Dismissible notification. Image 45180241320 New custom console extensions are available:
    WebView2 extension Text 46018024240 Dismiss Button 41224176160 Install custom console extensions Text 41224176160 Install custom console extensions Hyperlink 41224176160 Install custom console extensions Text 4124852160 1 day ago Text 46022424240 Options Button ENU,guid=55854a23-392a-5a2d-4f35-40cec0f51845]

             */
            string guidstring1 = "005006000 Configuration Manager Notifications Window 9324825590  Custom 173636160 2 Total Text 9604825310 2 Total List 11624781120 Dismissible notification. Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. ListItem 176816160 Dismissible notification. Image 4568399480 Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. Text 4606824240 Dismiss Button 4112852160 More info Text 4112852160 More info Hyperlink 4112852160 More info Text 4115252160 1 day ago Text 46012824240 Options Button 11174478960 Dismissible notification. New custom console extensions are available:    WebView2 extension ListItem 1718016160 Dismissible notification. Image 45180241320 New custom console extensions are available:    WebView2 extension Text 46018024240 Dismiss Button 41224176160 Install custom console extensions Text 41224176160 Install custom console extensions Hyperlink 41224176160 Install custom console extensions Text 4124852160 1 day ago Text 46022424240 Options Button";

            string guidstring2 = "005006000 Configuration Manager Notifications Window 63157972280  Window 9324825590  Custom 173636160 2 Total Text 9604825310 2 Total List 11624781120 Dismissible notification. Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. ListItem 176816160 Dismissible notification. Image 4568399480 Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. Text 4606824240 Dismiss Button 4112852160 More info Text 4112852160 More info Hyperlink 4112852160 More info Text 4115252160 1 day ago Text 46012824240 Options Button 11174478960 Dismissible notification. New custom console extensions are available:    WebView2 extension ListItem 1718016160 Dismissible notification. Image 45180241320 New custom console extensions are available:    WebView2 extension Text 46018024240 Dismiss Button 41224176160 Install custom console extensions Text 41224176160 Install custom console extensions Hyperlink 41224176160 Install custom console extensions Text 4124852160 1 day ago Text 46022424240 Options Button";

            string guidstring3 = "005006000 Configuration Manager Notifications Window 63154972280  Window 9324825590  Custom 173636160 2 Total Text 9604825310 2 Total List 11624781120 Dismissible notification. Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. ListItem 176816160 Dismissible notification. Image 4568399480 Get a consolidated view of all your devices in a single cloud console. Enable tenant attach by choosing 'Upload to Microsoft Endpoint Manager admin center'. Text 4606824240 Dismiss Button 4112852160 More info Text 4112852160 More info Hyperlink 4112852160 More info Text 4115252160 1 day ago Text 46012824240 Options Button 11174478960 Dismissible notification. New custom console extensions are available:    WebView2 extension ListItem 1718016160 Dismissible notification. Image 45180241320 New custom console extensions are available:    WebView2 extension Text 46018024240 Dismiss Button 41224176160 Install custom console extensions Text 41224176160 Install custom console extensions Hyperlink 41224176160 Install custom console extensions Text 4124852160 1 day ago Text 46022424240 Options Button";

            CMGUIDGenerator generator = new CMGUIDGenerator();
            List<Guid> list = new List<Guid>();
            list.Add(generator.GetGuidFromString(guidstring1));
            list.Add(generator.GetGuidFromString(guidstring2));
            list.Add(generator.GetGuidFromString(guidstring3));
            list.Add(generator.GetGuidFromString(guidstring1));
            list.Add(generator.GetGuidFromString(guidstring2));
            list.Add(generator.GetGuidFromString(guidstring3));
            list.Add(generator.GetGuidFromString(guidstring3));
            list.Add(generator.GetGuidFromString(guidstring3));
            list.Add(generator.GetGuidFromString(guidstring3));

            bool b11 = guidstring1.Replace(" ", "").ToLower().Equals(guidstring2.Replace(" ", "").ToLower());
            bool b13 = guidstring1.Replace(" ", "").ToLower().Equals(guidstring3.Replace(" ", "").ToLower());
            bool b23 = guidstring2.Replace(" ", "").ToLower().Equals(guidstring3.Replace(" ", "").ToLower());

            string s2 = guidstring2.ToLower().Replace(" ", "");
            string s3 = guidstring3.ToLower().Replace(" ", "");

            StringBuilder builder = new StringBuilder();
            //63157972280位置信息不同，造成guid生成不一样，是window控件
            for (int i = 0; i < guidstring2.Length; i++)
            {
                if (guidstring2[i] != guidstring3[i])
                {
                    builder.Append(guidstring2[i].ToString());
                }
            }

        }

        [TestMethod]
        public void TestRunbat()
        {
            Tool.RunBat("AccessServer.bat");
        }
    }
}

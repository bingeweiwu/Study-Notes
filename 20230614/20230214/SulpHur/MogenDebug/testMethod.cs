using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MS.Internal.SulpHur.CMRules;
using MS.Internal.SulpHur.SulpHurService;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Diagnostics;
using MS.Internal.SulpHur.UICompliance;
using System.Xml.Schema;
using System.Reflection;
using System.CodeDom.Compiler;

namespace MogenDebug
{
    public static class testMethod
    {
        public static Queue<UIANDRule> dirtyUIQueue = new Queue<UIANDRule>();
        static List<UIComplianceRuleBase> ruleList = new List<UIComplianceRuleBase>();


        public static void Rule(int resultID)
        {
            ZMogenDebugSulpHurEntities entities = new ZMogenDebugSulpHurEntities();
            var varList = (from u in entities.UIContents
                           join r in entities.Results on u.ContentID equals r.ContentID
                           where r.ResultID == resultID
                           select new UIANDRule()
                           {
                               Bitmap = new Bitmap(new MemoryStream(u.UIScreenShot)),
                               Element = XElement.Parse(u.UIContent1)

                           });
            UIANDRule uiandrule = varList.FirstOrDefault();
            UIANDRule test = new UIANDRule();
            test.Element = varList.FirstOrDefault().Element;
            test.Bitmap = varList.FirstOrDefault().Bitmap;

        }

        /// <summary>
        /// use assembly 
        /// </summary>
        /// <param name="resultID"></param>
        public static void TestAllRule(int resultID)
        {

            ZMogenDebugSulpHurEntities entity = new ZMogenDebugSulpHurEntities();
            var varList = from u in entity.UIContents
                          join r in entity.Results on u.ContentID equals r.ContentID
                          join b in entity.BuildInfoes on u.BuildID equals b.BuildID
                          where r.ResultID == resultID
                          select new
                          {
                              //xmlContent = XElement.Parse(u.UIContent1),
                              language = b.Language,
                              xmlContent = u.UIContent1,
                              screenshot = u.UIScreenShot
                          };
            var obj = varList.FirstOrDefault();
            Bitmap bitmap = null;
            using (MemoryStream ms = new MemoryStream(obj.screenshot))
            {
                bitmap = new Bitmap(ms);
            }

            ControlScreen.CurrentBit = bitmap;
            List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(XElement.Parse(obj.xmlContent));


            ElementInformation rootControl = CommonRuleUtility.GetRootElement(eiList);

            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            string[] assemblyFiles = Directory.GetFiles("F:\\SD\\ConfigMgr-Test\\src\\Tools\\SulpHur20\\SulpHur\\MS.Internal.SulpHur.CMRules\\bin\\Debug", "*.dll");
            Console.WriteLine("assemblyFiles.Count = " + assemblyFiles.Count());
            Assembly assembly = null;
            MS.Internal.SulpHur.SulpHurService.RuleManager ruleManager = new MS.Internal.SulpHur.SulpHurService.RuleManager();
            foreach (string ruleFile in assemblyFiles)
            {
                FileInfo fi = new FileInfo(ruleFile);
                if (!fi.Name.Equals(System.Configuration.ConfigurationManager.AppSettings["EnabledRuleDLL"])) continue;

                try
                {
                    assembly = Assembly.LoadFile(ruleFile);
                }
                catch (Exception ex)
                {
                }


                foreach (Type type in assembly.GetTypes())
                {

                    if (ZMogenDebug.Program.IsSubclassOf(type, typeof(UIComplianceRuleBase)))
                    {
                        try
                        {
                            UIComplianceRuleBase rule = (UIComplianceRuleBase)assembly.CreateInstance(type.FullName);
                            ruleList.Add(rule);
                        }
                        catch (Exception ex)
                        {
                            //Log.WriteServerLog("fail to create rule instance exception:" + ex.Message + ",type:" + type.Name, TraceLevel.Error);
                        }
                    }
                }
            }

            foreach (UIComplianceRuleBase rule in ruleList)
            {
                if (rule.IsEnabled)
                {
                    //Console.WriteLine(rule.Name + " isEnable " + rule.IsEnabled.ToString());]
                    if (rule.Name == "unlocalization Rule")
                        rule.lan = obj.language;
                    results = rule.UIVerify(eiList);
                    foreach (UIComplianceResultBase b in results)
                    {
                        Console.WriteLine(b.RuleName + " :" + b.Type.ToString());
                        Console.WriteLine();
                        if (b.Type.ToString().Equals("Fail"))
                        {
                            Console.WriteLine(b.Message);
                            Console.WriteLine();
                            Console.WriteLine();
                        }
                    }
                }
            }
        }



        public static void spellRule(int resultID)
        {

            //string[] line = File.ReadAllLines(@"C:\Users\v-mogenzhang\Desktop\resultID.txt");
            //Console.WriteLine(line[0]);
            //string temp = line[0];
            //int resultID = int.Parse(temp);


            //int resultID = 37461475;37174428
            ZMogenDebugSulpHurEntities entity = new ZMogenDebugSulpHurEntities();
            var varList = from u in entity.UIContents
                          join r in entity.Results on u.ContentID equals r.ContentID
                          where r.ResultID == resultID
                          select new
                          {
                              //xmlContent = XElement.Parse(u.UIContent1),
                              xmlContent = u.UIContent1,
                              screenshot = u.UIScreenShot
                          };
            var obj = varList.FirstOrDefault();
            Bitmap bitmap = null;
            using (MemoryStream ms = new MemoryStream(obj.screenshot))
            {
                bitmap = new Bitmap(ms);
            }

            ControlScreen.CurrentBit = bitmap;
            List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(XElement.Parse(obj.xmlContent));
            //UIANDRule temp2;
            //DBOperator dbOperator = new DBOperator();
            //List<UIANDRule> allDirtyUI = dbOperator. QueryAllDirtyUI();
            //lock (obj)
            //{
            //    foreach (UIANDRule temp3 in allDirtyUI)
            //    {
            //        dirtyUIQueue.Enqueue(temp3);
            //    }
            //}

            //lock (obj)
            //{
            //    temp2 = dirtyUIQueue.Dequeue();
            //}
            // List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(temp2.Element);

            //TabOrderRule rule = new TabOrderRule();

            ElementInformation rootControl = CommonRuleUtility.GetRootElement(eiList);

            // rule.lan = dbOperator.QuerylanbycontentID(temp2.Uiid);
            //Console.WriteLine("start ");
            //  Console.WriteLine("dbOperator.QuerylanbycontentID(temp2.Uiid) = " + dbOperator.QuerylanbycontentID(temp2.Uiid));
            //Console.WriteLine("stop");
            //Console.ReadKey();
            SpellingRule rule = new SpellingRule();
            List<UIComplianceResultBase> results = rule.UIVerify(eiList);

            string fileName = Environment.CurrentDirectory + @"\ResultImage.png";

            foreach (UIComplianceResultBase b in results)
            {
                //uncomment after debugging 
                // b.Image.Save(fileName);
                Console.WriteLine(b.Message);
            }


        }
        public static void tabOrder(int resultID)
        {
            //string[] line = File.ReadAllLines(@"C:\Users\v-mogenzhang\Desktop\resultID.txt");
            //Console.WriteLine(line[0]);
            //string temp = line[0];
            //int resultID = int.Parse(temp);

            //int resultID = 46583609;
            ZMogenDebugSulpHurEntities entity = new ZMogenDebugSulpHurEntities();
            var varList = from u in entity.UIContents
                          join r in entity.Results on u.ContentID equals r.ContentID
                          where r.ResultID == resultID
                          select new
                          {
                              //xmlContent = XElement.Parse(u.UIContent1),
                              xmlContent = u.UIContent1,
                              screenshot = u.UIScreenShot
                          };

            var obj = varList.FirstOrDefault();
            Bitmap bitmap = null;
            using (MemoryStream ms = new MemoryStream(obj.screenshot))
            {
                bitmap = new Bitmap(ms);
            }

            ControlScreen.CurrentBit = bitmap;
            List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(XElement.Parse(obj.xmlContent));
            TabOrderRule rule = new TabOrderRule();
            ElementInformation rootControl = CommonRuleUtility.GetRootElement(eiList);
            // Console.WriteLine("rootControl.Name = " + rootControl.Name);


            // Console.WriteLine("Start verify through original method 10 times "+DateTime.Now);

            List<UIComplianceResultBase> results = rule.UIVerify(eiList);


            Console.WriteLine("results.Count = " + results.Count);
            Console.WriteLine("results[0].Type = " + results[0].Type);
            Console.WriteLine("results[0].Message" + results[0].Message);

            // string fileName = @"C:\Users\v-mogenzhang\Desktop\ResultImage.png";
            string fileName = Environment.CurrentDirectory + @"\ResultImage.png";
            Console.WriteLine("fileName = " + fileName);
            foreach (UIComplianceResultBase b in results)
            {
                //  b.Image.Save(fileName);
            }

        }
        public static void unlocalizationRule(int resultID)
        {
           
            ZMogenDebugSulpHurEntities entity = new ZMogenDebugSulpHurEntities();
            var varList = from u in entity.UIContents
                          join r in entity.Results on u.ContentID equals r.ContentID
                          join b in entity.BuildInfoes on u.BuildID equals b.BuildID
                          where r.ResultID == resultID
                          select new
                          {
                              //xmlContent = XElement.Parse(u.UIContent1),
                              language = b.Language,
                              xmlContent = u.UIContent1,
                              screenshot = u.UIScreenShot
                          };
            var obj = varList.FirstOrDefault();
            Bitmap bitmap = null;
            using (MemoryStream ms = new MemoryStream(obj.screenshot))
            {
                bitmap = new Bitmap(ms);
            }

            ControlScreen.CurrentBit = bitmap;
            List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(XElement.Parse(obj.xmlContent));
            

            //ElementInformation rootControl = CommonRuleUtility.GetRootElement(eiList);


            UnlocalizationRule rule = new UnlocalizationRule();           
            rule.lan = obj.language;
            List<UIComplianceResultBase> results = rule.UIVerify(eiList);

            string fileName = Environment.CurrentDirectory + @"\ResultImage.png";

            foreach (UIComplianceResultBase b in results)
            {
                //uncomment after debugging 
                // b.Image.Save(fileName);
                Console.WriteLine(b.Message);
            }


        }

        public static void Overlap(int resultID)
        {

            //string[] line = File.ReadAllLines(@"C:\Users\v-mogenzhang\Desktop\resultID.txt");
            //Console.WriteLine(line[0]);
            //string temp = line[0];
            //int resultID = int.Parse(temp);


            //int resultID = 37461475;37174428
            ZMogenDebugSulpHurEntities entity = new ZMogenDebugSulpHurEntities();
            var varList = from u in entity.UIContents
                          join r in entity.Results on u.ContentID equals r.ContentID
                          where r.ResultID == resultID
                          select new
                          {
                              //xmlContent = XElement.Parse(u.UIContent1),
                              xmlContent = u.UIContent1,
                              screenshot = u.UIScreenShot
                          };
            var obj = varList.FirstOrDefault();
            Bitmap bitmap = null;
            using (MemoryStream ms = new MemoryStream(obj.screenshot))
            {
                bitmap = new Bitmap(ms);
            }

            ControlScreen.CurrentBit = bitmap;
            List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(XElement.Parse(obj.xmlContent));
            //UIANDRule temp2;
            //DBOperator dbOperator = new DBOperator();
            //List<UIANDRule> allDirtyUI = dbOperator. QueryAllDirtyUI();
            //lock (obj)
            //{
            //    foreach (UIANDRule temp3 in allDirtyUI)
            //    {
            //        dirtyUIQueue.Enqueue(temp3);
            //    }
            //}

            //lock (obj)
            //{
            //    temp2 = dirtyUIQueue.Dequeue();
            //}
            // List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(temp2.Element);

            //TabOrderRule rule = new TabOrderRule();

            ElementInformation rootControl = CommonRuleUtility.GetRootElement(eiList);

            // rule.lan = dbOperator.QuerylanbycontentID(temp2.Uiid);
            //Console.WriteLine("start ");
            //  Console.WriteLine("dbOperator.QuerylanbycontentID(temp2.Uiid) = " + dbOperator.QuerylanbycontentID(temp2.Uiid));
            //Console.WriteLine("stop");
            //Console.ReadKey();
            OverlapRule rule = new OverlapRule();
            List<UIComplianceResultBase> results = rule.UIVerify(eiList);

            string fileName = Environment.CurrentDirectory + @"\ResultImage.png";

            foreach (UIComplianceResultBase b in results)
            {
                //uncomment after debugging 
                // b.Image.Save(fileName);
                Console.WriteLine(b.Message);

            }


        }


        public static void Overlap_1(int contentID)
        {

            //string[] line = File.ReadAllLines(@"C:\Users\v-mogenzhang\Desktop\resultID.txt");
            //Console.WriteLine(line[0]);
            //string temp = line[0];
            //int resultID = int.Parse(temp);


            //int resultID = 37461475;37174428
            ZMogenDebugSulpHurEntities entity = new ZMogenDebugSulpHurEntities();
            var varList = from u in entity.UIContents
                          //join r in entity.Results on u.ContentID equals r.ContentID
                          where u.ContentID == contentID
                          select new
                          {
                              //xmlContent = XElement.Parse(u.UIContent1),
                              xmlContent = u.UIContent1,
                              screenshot = u.UIScreenShot
                          };
            var obj = varList.FirstOrDefault();
            Bitmap bitmap = null;
            using (MemoryStream ms = new MemoryStream(obj.screenshot))
            {
                bitmap = new Bitmap(ms);
            }

            ControlScreen.CurrentBit = bitmap;
            List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(XElement.Parse(obj.xmlContent));
            //UIANDRule temp2;
            //DBOperator dbOperator = new DBOperator();
            //List<UIANDRule> allDirtyUI = dbOperator. QueryAllDirtyUI();
            //lock (obj)
            //{
            //    foreach (UIANDRule temp3 in allDirtyUI)
            //    {
            //        dirtyUIQueue.Enqueue(temp3);
            //    }
            //}

            //lock (obj)
            //{
            //    temp2 = dirtyUIQueue.Dequeue();
            //}
            // List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(temp2.Element);

            //TabOrderRule rule = new TabOrderRule();

            ElementInformation rootControl = CommonRuleUtility.GetRootElement(eiList);

            // rule.lan = dbOperator.QuerylanbycontentID(temp2.Uiid);
            //Console.WriteLine("start ");
            //  Console.WriteLine("dbOperator.QuerylanbycontentID(temp2.Uiid) = " + dbOperator.QuerylanbycontentID(temp2.Uiid));
            //Console.WriteLine("stop");
            //Console.ReadKey();
            OverlapRule rule = new OverlapRule();
            List<UIComplianceResultBase> results = rule.UIVerify(eiList);

            string fileName = Environment.CurrentDirectory + @"\ResultImage.png";

            foreach (UIComplianceResultBase b in results)
            {
                //uncomment after debugging 
                // b.Image.Save(fileName);
                Console.WriteLine(b.Message);

            }


        }


        #region
        /*
         
            string[] line = File.ReadAllLines(@"C:\Users\v-mogenzhang\Desktop\resultID.txt");
            Console.WriteLine(line[0]);
            string temp = line[0];
            int resultID = int.Parse(temp);            
            //int resultID = 37461475;37174428
            SulpHurEntitiesAzureSulphur entity = new SulpHurEntitiesAzureSulphur();
            var varList = from u in entity.UIContents
                          join r in entity.Results on u.ContentID equals r.ContentID
                          where r.ResultID == resultID
                          select new
                          {
                              //xmlContent = XElement.Parse(u.UIContent1),
                              xmlContent = u.UIContent1,
                              screenshot = u.UIScreenShot
                          };                     
            var obj = varList.FirstOrDefault();
            Bitmap bitmap = null;
            using (MemoryStream ms = new MemoryStream(obj.screenshot))
            {
                bitmap = new Bitmap(ms);
            }

            ControlScreen.CurrentBit = bitmap;
            //List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(XElement.Parse(obj.xmlContent));
            UIANDRule temp2;
            DBOperator dbOperator = new DBOperator();
            List<UIANDRule> allDirtyUI = dbOperator.QueryAllDirtyUI();
            lock (obj)
            {
                foreach (UIANDRule temp3 in allDirtyUI)
                {
                    dirtyUIQueue.Enqueue(temp3);
                }
            }

            lock (obj)
            {               
                temp2 = dirtyUIQueue.Dequeue();
            }
            List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(temp2.Element);

            //TabOrderRule rule = new TabOrderRule();
            SpellingRule rule = new SpellingRule();
            ElementInformation rootControl = CommonRuleUtility.GetRootElement(eiList);

            rule.lan = dbOperator.QuerylanbycontentID(temp2.Uiid);
            Console.WriteLine("start ");
            Console.WriteLine("dbOperator.QuerylanbycontentID(temp2.Uiid) = " + dbOperator.QuerylanbycontentID(temp2.Uiid));
            Console.WriteLine("stop");
            Console.ReadKey();
            List<UIComplianceResultBase> results = rule.UIVerify(eiList);

            string fileName = Environment.CurrentDirectory + @"\ResultImage.png";

            foreach (UIComplianceResultBase b in results)
            {
                //uncomment after debugging 
                // b.Image.Save(fileName);

            }
            Console.ReadKey();
         
         */
        #endregion
        #region
        /*
         string[] line = File.ReadAllLines(@"C:\Users\v-mogenzhang\Desktop\resultID.txt");
            Console.WriteLine(line[0]);
            string temp = line[0];
            int resultID = int.Parse(temp);

            //int resultID = 37461475;
            SulpHurEntitiesAzureSulphur entity = new SulpHurEntitiesAzureSulphur();
            var varList = from u in entity.UIContents
                          join r in entity.Results on u.ContentID equals r.ContentID
                          where r.ResultID == resultID
                          select new
                          {
                              //xmlContent = XElement.Parse(u.UIContent1),
                              xmlContent = u.UIContent1,
                              screenshot = u.UIScreenShot
                          };
            var obj = varList.FirstOrDefault();
            Bitmap bitmap = null;
            using (MemoryStream ms = new MemoryStream(obj.screenshot))
            {
                bitmap = new Bitmap(ms);
            }

            ControlScreen.CurrentBit = bitmap;
            List<ElementInformation> eiList = new UIContentVerification().ParseTreeToList(XElement.Parse(obj.xmlContent));
            TabOrderRule rule = new TabOrderRule();
            ElementInformation rootControl = CommonRuleUtility.GetRootElement(eiList);
            // Console.WriteLine("rootControl.Name = " + rootControl.Name);
            List<UIComplianceResultBase> results = rule.UIVerify(eiList);



            Console.WriteLine("results.Count = " + results.Count);
            Console.WriteLine("results[0].Type = " + results[0].Type);
            Console.WriteLine("results[0].Message" + results[0].Message);

            string fileName = Environment.CurrentDirectory + @"\ResultImage.png";
            Console.WriteLine("fileName = " + fileName);
            foreach (UIComplianceResultBase b in results)
            {
                b.Image.Save(fileName);
            }
            Console.ReadKey();

         */
        #endregion
        #region
        //DBOperator op = new DBOperator();
        //var list = op.QueryAllDirtyUI1();
        //UIANDRule temp = list.FirstOrDefault();
        //UIContentVerification v = new UIContentVerification();
        //List<ElementInformation> eiList = v.ParseTreeToList(temp.Element);
        //TabOrderRule rule = new TabOrderRule();
        //rule.UIVerify(eiList);
        #endregion
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections;
using System.Reflection;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace RenameLog
{
    public class ClassGenerator
    {
        public CodeCompileUnit targetUnit;

        public CodeTypeDeclaration targetClass;

        public List<CodeTypeDeclaration> classList = new List<CodeTypeDeclaration>();

        public CodeNamespace samples;

        private static String CSName=".cs";

        private static String Lan = "CSharp";

        private List<string> xmlList ;

        private string className="testclass";

        private string Namespace="SP3UIAuto.Dialogs";

        private string dialog = "AAA";

        public ClassGenerator()
        {
            targetUnit = new CodeCompileUnit();
            samples = new CodeNamespace();
            samples.Imports.Add(new CodeNamespaceImport("System"));
            samples.Imports.Add(new CodeNamespaceImport("OpenQA.Selenium.Appium"));
            samples.Imports.Add(new CodeNamespaceImport("OpenQA.Selenium.Appium.Windows"));
            samples.Name = Namespace;

            targetClass = new CodeTypeDeclaration(className);
            targetClass.IsClass = true;
            targetClass.TypeAttributes = TypeAttributes.Public;
            samples.Types.Add(targetClass);
            targetUnit.Namespaces.Add(samples);

        }

        public ClassGenerator(string id)
        {
            targetUnit = new CodeCompileUnit();
            samples = new CodeNamespace();
            samples.Imports.Add(new CodeNamespaceImport("System"));
            samples.Imports.Add(new CodeNamespaceImport("OpenQA.Selenium.Appium"));
            samples.Imports.Add(new CodeNamespaceImport("OpenQA.Selenium.Appium.Windows"));
            samples.Name = Namespace;

            targetClass = new CodeTypeDeclaration(id);
            targetClass.IsClass = true;
            targetClass.TypeAttributes = TypeAttributes.Public;
            targetClass.Comments.Add(new CodeCommentStatement("AutomationID is " + id));
            samples.Types.Add(targetClass);
            targetUnit.Namespaces.Add(samples);
            init("Session");

        }
        public ClassGenerator(XmlNode node)
        {

        }
        public ClassGenerator(List<string> strlist)
        {
            xmlList = new List<string>();
            xmlList = strlist;

        }

        public void init(string id)
        {
            AddFieldsWindowsDriver("Session");
            AddConstructor(id);
            classList.Add(targetClass);
        }

        public void AddFields(string name)
        {
            CodeMemberField member = new CodeMemberField();
            member.Attributes= MemberAttributes.Private;
            member.Name = name;
            member.Type = new CodeTypeReference(typeof(AppiumWebElement));
            member.Comments.Add(new CodeCommentStatement("This is "+ name + " element"));
            targetClass.Members.Add(member);
        }

        public void AddFields(string name, CodeTypeDeclaration targetclass1)
        {
            CodeMemberField member = new CodeMemberField();
            member.Attributes = MemberAttributes.Private;
            member.Name = name;
            member.Type = new CodeTypeReference(typeof(AppiumWebElement));
            member.Comments.Add(new CodeCommentStatement("This is " + name + " element"));
            targetclass1.Members.Add(member);
        }

        public void AddFieldsWindowsDriver(string name)
        {
            CodeMemberField member = new CodeMemberField();
            member.Attributes = MemberAttributes.Private;
            member.Name = name;
            member.Type = new CodeTypeReference(typeof(WindowsDriver<WindowsElement>));
            member.Comments.Add(new CodeCommentStatement("This is " + name + " element"));
            targetClass.Members.Add(member);
        }

        public void AddProperties(string name,string automationId,string dialog)
        {
            name = name.Replace(".", "_").Replace(" ", "_");
            CodeMemberProperty memberProperty = new CodeMemberProperty();
            memberProperty.Attributes=
                MemberAttributes.Public | MemberAttributes.Final;
            memberProperty.Name = name;
            memberProperty.HasGet = true;
            memberProperty.Type = new CodeTypeReference(typeof(AppiumWebElement));
            memberProperty.Comments.Add(new CodeCommentStatement("The property "+ name + " for the element."));

            CodeFieldReferenceExpression refd = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dialog);
            CodeMethodReferenceExpression code = new CodeMethodReferenceExpression(refd, nameof(AppiumWebElement.FindElementByAccessibilityId));
            CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression(code, new CodePrimitiveExpression(automationId));
            
            CodeMethodReturnStatement ret = new CodeMethodReturnStatement(invoke);
            memberProperty.GetStatements.Add(ret);
            targetClass.Members.Add(memberProperty);
        }

        public void AddProperties(string name, string automationId, string dialog, CodeTypeDeclaration targetclass1)
        {
            name = name.Replace(".", "_").Replace(" ","_");
            CodeMemberProperty memberProperty = new CodeMemberProperty();
            memberProperty.Attributes =
                MemberAttributes.Public | MemberAttributes.Final;
            memberProperty.Name = name;
            memberProperty.HasGet = true;
            memberProperty.Type = new CodeTypeReference(typeof(AppiumWebElement));
            memberProperty.Comments.Add(new CodeCommentStatement("The property " + name + " for the element."));

            CodeFieldReferenceExpression refd = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dialog);
            CodeMethodReferenceExpression code = new CodeMethodReferenceExpression(refd, nameof(AppiumWebElement.FindElementByAccessibilityId));
            CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression(code, new CodePrimitiveExpression(automationId));

            CodeMethodReturnStatement ret = new CodeMethodReturnStatement(invoke);
            memberProperty.GetStatements.Add(ret);
            targetclass1.Members.Add(memberProperty);
        }

        public void AddMethod()
        {
        }

        public CodeTypeDeclaration GetClassByName(string name)
        {
            foreach (CodeTypeDeclaration item in classList)
            {
                if (item.Name.ToUpper() == name.ToUpper())
                {
                    return item;
                }
            }

            return null;
            //return GetClassByNameOnTargetClass(name,targetClass);
        }



        public int classCount(CodeTypeDeclaration target)
        {
            CodeTypeMemberCollection cmc = target.Members;
            int count = 0;
            for (int i = 0; i < cmc.Count; i++)
            {
                try
                {
                    CodeTypeDeclaration ctd = (CodeTypeDeclaration)cmc[i];
                    if (ctd.IsClass == true)
                    {
                        count++;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return count;
        }

        public CodeTypeDeclaration GetClassByNameOnTargetClass(string name, CodeTypeDeclaration targetclass)
        {
            if (targetclass.Name.ToUpper() == name.ToUpper())
            {
                return targetclass;
            }
            CodeTypeMemberCollection cmc = targetclass.Members;

            for (int i = 0; i < cmc.Count; i++)
            {
                try
                {
                    CodeTypeDeclaration ctd = (CodeTypeDeclaration)cmc[i];
                    if (ctd.IsClass == true)
                    {
                        if (name == "panelPublicFQDN")
                        {
                            System.Console.WriteLine(name);
                        }
                        if (classCount(ctd)>0)
                        {
                            return GetClassByNameOnTargetClass(name, ctd);
                        }
                        
                    }                    
                }
                catch (Exception)
                {
                }
            }
            return null;
            
        }
        public CodeTypeDeclaration CreateNextClass(string name,CodeTypeDeclaration parentClass)
        {
            CodeTypeDeclaration NextClass = new CodeTypeDeclaration();
            NextClass.Name = name;
            NextClass.IsClass = true;
            NextClass.TypeAttributes= TypeAttributes.Public;

            parentClass.Members.Add(NextClass);
            return NextClass;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classNametype">the create class name</param>
        /// <param name="targetclass"></param>
        /// <param name="paraname"></param>
        /// <param name="automationId"></param>
        /// <param name="para1"></param>
        public void AddNewClassFields(string classNametype,string paraname,string automationId,string para1)
        {
            CodeMemberProperty memberProperty = new CodeMemberProperty();
            memberProperty.Attributes =
                MemberAttributes.Public | MemberAttributes.Final;
            memberProperty.Name = paraname;
            memberProperty.HasGet = true;
            memberProperty.Type = new CodeTypeReference(classNametype);
            memberProperty.Comments.Add(new CodeCommentStatement("The class property " + paraname + " for the element."));

            CodeFieldReferenceExpression refd = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),para1);
            CodeMethodReferenceExpression code = new CodeMethodReferenceExpression(refd, nameof(AppiumWebElement.FindElementByAccessibilityId));
            CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression(code, new CodePrimitiveExpression(automationId));
            
            CodeObjectCreateExpression objectCreate =
                new CodeObjectCreateExpression(
                new CodeTypeReference(classNametype),
                invoke);

            CodeMethodReturnStatement ret = new CodeMethodReturnStatement(objectCreate);
            memberProperty.GetStatements.Add(ret);
            targetClass.Members.Add(memberProperty);

        }

        public void AddNewClassFields(string classNametype, string paraname, string automationId, string para1, CodeTypeDeclaration targetclass)
        {
            CodeMemberProperty memberProperty = new CodeMemberProperty();
            memberProperty.Attributes =
                MemberAttributes.Public | MemberAttributes.Final;
            memberProperty.Name = paraname;
            memberProperty.HasGet = true;
            memberProperty.Type = new CodeTypeReference(classNametype);
            memberProperty.Comments.Add(new CodeCommentStatement("The class property " + paraname + " for the element."));

            CodeFieldReferenceExpression refd = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), para1);
            CodeMethodReferenceExpression code = new CodeMethodReferenceExpression(refd, nameof(AppiumWebElement.FindElementByAccessibilityId));
            CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression(code, new CodePrimitiveExpression(automationId));

            CodeObjectCreateExpression objectCreate =
                new CodeObjectCreateExpression(
                new CodeTypeReference(classNametype),
                invoke);

            CodeMethodReturnStatement ret = new CodeMethodReturnStatement(objectCreate);
            memberProperty.GetStatements.Add(ret);
            targetclass.Members.Add(memberProperty);

        }

        public void AddConstructor(string dialog)
        {
            CodeConstructor con = new CodeConstructor();
            con.Attributes=
                MemberAttributes.Public | MemberAttributes.Final;
            CodeParameterDeclarationExpression pc = new CodeParameterDeclarationExpression(typeof(WindowsDriver<WindowsElement>), "session");
            con.Parameters.Add(pc);
            CodeFieldReferenceExpression codeField = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dialog);

            CodeAssignStatement ass = new CodeAssignStatement(codeField,new CodeVariableReferenceExpression(pc.Name));

            con.Statements.Add(ass);
            targetClass.Members.Add(con);

        }

        public void AddConstructor(string dialog, CodeTypeDeclaration targetclass1)
        {
            CodeConstructor con = new CodeConstructor();
            con.Attributes =
                MemberAttributes.Public | MemberAttributes.Final;
            CodeParameterDeclarationExpression pc = new CodeParameterDeclarationExpression(typeof(AppiumWebElement), "session");
            con.Parameters.Add(pc);
            CodeFieldReferenceExpression codeField = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dialog);

            CodeAssignStatement ass = new CodeAssignStatement(codeField, new CodeVariableReferenceExpression(pc.Name));

            con.Statements.Add(ass);
            targetclass1.Members.Add(con);

        }

        /// <summary>
        /// Generate CSharp source code from the compile unit.
        /// </summary>
        /// <param name="filename">Output file name</param>
        public void GenerateCSharpCode(string fileName)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider(Lan);
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            using (StreamWriter sourceWriter = new StreamWriter(fileName))
            {
                provider.GenerateCodeFromCompileUnit(
                    targetUnit, sourceWriter, options);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="classname">生成的类名</param>
        /// <param name="paraname">类参数名</param>
        /// <param name="automationId">类的automationID</param>
        /// <param name="elemname">父类参数名称</param>
        public void CreateNewCassAndField(ClassGenerator gen,string classname,string paraname,string automationId,string elemname)
        {
            gen.AddNewClassFields(classname, paraname,automationId,elemname);
            CodeTypeDeclaration dec = gen.CreateNextClass(classname,gen.targetClass);
            gen.AddFields(dialog,dec);
            gen.AddConstructor(dialog,dec);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="classname"></param>
        /// <param name="proname"></param>
        /// <param name="paraname"></param>
        /// <param name="automationId"></param>
        /// <param name="targetclass"></param>
        public void CreateNewCassAndFieldByTargetClass(ClassGenerator gen, string classname,string proname, string paraname, string automationId, CodeTypeDeclaration targetclass)
        {
            gen.AddNewClassFields(classname, proname, automationId, paraname);
            CodeTypeDeclaration dec = gen.CreateNextClass(classname, targetclass);
            gen.AddFields(paraname, dec);
            gen.AddConstructor(paraname, dec);
        }

        public void CreateNewCassAndFieldOnTargetClass(ClassGenerator gen, string classname, string proname, string paraname, string automationId, CodeTypeDeclaration targetclass)
        {
            gen.AddNewClassFields(classname, proname, automationId, paraname,targetclass);
            CodeTypeDeclaration dec = gen.CreateNextClass(classname, targetclass);
            classList.Add(dec);
            gen.AddFields(paraname, dec);
            gen.AddConstructor(paraname, dec);
        }

        //static void Main()
        //{
        //    ClassGenerator gen = new ClassGenerator();
        //    gen.AddFields("asd");
        //    gen.AddFields("bbb");
        //    gen.AddFields("css");
        //    gen.AddFieldsWindowsDriver("ddd");
        //    gen.CreateNewCassAndField(gen,"TEST","assd","_panel1","ASD");

        //    gen.AddConstructor("ddd");
        //    gen.AddProperties("aaa","_asd","ddd");
        //    gen.AddProperties("ccc", "_bbb", "ddd");
        //    gen.AddProperties("Dss", "_dss", "ddd");
        //    gen.GenerateCSharpCode("test.cs");
        //}
    }
}

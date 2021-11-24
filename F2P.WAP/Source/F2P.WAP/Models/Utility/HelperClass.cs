using F2P.Utilitarios.Handler;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace F2P.WAP.Models.Utility
{
    public class HelperClass
    {
        public static void ClassGeneraction(ref StringBuilder xStringBuilder, string xName, string xDescription,
                                            List<PropertiesInfo> xList)
        {
            var info = new StringBuilder();
            info.AppendLine(string.Empty);
            foreach (PropertiesInfo dato in xList)
            {
                if (dato.PropertyType.Equals(EnumTypeProperty.PROPERTY_ARRAY))
                    info.AppendLine(ArrayGeneraction(dato));

                if (dato.PropertyType.Equals(EnumTypeProperty.PROPERTY_PRIMITIVE))
                    info.AppendLine(PropertyGeneraction(dato));
            }

            HeaderGeneraction(ref xStringBuilder, xDescription);

            xStringBuilder.AppendLine(string.Format("public class {0} {2} {1} {3}", xName, info, "{", "}"));
        }

        public static void FileGeneraction(ref StringBuilder xStringBuilder)
        {
            var info = new StringBuilder();
            info.AppendLine(string.Empty);
            info.AppendLine("using System;");
            info.AppendLine("using System.Collections.Generic;");
            info.AppendLine(string.Empty);
            info.AppendLine(string.Format("namespace  {0} {1} {3} {2} ", "F2P.BOM.Common", "{", "}",
                                          xStringBuilder));

            string fileName = ConfigurationManager.AppSettings["FileNameCreateDLL"];
            string Path = ConfigurationManager.AppSettings["PathCreateDLL"];

            FileClass.CreateLogFile(info.ToString(), fileName, Path);
        }

        public static void EnumGeneraction(ref StringBuilder xStringBuilder, string xName, string xDescription,
                                           List<PropertiesInfo> xList)
        {
            var info = new StringBuilder();
            info.AppendLine(string.Empty);
            foreach (PropertiesInfo dato in xList)
            {
                info.AppendLine(string.Format("{0} = {1},", dato.PropertyName, dato.PropertyValue));
            }

            HeaderGeneraction(ref xStringBuilder, xDescription);

            xStringBuilder.AppendLine(string.Format("public enum {0} {2} {1} {3}", xName, info, "{", "}"));
        }

        public static void HeaderGeneraction(ref StringBuilder xStringBuilder, string xDescription)
        {
            xStringBuilder.AppendLine("/// <summary>");
            xStringBuilder.AppendLine("///     Autor:  Generador Automatico");
            xStringBuilder.AppendLine(string.Format("///     Fecha:  {0}", DateTime.Now));
            xStringBuilder.AppendLine(string.Format("///     Descripcion: {0}", xDescription));
            xStringBuilder.AppendLine("/// </summary>");
            xStringBuilder.AppendLine("[Serializable]");
        }

        public static string ArrayGeneraction(PropertiesInfo xPropertiesInfo)
        {
            return string.Format("public List<{0}> {1} {2} get; set; {3} ", xPropertiesInfo.PropertyNameType,
                                 xPropertiesInfo.PropertyName, "{", "}");
        }

        public static string PropertyGeneraction(PropertiesInfo xPropertiesInfo)
        {
            return string.Format("public {0} {1} {2}  get; set; {3}", xPropertiesInfo.PropertyNameType,
                                 xPropertiesInfo.PropertyName, "{", "}");
        }

        public static string FormatXml(string xml)
        {
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString(SaveOptions.DisableFormatting);
            }
            catch (Exception)
            {
                return xml;
            }
        }


        /// <summary>
        /// IVAN CORTES CREADOR DE ENSAMBLADO
        ///    string className = "IVANCLASS";
        //var props = new Dictionary<string, Type>() {
        //    { "ID", typeof(string) },
        //    { "NOMBRE", typeof(string) },
        //    { "VALOR", typeof(string[]) }
        //};

        //createType(className, props);
        /// </summary>
        /// <param name="name"></param>
        /// <param name="props"></param>
        public static void createType(string name, IDictionary<string, Type> props)
        {
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });
            var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, "Test.dll", false);
            parameters.GenerateExecutable = false;

            var compileUnit = new CodeCompileUnit();
            var ns = new CodeNamespace("Test.Dynamic");
            compileUnit.Namespaces.Add(ns);
            ns.Imports.Add(new CodeNamespaceImport("System"));

            var classType = new CodeTypeDeclaration(name);
            classType.Attributes = MemberAttributes.Public;
            ns.Types.Add(classType);

            foreach (var prop in props)
            {
                var fieldName = "_" + prop.Key;
                var field = new CodeMemberField(prop.Value, fieldName);
                classType.Members.Add(field);

                var property = new CodeMemberProperty();
                property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                property.Type = new CodeTypeReference(prop.Value);
                property.Name = prop.Key;
                property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)));
                property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodePropertySetValueReferenceExpression()));
                classType.Members.Add(property);
            }

            var results = csc.CompileAssemblyFromDom(parameters, compileUnit);
            results.Errors.Cast<CompilerError>().ToList().ForEach(error => Console.WriteLine(error.ErrorText));
        }
    }

    public class PropertiesInfo
    {
        public string PropertyName { get; set; }
        public string PropertyNameType { get; set; }
        public string PropertyValue { get; set; }
        public EnumTypeProperty PropertyType { get; set; }
    }

    public enum EnumTypeProperty
    {
        CLASS = 0,
        ENUM = 1,
        PROPERTY_ARRAY = 3,
        PROPERTY_PRIMITIVE = 4,
        PROPERTY_CLASS = 5,
        PROPERTY_ENUM = 6,
    }
}
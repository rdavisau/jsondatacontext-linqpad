using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Windows.Input;
using JsonDataContext;
using JsonDataContextDriver.Notepad;
using LINQPad.Extensibility.DataContext;
using Microsoft.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamasoft.JsonClassGenerator;
using MessageBox = System.Windows.MessageBox;

namespace JsonDataContextDriver
{
    public class JsonDynamicDataContextDriver : DynamicDataContextDriver
    {
        public override string Name
        {
            get { return "JSON DataContext Provider"; }
        }

        public override string Author
        {
            get { return "Ryan Davis"; }
        }

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            return String.IsNullOrWhiteSpace(cxInfo.DisplayName) ? "Unnamed JSON Data Context" : cxInfo.DisplayName;
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            var dialog = new ConnectionDialog();
            dialog.SetContext(cxInfo, isNewConnection);

            var result = dialog.ShowDialog();
            return result == true;
        }

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            return base.GetAssembliesToAdd(cxInfo)
                .Concat(new[] {typeof (JsonDataContextBase).Assembly.Location, typeof(HttpUtility).Assembly.Location});
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            return base.GetNamespacesToAdd(cxInfo)
                .Concat(_nameSpacesToAdd)
                .Distinct();
        }

        private List<string> _nameSpacesToAdd = new List<string>();
        

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo,
            AssemblyName assemblyToBuild, ref string nameSpace,
            ref string typeName)
        {
            _nameSpacesToAdd = new List<string>();
            
            var xInputs = cxInfo.DriverData.Element("inputDefs");
            if (xInputs == null)
                return new List<ExplorerItem>();

            var jss = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
            var inputDefs = JsonConvert.DeserializeObject<List<IJsonInput>>(xInputs.Value, jss).ToList();

            var ns = nameSpace;

            // generate class definitions
            var classDefinitions =
                inputDefs
                    .AsParallel()
                    .SelectMany(i =>
                    {
                        i.GenerateClasses(ns);
                        return i.GeneratedClasses;
                    })
                    .ToList();

            // add namespaces
            _nameSpacesToAdd.AddRange(inputDefs.SelectMany(i=>i.NamespacesToAdd));
            _nameSpacesToAdd.AddRange(classDefinitions.Select(c=> c.Namespace));

            // remove the error'd inputs
            var classGenErrors = inputDefs.SelectMany(i => i.Errors).ToList();

            classDefinitions =
                classDefinitions
                    .Where(c => c.Success)
                    .ToList();

            // resolve duplicates
            classDefinitions
                .GroupBy(c => c.ClassName)
                .Where(c => c.Count() > 1)
                .SelectMany(cs => cs.Select((c, i) => new {Class = c, Index = i + 1}).Skip(1))
                .ToList()
                .ForEach(c => c.Class.ClassName += "_" + c.Index);

            // create code to compile
            var usings = "using System;\r\n" +
                         "using System.Collections.Generic;\r\n" +
                         "using System.IO;\r\n" +
                         "using Newtonsoft.Json;\r\n" +
                         "using System.Web;\r\n" +
                         "using JsonDataContext;\r\n";

            var contextProperties =
                inputDefs.SelectMany(i => i.ContextProperties);

            var context =
                String.Format("namespace {1} {{\r\n\r\n public class {2} : JsonDataContextBase {{\r\n\r\n\t\t{0}\r\n\r\n}}\r\n\r\n}}",
                    String.Join("\r\n\r\n\t\t", contextProperties), nameSpace, typeName);
            var code = String.Join("\r\n", classDefinitions.Select(c => c.ClassDefinition));

            var contextWithCode = String.Join("\r\n\r\n", usings, context, code);

            var provider = new CSharpCodeProvider();
            var parameters = new CompilerParameters
            {
                IncludeDebugInformation = true,
                OutputAssembly = assemblyToBuild.CodeBase,
                ReferencedAssemblies =
                {
                    typeof (JsonDataContextBase).Assembly.Location,
                    typeof (JsonConvert).Assembly.Location,

                    typeof (UriBuilder).Assembly.Location,
                    typeof (HttpUtility).Assembly.Location
                }
            };

            var result = provider.CompileAssemblyFromSource(parameters, contextWithCode);

            if (!result.Errors.HasErrors)
            {
                // Pray to the gods of UX for redemption..
                // We Can Do Better
                if (classGenErrors.Any())
                    MessageBox.Show(String.Format("Couldn't process {0} inputs:\r\n{1}", classGenErrors.Count,
                        String.Join(Environment.NewLine, classGenErrors)));

                return
                    LinqPadSampleCode.GetSchema(result.CompiledAssembly.GetType(String.Format("{0}.{1}", nameSpace, typeName)))
                    .Concat(inputDefs.SelectMany(i=>i.ExplorerItems??new List<ExplorerItem>()))
                    .ToList();
            }
            else
            {
                // compile failed, this is Bad
                var sb = new StringBuilder();
                sb.AppendLine("Could not generate a typed context for the given inputs. The compiler returned the following errors:\r\n");

                foreach (var err in result.Errors)
                    sb.AppendFormat(" - {0}\r\n", err);

                if (classGenErrors.Any())
                {
                    sb.AppendLine("\r\nThis may have been caused by the following class generation errors:\r\n");
                    sb.AppendLine(String.Join(Environment.NewLine, String.Join(Environment.NewLine, classGenErrors)));
                }

                MessageBox.Show(sb.ToString());

                NotepadHelper.ShowMessage(contextWithCode, "Generated source code");

                throw new Exception("Could not generate a typed context for the given inputs");
            }
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            
            base.InitializeContext(cxInfo, context, executionManager);
            
            var ctx = (JsonDataContextBase) context;

            var xInputs = cxInfo.DriverData.Element("inputDefs");
            if (xInputs == null)
                return;

            var jss = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var inputs = JsonConvert.DeserializeObject<List<IJsonInput>>(xInputs.Value, jss).ToList();

            inputs
                .OfType<JsonTextInput>()
                .ToList()
                .ForEach(c=> ctx._jsonTextInputs.Add(c.InputGuid, c.Json));
        }
    }
}
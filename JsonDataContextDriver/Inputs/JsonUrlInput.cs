using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using LINQPad.Extensibility.DataContext;
using Newtonsoft.Json;
using PropertyChanged;
using Xamasoft.JsonClassGenerator;

namespace JsonDataContextDriver
{
    [ImplementPropertyChanged]
    public class JsonUrlInput : IJsonInput
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool GenerateAsMethod { get; set; }

        public JsonUrlInput()
        {
            NamespacesToAdd = new List<string>();
            Errors = new List<string>();
        }

        public void GenerateClasses(string nameSpace)
        {
            try
            {
                var req = (HttpWebRequest) HttpWebRequest.Create(Url);
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                req.UserAgent = @"Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

                var examplesJson = "";
                using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
                    examplesJson = sr.ReadToEnd();

                var className = Name.SanitiseClassName();
                var finalNamespace = nameSpace + "." + className + "Input";
                var outputStream = new MemoryStream();
                var outputWriter = new StreamWriter(outputStream);

                var jsg = new JsonClassGenerator
                {
                    Example = examplesJson,
                    Namespace = finalNamespace,
                    MainClass = className,
                    OutputStream = outputWriter,
                    NoHelperClass = true,
                    UseProperties = true
                };

                jsg.GenerateClasses();

                outputWriter.Flush();
                outputStream.Seek(0, SeekOrigin.Begin);

                var classDef = new StreamReader(outputStream)
                    .ReadToEnd()
                    .Replace("IList<", "List<");

                classDef =
                    classDef.Substring(classDef.IndexOf(String.Format("namespace {0}", nameSpace),
                        StringComparison.Ordinal));

                NamespacesToAdd.Add(finalNamespace);

                GeneratedClasses = new List<IGeneratedClass>
                {
                    new JsonUrlGeneratedClass(this)
                    {
                        Namespace = finalNamespace,
                        ClassName = className,
                        Url = Url,
                        ClassDefinition = classDef,
                        Success = true
                    }
                };
            }
            catch (Exception e)
            {
                GeneratedClasses = new List<IGeneratedClass>
                {
                    new JsonUrlGeneratedClass(this)
                    {
                        Url = Url,
                        Success = false,
                        Error = e
                    }
                };
            }

        }

        [JsonIgnore]
        public List<IGeneratedClass> GeneratedClasses { get; set;  }

        [JsonIgnore]
        public List<ExplorerItem> ExplorerItems => 
            GeneratedClasses
                .OfType<JsonUrlGeneratedClass>()
                .Where(c => c.OriginalInput.GenerateAsMethod)
                .Select(
                    c => new ExplorerItem(GetNameForMethod(c), ExplorerItemKind.QueryableObject, ExplorerIcon.Box)
                        {
                            Children = new List<ExplorerItem> { 
                                new ExplorerItem("Parameters", ExplorerItemKind.Category, ExplorerIcon.Schema)
                                {
                                    Children = c.OriginalInput.GetUrlQueryStringParameters()
                                        .Select(p=> new ExplorerItem(p.Item1, ExplorerItemKind.Parameter, ExplorerIcon.Parameter))
                                        .ToList()
                                },
                        }
                    })
                .ToList();

        [JsonIgnore]
        public List<string> NamespacesToAdd { get; set; }

        [JsonIgnore]
        public List<string> ContextProperties => 
            GeneratedClasses
                .OfType<JsonUrlGeneratedClass>()
                .Select(GetContextMethod)
                .ToList();

        [JsonIgnore]
        public List<string> Errors { get; set; }

        private static string GetContextMethod(JsonUrlGeneratedClass c)
        {
            if (c.OriginalInput.GenerateAsMethod)
            {
                var ns = c.Namespace;
                var name = GetNameForMethod(c);
                var cls = c.ClassName;

                var url = c.Url;
                var ps = c.OriginalInput.GetUrlQueryStringParameters();

                var args = String.Join(", ", ps.Select(p => String.Format("string {0} = {1}", p.Item1, ToLiteral(p.Item2))));
                var prototype = String.Format("public IEnumerable<{0}.{1}> {2}({3})", ns, cls, name, args);

                var methodBody =
                    String.Format("var _____uri = new UriBuilder(@\"{0}\");\r\n", url) +
                    String.Format("var _____q = HttpUtility.ParseQueryString(_____uri.Query);\r\n\r\n") +
                    String.Join(Environment.NewLine, ps.Select(q => String.Format("_____q[\"{0}\"] = {0};", q.Item1))) + "\r\n\r\n" +
                    String.Format("_____uri.Query = _____q.ToString();\r\n") +
                    String.Format("return GetUrlParameterlessInput<{0}.{1}>(_____uri.ToString());", ns, cls);

                return String.Format("{0}\r\n{{\r\n{1}\r\n}}", prototype, methodBody);
            }
            else
                return String.Format(
                    "public IEnumerable<{0}.{1}> {2}s {{ get {{ return GetUrlParameterlessInput<{0}.{1}>(@\"{3}\"); }} }}",
                    c.Namespace, c.ClassName, c.ClassName, c.Url);
        }

        private List<Tuple<string, string>> GetUrlQueryStringParameters()
        {
            var uri = new UriBuilder(Url);
            var pcol = HttpUtility.ParseQueryString(uri.Query);
            var ps = pcol
                .AllKeys
                .Select(k => Tuple.Create(k, pcol[k]))
                .ToList();
            return ps;
        }

        private static string GetNameForMethod(JsonUrlGeneratedClass c)
        {
            return String.Format("Get{0}", c.ClassName.SanitiseClassName());
        }

        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, new CodeGeneratorOptions { IndentString = "\t" });
                    var literal = writer.ToString();
                    literal = literal.Replace(string.Format("\" +{0}\t\"", Environment.NewLine), "");
                    return literal;
                }
            }
        }

        public override string ToString()
        {
            return GenerateAsMethod
                ? String.Format("{0}: {1} with parameters ({2})", this.Name, new Uri(this.Url).AbsolutePath,
                    String.Join(", ", GetUrlQueryStringParameters().Select(p => p.Item1)))
                : String.Format("{0}: {1}", this.Name, this.Url);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Runtime.Serialization;
using LINQPad.Extensibility.DataContext;
using Newtonsoft.Json;
using PropertyChanged;
using Xamasoft.JsonClassGenerator;
using JsonDataContextDriver;

namespace JsonDataContextDriver
{
    [ImplementPropertyChanged]
    public class JsonTextInput : IJsonInput
    {
        public string InputGuid { get; set; }
        public string Name { get; set; }
        public string Json { get; set; }

        private JsonTextGeneratedClass _generatedClass;

        public JsonTextInput()
        {
            InputGuid = Guid.NewGuid().ToString();
            NamespacesToAdd = new List<string>();
        }

        public override string ToString()
        {
            var summary = "";
            try { summary = String.Format(" : {0} rows of json text", JsonConvert.DeserializeObject<List<ExpandoObject>>(Json).Count); } catch { }

            return String.Format("{0}{1}", Name, summary);
        }

        public void GenerateClasses(string nameSpace)
        {
            try
            {
                var className = Name.SanitiseClassName();

                var finalNamespace = nameSpace + "." + className + "Input";
                var outputStream = new MemoryStream();
                var outputWriter = new StreamWriter(outputStream);

                var jsg = new JsonClassGenerator
                {
                    Example = Json,
                    Namespace = finalNamespace,
                    MainClass = className,
                    OutputStream = outputWriter,
                    NoHelperClass = true,
                    UseProperties = true,
                    GeneratePartialClasses = true
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

                _generatedClass = new JsonTextGeneratedClass(this)
                {
                    Namespace = finalNamespace,
                    ClassName = className,
                    ClassDefinition = classDef,
                    Success = true
                };
            }
            catch (Exception e)
            {
                _generatedClass = new JsonTextGeneratedClass(this)
                {
                    Success = false,
                    Error = e
                };
            }
        }

        [JsonIgnore]
        public List<IGeneratedClass> GeneratedClasses { get { return new List<IGeneratedClass> {_generatedClass}; } }
        [JsonIgnore]
        public List<ExplorerItem> ExplorerItems { get; set;  }
        [JsonIgnore]
        public List<string> NamespacesToAdd { get; set; }

        [JsonIgnore]
        public List<string> ContextProperties => new List<string>
        {
            String.Format("public IEnumerable<{0}.{1}> {1} {{ get {{ return GetTextJsonInput<{0}.{1}>(\"{2}\"); }}}}", _generatedClass.Namespace, _generatedClass.ClassName, InputGuid)
        };

        [JsonIgnore]
        public List<string> Errors { get { return _generatedClass == null || _generatedClass.Success ? new List<string>() : new List<string> {  _generatedClass.Error.Message }; } }
    }
}
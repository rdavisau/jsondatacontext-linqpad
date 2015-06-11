using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using JsonDataContextDriver;
using LINQPad.Extensibility.DataContext;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using Xamasoft.JsonClassGenerator;

namespace JsonDataContextDriver
{
    [ImplementPropertyChanged]
    public class JsonFileInput : IJsonInput
    {
        public string InputPath { get; set; }

        public string Mask { get; set; }

        public bool Recursive { get; set; }

        public int NumRowsToSample { get; set; }

        public JsonInputType InputType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(InputPath))
                    return JsonInputType.Nothing;
                if (File.Exists(InputPath))
                    return JsonInputType.File;
                if (Directory.Exists(InputPath))
                    return JsonInputType.Directory;
                return JsonInputType.Invalid;
            }
        }

        public bool IsDirectory
        {
            get { return InputType == JsonInputType.Directory; }
        }

        public JsonFileInput()
        {
            NumRowsToSample = 1000;
            NamespacesToAdd = new List<string>();
        }

        public override string ToString()
        {
            switch (InputType)
            {
                case JsonInputType.File:
                    return InputPath;
                case JsonInputType.Directory:
                    return Path.Combine(InputPath, Mask ?? "*.*") + (Recursive ? " + subfolders" : "");
                default:
                    return "ERR";
            }
        }

        public void GenerateClasses(string nameSpace)
        {
            var numSamples = NumRowsToSample;

            _generatedClasses =
                GetInputFiles()
                    .Select(f =>
                    {
                        // TODO: Be a better error handler
                        try
                        {
                            var fs = new FileStream(f, FileMode.Open);
                            var sr = new StreamReader(fs);
                            var jtr = new JsonTextReader(sr);

                            var examples =
                                Enumerable
                                    .Range(0, numSamples)
                                    .Select(_ =>
                                    {
                                        while (jtr.Read())
                                            if (jtr.TokenType == JsonToken.StartObject)
                                                return JObject.Load(jtr).ToString();
                                        return null;
                                    })
                                    .Where(json => json != null);

                            var examplesJson = String.Format("[{0}]", String.Join(",\r\n", examples));

                            jtr.Close();
                            sr.Close();
                            fs.Close();


                            var className = Path.GetFileNameWithoutExtension(f).SanitiseClassName();
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
                            };

                            jsg.GenerateClasses();

                            outputWriter.Flush();
                            outputStream.Seek(0, SeekOrigin.Begin);

                            var classDef = new StreamReader(outputStream)
                                .ReadToEnd()
                                .Replace("IList<", "List<")
                                .Replace(";\r\n", " { get; set; }\r\n");

                            classDef =
                                classDef.Substring(classDef.IndexOf(String.Format("namespace {0}", nameSpace),
                                    StringComparison.Ordinal));

                            NamespacesToAdd.Add(finalNamespace);

                            return new JsonFileGeneratedClass
                            {
                                Namespace = finalNamespace,
                                ClassName = className,
                                DataFilePath = f,
                                ClassDefinition = classDef,
                                Success = true
                            };
                        }
                        catch (Exception e)
                        {
                            return new JsonFileGeneratedClass
                            {
                                DataFilePath = f,
                                Success = false,
                                Error = e
                            };
                        }
                    })
                    .ToList();
        }

        private List<string> GetInputFiles()
        {
            switch (InputType)
            {
                case JsonInputType.File:
                    return new List<string> { InputPath };
                case JsonInputType.Directory:
                    return
                        Directory.GetFiles(InputPath, Mask,
                            Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                default:
                    return new List<string>();
            }
        }

        private List<JsonFileGeneratedClass> _generatedClasses = new List<JsonFileGeneratedClass>(); 
        public List<IGeneratedClass> GeneratedClasses { get { return _generatedClasses.OfType<IGeneratedClass>().ToList(); } }
        public List<ExplorerItem> ExplorerItems { get; set; }
        public List<string> NamespacesToAdd { get; set; }

        public List<string> ContextProperties
        {
            get
            {
                return _generatedClasses
                    .Where(c=> c.Success)
                    .Select(c =>
                       String.Format(
                           "public IEnumerable<{0}.{1}> {2}s {{ get {{ return DeserializeSequenceFromJsonFile<{0}.{1}>(@\"{3}\"); }} }}",
                           c.Namespace, c.ClassName, c.ClassName, c.DataFilePath))
                    .ToList();
            }
        }

        public List<string> Errors
        {
            get
            {
                return _generatedClasses
                            .Where(c=> !c.Success)
                            .Select(e => String.Format("  {0} - {1}", e.DataFilePath, e.Error.Message))
                            .ToList();
            }
        }
    }
}
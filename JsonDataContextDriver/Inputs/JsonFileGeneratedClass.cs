using System;

namespace JsonDataContextDriver
{
    public class JsonFileGeneratedClass : IGeneratedClass
    {
        public JsonFileGeneratedClass(JsonFileInput input)
        {
            OriginalInput = input;
        }

        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string DataFilePath { get; set; }
        public string ClassDefinition { get; set; }
        public bool Success { get; set; }
        public Exception Error { get; set; }

        public JsonFileInput OriginalInput { get; set; }
        IJsonInput IGeneratedClass.OriginalInput
        {
            get { return OriginalInput; }
            set { OriginalInput = (JsonFileInput) value; }
        }
    }
}
using System;

namespace JsonDataContextDriver
{
    public class JsonUrlGeneratedClass : IGeneratedClass
    {
        public JsonUrlGeneratedClass(JsonUrlInput input)
        {
            OriginalInput = input;
        }

        public string OriginalName { get; set; }
        public string Url { get; set; }

        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string ClassDefinition { get; set; }
        public bool Success { get; set; }
        public Exception Error { get; set; }

        public JsonUrlInput OriginalInput;
        IJsonInput IGeneratedClass.OriginalInput
        {
            get { return OriginalInput; }
            set { OriginalInput = (JsonUrlInput) value; }
        }
    }
}
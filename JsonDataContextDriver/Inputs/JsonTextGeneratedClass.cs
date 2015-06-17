using System;

namespace JsonDataContextDriver
{
    public class JsonTextGeneratedClass : IGeneratedClass
    {
        public JsonTextGeneratedClass(JsonTextInput input)
        {
            Input = input;
        }

        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string ClassDefinition { get; set; }
        public bool Success { get; set; }
        public Exception Error { get; set; }

        public JsonTextInput Input { get; set; }
        IJsonInput IGeneratedClass.OriginalInput
        {
            get { return Input; }
            set { Input = (JsonTextInput)value; }
        }
    }
}
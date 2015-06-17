using System;

namespace JsonDataContextDriver
{
    public interface IGeneratedClass
    {   
        string Namespace { get; set; }
        string ClassName { get; set; }
        string ClassDefinition { get; set; }
        bool Success { get; set; }
        Exception Error { get; set; }

        IJsonInput OriginalInput { get; set; }
    }
}
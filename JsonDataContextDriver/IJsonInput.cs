using System.Collections.Generic;
using LINQPad.Extensibility.DataContext;

namespace JsonDataContextDriver
{
    public interface IJsonInput
    {
        void GenerateClasses(string nameSpace);

        List<IGeneratedClass> GeneratedClasses { get; }
        List<ExplorerItem> ExplorerItems { get; }
        List<string> NamespacesToAdd { get; }
        List<string> ContextProperties { get; } 
        List<string> Errors { get; }
    }
}
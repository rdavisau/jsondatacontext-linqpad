using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using JsonDataContextDriver;
using PropertyChanged;

namespace JsonDataContextDriver
{
    [ImplementPropertyChanged]
    public class JsonInput
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

        public JsonInput()
        {
            NumRowsToSample = 1000;
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
    }
}
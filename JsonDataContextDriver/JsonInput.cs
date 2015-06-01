using System;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using JsonDataContextDriver;
using ReactiveUI;

namespace JsonDataContextDriver
{
    public class JsonInput : ReactiveObject
    {
        private string _path;
        [DataMember]
        public string InputPath { get { return _path; } set { this.RaiseAndSetIfChanged(ref _path, value); } }
        
        private string _mask;
        [DataMember]
        public string Mask { get { return _mask; } set { this.RaiseAndSetIfChanged(ref _mask, value); } }
        
        private bool _recursive;
        [DataMember]
        public bool Recursive { get { return _recursive; } set { this.RaiseAndSetIfChanged(ref _recursive, value); } }

        private int _numRowsToSample;
        [DataMember]
        public int NumRowsToSample { get { return _numRowsToSample; } set { this.RaiseAndSetIfChanged(ref _numRowsToSample, value); } }

        private readonly ObservableAsPropertyHelper<JsonInputType> _inputType;
        public JsonInputType InputType { get { return _inputType.Value; } }

        private readonly ObservableAsPropertyHelper<Brush> _bgBrush;
        public Brush BGBrush { get { return _bgBrush.Value; } }

        private readonly ObservableAsPropertyHelper<bool> _isDirectory;
        public bool IsDirectory { get { return _isDirectory.Value; } }

        private readonly ObservableAsPropertyHelper<Visibility> _showDirectoryControls;
        public Visibility ShowDirectoryControls { get { return _showDirectoryControls.Value; } }


        public JsonInput()
        {
            NumRowsToSample = 100;

            this.WhenAnyValue(x => x.InputPath)
                .Select(path =>
                {
                    if (string.IsNullOrWhiteSpace(path))
                        return JsonInputType.Nothing;
                    if (File.Exists(path))
                        return JsonInputType.File;
                    if (Directory.Exists(path))
                        return JsonInputType.Directory;
                    return JsonInputType.Invalid;
                })
                .ToProperty(this, x => x.InputType, out _inputType);

            this.WhenAnyValue(x => x.InputType)
                .StartWith(JsonInputType.Nothing)
                .Select(t => (t == JsonInputType.Invalid || t == JsonInputType.Nothing) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White))
                .ToProperty(this, x => x.BGBrush, out _bgBrush);

            this.WhenAnyValue(x => x.InputType)
                .StartWith(JsonInputType.Nothing)
                .Select(t => t == JsonInputType.Directory)
                .ToProperty(this, x => x.IsDirectory, out _isDirectory);

            this.WhenAnyValue(x => x.IsDirectory)
                .Select(d => d ? Visibility.Visible : Visibility.Hidden)
                .ToProperty(this, x => x.ShowDirectoryControls, out _showDirectoryControls);
        }
    }
}
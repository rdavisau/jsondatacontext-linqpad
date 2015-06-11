using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JsonDataContextDriver;
using LINQPad.Extensibility.DataContext;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace JsonDataContextDriver
{
    public partial class AddNewUrlSourceDialog : Window
    {
        private readonly SolidColorBrush _goodBrush = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush _badBrush = new SolidColorBrush(Colors.IndianRed) {Opacity = .5};

        private readonly JsonTextInput _input;

        public JsonTextInput Input { get; set; }

        public AddNewUrlSourceDialog()
        {
            _input = new JsonTextInput();

            InitializeComponent();

            // sigh, too bad reactiveui doesn't support .net 4
            Action doValidation = () =>
            {
                var name = NameTextBox.Text;
                var json = "";//JsonTextBox.Text;

                bool validJson = false;
                try
                {
                    var obj = JContainer.Parse(json);
                    validJson = true;
                } catch { }

                var nameOk = true;
                var jsonOk = validJson || String.IsNullOrEmpty(json);

                OkButton.IsEnabled = (nameOk && jsonOk && !String.IsNullOrEmpty(json));

                NameTextBox.Background = nameOk ? _goodBrush : _badBrush;
                //JsonTextBox.Background = jsonOk ? _goodBrush : _badBrush;
                
            };

            NameTextBox.TextChanged += (sender, args) => doValidation();
            //JsonTextBox.TextChanged += (sender, args) => doValidation();

            UrlTextBox.TextChanged += (sender, args) =>
            {
                try
                {
                    var uri = new Uri(UrlTextBox.Text);
                    var pcol = HttpUtility.ParseQueryString(uri.Query);
                    var ps = pcol
                        .AllKeys
                        .Select(k => Tuple.Create(k, pcol[k]))
                        .ToList();

                    ParametersListView.ItemsSource = ps;

                    UrlTextBox.Background = _goodBrush;
                }
                catch
                {
                    UrlTextBox.Background = _badBrush;
                }
            };

            CancelButton.Click += (sender, args) => DialogResult = false;
            OkButton.Click += (sender, args) =>
            {
                _input.Name = NameTextBox.Text;
                //_input.Json = JsonTextBox.Text;

                Input = _input;
                DialogResult = true;
            };

            doValidation();
        }

        public AddNewUrlSourceDialog(JsonTextInput input) : this()
        {
            _input = input;

            NameTextBox.Text = _input.Name;
            //JsonTextBox.Text = _input.Json;
        }
        
    }

    [ImplementPropertyChanged]
    public class JsonUrlInput : IJsonInput
    {
        public string Name { get; set; }
        public string Url { get; set; }
        

        public void GenerateClasses(string nameSpace)
        {
            throw new NotImplementedException();
        }

        public List<IGeneratedClass> GeneratedClasses { get; }
        public List<ExplorerItem> ExplorerItems { get; }
        public List<string> NamespacesToAdd { get; }
        public List<string> ContextProperties { get; }
        public List<string> Errors { get; }
    }

    public class MockXViewModel
    {
        public List<Tuple<string, string>> Items => new List<Tuple<string, string>>
        {
            Tuple.Create("param1", "value1"),
            Tuple.Create("param2", "value2"),
            Tuple.Create("param3", "value3"),
        };

    }



}
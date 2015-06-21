using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using Path = System.IO.Path;

namespace JsonDataContextDriver
{
    [ImplementPropertyChanged]
    public partial class AddNewUrlSourceDialog : Window
    {
        private readonly SolidColorBrush _goodBrush = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush _badBrush = new SolidColorBrush(Colors.IndianRed) {Opacity = .5};

        private readonly JsonUrlInput _input;

        public ObservableCollection<WebRequestHeader> Headers = new ObservableCollection<WebRequestHeader>(WebRequestHeader.DefaultHeaders);

        public JsonUrlInput Input { get; set; }

        public AddNewUrlSourceDialog()
        {
            _input = new JsonUrlInput();

            InitializeComponent();

            // sigh, too bad reactiveui doesn't support .net 4
            Action doValidation = () =>
            {
                var name = NameTextBox.Text;
                var uri = UrlTextBox.Text;

                bool validUri = false;
                try
                {
                    var obj = new Uri(uri, UriKind.Absolute);
                    validUri = true;
                } catch { }

                var nameOk = String.IsNullOrEmpty(name) || char.IsLetter(name.ToCharArray()[0]);
                var jsonOk = validUri || String.IsNullOrEmpty(uri);

                OkButton.IsEnabled = (nameOk && jsonOk && !String.IsNullOrEmpty(name));

                NameTextBox.Background = nameOk ? _goodBrush : _badBrush;
                UrlTextBox.Background = jsonOk ? _goodBrush : _badBrush;
            };

            NameTextBox.TextChanged += (sender, args) => doValidation();
            UrlTextBox.TextChanged += (sender, args) => doValidation();

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
                    ParametersListView.ItemsSource = null;
                }

                if (ParametersListView.Items.Count > 0)
                    ExposeAsMethodCheckbox.IsEnabled = true;
                else
                {
                    ExposeAsMethodCheckbox.IsEnabled = false;
                    ExposeAsPropertyCheckBox.IsChecked = true;
                }
            };

            HeadersListView.SelectionChanged += (sender, args) => RemoveHeaderButton.IsEnabled = HeadersListView.SelectedItem != null;
            AddNewHeaderButton.Click += (sender, args) =>
            {
                var h = new WebRequestHeader() {Name = "Name", Value = "Value"};
                Headers.Add(h);
                HeadersListView.SelectedItem = h;
            };
            RemoveHeaderButton.Click += (sender, args) =>
            {
                var h = HeadersListView.SelectedItem as WebRequestHeader;
                Headers.Remove(h);
            };

            CancelButton.Click += (sender, args) => DialogResult = false;
            OkButton.Click += (sender, args) =>
            {
                _input.Name = NameTextBox.Text;
                _input.Url = UrlTextBox.Text;
                _input.Headers = Headers;
                _input.GenerateAsMethod = ExposeAsMethodCheckbox.IsChecked == true;

                Input = _input;
                DialogResult = true;
            };

            HeadersListView.ItemsSource = Headers;

            doValidation();

            NameTextBox.Focus();
        }

        public AddNewUrlSourceDialog(JsonUrlInput input) : this()
        {
            _input = input;

            NameTextBox.Text = _input.Name;
            UrlTextBox.Text = _input.Url;
            Headers = input.Headers;

            ExposeAsMethodCheckbox.IsChecked = _input.GenerateAsMethod;
            ExposeAsPropertyCheckBox.IsChecked = !_input.GenerateAsMethod;

            HeadersListView.ItemsSource = Headers;
        }
    }

    [ImplementPropertyChanged]
    public class WebRequestHeader
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public static List<WebRequestHeader> DefaultHeaders = new List<WebRequestHeader>
        {
            new WebRequestHeader { Name = "Accept", Value = "application/json; text/json"},
        };
    }

    public class MockXViewModel
    {
        public List<Tuple<string, string>> Items = new List<Tuple<string, string>>
        {
            Tuple.Create("param1", "value1"),
            Tuple.Create("param2", "value2"),
            Tuple.Create("param3", "value3"),
        };

    }


}
using System;
using System.Collections.Generic;
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
using Path = System.IO.Path;

namespace JsonDataContextDriver
{
    public partial class AddNewUrlSourceDialog : Window
    {
        private readonly SolidColorBrush _goodBrush = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush _badBrush = new SolidColorBrush(Colors.IndianRed) {Opacity = .5};

        private readonly JsonUrlInput _input;

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

            ExposeAsMethodCheckbox.Checked += (sender, args) => ParametersListView.Visibility = Visibility.Visible;
            ExposeAsMethodCheckbox.Unchecked += (sender, args) => ParametersListView.Visibility = Visibility.Collapsed;


            CancelButton.Click += (sender, args) => DialogResult = false;
            OkButton.Click += (sender, args) =>
            {
                _input.Name = NameTextBox.Text;
                _input.Url = UrlTextBox.Text;
                _input.GenerateAsMethod = ExposeAsMethodCheckbox.IsChecked == true;

                Input = _input;
                DialogResult = true;
            };

            doValidation();

            NameTextBox.Focus();
        }

        public AddNewUrlSourceDialog(JsonUrlInput input) : this()
        {
            _input = input;

            NameTextBox.Text = _input.Name;
            UrlTextBox.Text = _input.Url;
            ExposeAsMethodCheckbox.IsChecked = _input.GenerateAsMethod;
            ExposeAsPropertyCheckBox.IsChecked = !_input.GenerateAsMethod;
        }
        
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
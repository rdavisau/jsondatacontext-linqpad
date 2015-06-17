using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FolderSelect;
using Microsoft.Win32;

namespace JsonDataContextDriver
{
    public partial class AddNewFolderSourceDialog : Window
    {
        private readonly SolidColorBrush _goodBrush = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush _badBrush = new SolidColorBrush(Colors.IndianRed) {Opacity = .5};

        private readonly JsonFileInput _input;

        public JsonFileInput Input { get; set; }

        public AddNewFolderSourceDialog()
        {
            _input = new JsonFileInput();

            InitializeComponent();

            // sigh, too bad reactiveui doesn't support .net 4
            Action doValidation = () =>
            {
                var path = PathTextBox.Text;
                var count = NumRowsToSampleTextBox.Text;

                int nr;
                var pathOk = Directory.Exists(path) || String.IsNullOrEmpty(path);
                var countOk = Int32.TryParse(count, out nr) && nr > 0;

                OkButton.IsEnabled = (pathOk && countOk && !String.IsNullOrEmpty(path));

                PathTextBox.Background = pathOk ? _goodBrush : _badBrush;
                NumRowsToSampleTextBox.Background = countOk ? _goodBrush : _badBrush;
            };

            PathTextBox.TextChanged += (sender, args) => doValidation();
            NumRowsToSampleTextBox.TextChanged += (sender, args) => doValidation();

            BrowseButton.Click += (sender, args) =>
            {
                var fileDialog = new FolderSelectDialog();
                var result = fileDialog.ShowDialog();

                if (result)
                    PathTextBox.Text = fileDialog.FileName;
            };

            CancelButton.Click += (sender, args) => DialogResult = false;
            OkButton.Click += (sender, args) =>
            {
                _input.InputPath = PathTextBox.Text;
                _input.Recursive = (bool) RecurseCheckbox.IsChecked;
                _input.Mask = String.IsNullOrWhiteSpace(FileMaskTextBox.Text) ? "*.*" : FileMaskTextBox.Text;
                _input.NumRowsToSample = Math.Max(0, Int32.Parse(NumRowsToSampleTextBox.Text));

                Input = _input;
                DialogResult = true;
            };

            doValidation();

            PathTextBox.Focus();
        }

        public AddNewFolderSourceDialog(JsonFileInput input) : this()
        {
            _input = input;

            PathTextBox.Text = _input.InputPath;
            RecurseCheckbox.IsChecked = _input.Recursive;
            FileMaskTextBox.Text = _input.Mask;
            NumRowsToSampleTextBox.Text = _input.NumRowsToSample.ToString();
        }
    }
}
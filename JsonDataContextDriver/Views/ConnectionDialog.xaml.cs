using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.IO;
using System.Windows.Media;
using LINQPad.Extensibility.DataContext;
using Newtonsoft.Json;

namespace JsonDataContextDriver
{
    public partial class ConnectionDialog
    {
        private IConnectionInfo _connectionInfo;
        private readonly ObservableCollection<IJsonInput> _jsonInputs = new ObservableCollection<IJsonInput>();
        private readonly SolidColorBrush _highlightedBrush = new SolidColorBrush(Colors.LightBlue);
        private readonly SolidColorBrush _standardBrush = new SolidColorBrush(Colors.White);

        public ConnectionDialog()
        {
            InitializeComponent();

            RemoveButton.Click += (sender, args) =>
            {
                var input = InputsListView.SelectedItem as IJsonInput;

                if (input == null)
                    return;

                _jsonInputs.Remove(input);
            };

            NewTextMenuItem.MouseUp += (sender, args) =>
            {
                var dialog = new AddNewTextSourceDialog() { Owner = this };
                var result = dialog.ShowDialog();

                if (!(result.HasValue && result.Value))
                    return;

                _jsonInputs.Add(dialog.Input);
            };

            NewFileMenuItem.MouseUp += (sender, args) =>
            {
                var dialog = new AddNewFileSourceDialog() { Owner = this };
                var result = dialog.ShowDialog();

                if (!(result.HasValue && result.Value))
                    return;

                _jsonInputs.Add(dialog.Input);
            };

            NewFolderMenuItem.MouseUp += (sender, args) =>
            {
                var dialog = new AddNewFolderSourceDialog { Owner = this };
                var result = dialog.ShowDialog();

                if (!(result.HasValue && result.Value))
                    return;

                _jsonInputs.Add(dialog.Input);
            };

            NewWebMenuItem.MouseUp += (sender, args) =>
            {
                var dialog = new AddNewUrlSourceDialog() { Owner = this };
                var result = dialog.ShowDialog();

                if (!(result.HasValue && result.Value))
                    return;

                _jsonInputs.Add(dialog.Input);
            };

            foreach (
                var panel in
                    new[]
                    {
                        (DockPanel) NewFileMenuItem.Parent, (DockPanel) NewFolderMenuItem.Parent,
                        (DockPanel) NewWebMenuItem.Parent, (DockPanel) NewTextMenuItem.Parent
                    })
            {
                var p = panel;
                p.MouseEnter += (sender, args) => p.Background = _highlightedBrush;
                p.MouseLeave += (sender, args) => p.Background = _standardBrush;
            }

            CancelButton.Click += (sender, args) => DialogResult = false;

            OkButton.Click += (sender, args) =>
            {
                var jss = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
                var inputDefs = JsonConvert.SerializeObject(_jsonInputs.ToList(), jss);

                _connectionInfo.DisplayName = ConnectionNameTextBox.Text;
                _connectionInfo.DriverData.SetElementValue("inputDefs", inputDefs);

                DialogResult = true;
            };

            InputsListView.MouseDoubleClick += (sender, args) =>
            {
                var selectedItem = InputsListView.SelectedItem;

                if (selectedItem == null)
                    return;

                if (selectedItem is JsonTextInput)
                {
                    var jti = selectedItem as JsonTextInput;
                    var dialog = new AddNewTextSourceDialog(jti) { Owner = this };
                    dialog.ShowDialog();
                }
                else if (selectedItem is JsonUrlInput)
                {
                    var jui = selectedItem as JsonUrlInput;
                    var dialog = new AddNewUrlSourceDialog(jui) { Owner = this };
                    dialog.ShowDialog();
                }
                else if (selectedItem is JsonFileInput)
                { 
                    var jfi = selectedItem as JsonFileInput;
                    if (jfi.IsDirectory)
                    {
                        var dialog = new AddNewFolderSourceDialog(jfi) { Owner = this };
                        dialog.ShowDialog();
                    }
                    else
                    {
                        var dialog = new AddNewFileSourceDialog(jfi) { Owner = this };
                        dialog.ShowDialog();
                    }
                }
            };

            Action checkCanOk = () => OkButton.IsEnabled = _jsonInputs.Count > 0;
            Action checkCanRemove = () => RemoveButton.IsEnabled = InputsListView.SelectedItem != null;

            _jsonInputs.CollectionChanged += (sender, args) => checkCanOk();
            InputsListView.SelectionChanged += (sender, args) => checkCanRemove();

            InputsListView.ItemsSource = _jsonInputs;
            checkCanOk();
            checkCanRemove();

            ConnectionNameTextBox.Focus();
        }

        public void SetContext(IConnectionInfo cxInfo, bool isNewConnection)
        {
            _connectionInfo = cxInfo;

            ConnectionNameTextBox.Text = _connectionInfo.DisplayName;

            var xInputs = cxInfo.DriverData.Element("inputDefs");
            if (xInputs == null) return;

            var jss = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
            var inputDefs = JsonConvert.DeserializeObject<List<IJsonInput>>(xInputs.Value, jss);

            _jsonInputs.Clear();
            inputDefs.ForEach(_jsonInputs.Add);
        }
    }
}
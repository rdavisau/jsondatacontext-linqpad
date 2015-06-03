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
        private readonly ObservableCollection<JsonInput> _jsonInputs = new ObservableCollection<JsonInput>();
        private readonly SolidColorBrush _highlightedBrush = new SolidColorBrush(Colors.LightBlue);
        private readonly SolidColorBrush _standardBrush = new SolidColorBrush(Colors.White);

        public ConnectionDialog()
        {
            InitializeComponent();

            RemoveButton.Click += (sender, args) =>
            {
                var input = InputsListView.SelectedItem as JsonInput;

                if (input == null)
                    return;

                _jsonInputs.Remove(input);
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

            NewWebMenuItem.MouseUp += (sender, args) => MessageBox.Show("NOT IMPLEMENTED, EXCEPTION! >:O");

            foreach (
                var panel in
                    new[]
                    {
                        (DockPanel) NewFileMenuItem.Parent, (DockPanel) NewFolderMenuItem.Parent,
                        (DockPanel) NewWebMenuItem.Parent
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
                var selectedItem = InputsListView.SelectedItem as JsonInput;

                if (selectedItem == null)
                    return;

                if (selectedItem.IsDirectory)
                {
                    var dialog = new AddNewFolderSourceDialog(selectedItem) { Owner = this };
                    dialog.ShowDialog();
                }
                else
                {
                    var dialog = new AddNewFileSourceDialog(selectedItem) { Owner = this };
                    dialog.ShowDialog();
                }
                
            };

            Action checkCanOk = () => OkButton.IsEnabled = _jsonInputs.Count > 0;

            _jsonInputs.CollectionChanged += (sender, args) => checkCanOk();

            InputsListView.ItemsSource = _jsonInputs;
            checkCanOk();
        }

        public void SetContext(IConnectionInfo cxInfo, bool isNewConnection)
        {
            _connectionInfo = cxInfo;

            ConnectionNameTextBox.Text = _connectionInfo.DisplayName;

            var xInputs = cxInfo.DriverData.Element("inputDefs");
            if (xInputs == null) return;

            var jss = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
            var inputDefs = JsonConvert.DeserializeObject<List<JsonInput>>(xInputs.Value, jss);

            _jsonInputs.Clear();
            inputDefs.ForEach(_jsonInputs.Add);
        }
    }
}
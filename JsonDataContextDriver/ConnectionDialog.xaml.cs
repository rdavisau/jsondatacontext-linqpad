using ReactiveUI;
using System;
using System.Collections.Generic;
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
using LINQPad.Extensibility.DataContext;
using Newtonsoft.Json;

namespace JsonDataContextDriver
{
    public partial class ConnectionDialog
    {
        private IConnectionInfo _connectionInfo; 
        private readonly ReactiveList<JsonInput> _jsonInputs = new ReactiveList<JsonInput>();

        public ConnectionDialog()
        {
            _jsonInputs = new ReactiveList<JsonInput>
            {
                new JsonInput { InputPath = @"C:\Ryan", Mask = "*.json", Recursive = true },
                new JsonInput { InputPath = @"C:\Windows", Mask = "*.*", Recursive = false }
            };

            InitializeComponent();

            Inputs.ItemsSource = _jsonInputs;
        }

        public ConnectionDialog(IConnectionInfo cxInfo) : base()
        {
            // blah blah blah
        }

        private void Inputs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var newInput = new JsonInput();
            _jsonInputs.Add(newInput);

            Inputs.SelectedItem = newInput;
        }

        public void SetContext(IConnectionInfo cxInfo, bool isNewConnection)
        {
            _connectionInfo = cxInfo;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var jss = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var inputDefs = JsonConvert.SerializeObject(_jsonInputs.ToList(), jss);

            _connectionInfo.DriverData.SetElementValue("inputDefs", inputDefs);

            DialogResult = true;
        }
    }
}

using System;
using System.Windows;
using Database;

namespace DatabaseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Data _data;

        public MainWindow()
        {
            InitializeComponent();
            _data = FindResource("Database") as Data;

            if (_data == null)
                throw new Exception("resource is missing");

            var powerNetOne = new PowerNet();
            var powerNetTwo = new PowerNet();
            powerNetOne.Nodes.Add(new Node { Name = "blub" });
            powerNetOne.Nodes.Add(new Node { Name = "blob" });
            powerNetTwo.Nodes.Add(new Node { Name = "heinz" });
            powerNetTwo.Nodes.Add(new Node { Name = "hanz" });
            powerNetTwo.Nodes.Add(new Node { Name = "kunz" });

            _data.PowerNets.Add(powerNetOne);
            _data.PowerNets.Add(powerNetTwo);
            LoggingOutput.Text = "blub\nblob";
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CalculateNodeVoltages(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

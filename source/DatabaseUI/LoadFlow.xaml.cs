using System;
using System.Windows;
using System.Windows.Controls;
using Database;

namespace DatabaseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Database.Database _database;

        public MainWindow()
        {
            InitializeComponent();
            _database = FindResource("Database") as Database.Database;

            if (_database == null)
                throw new Exception("resource is missing");

            var powerNetOne = new PowerNet();
            var powerNetTwo = new PowerNet();
            powerNetOne.Nodes.Add(new Node());
            powerNetOne.Nodes.Add(new Node());
            powerNetTwo.Nodes.Add(new Node());
            powerNetTwo.Nodes.Add(new Node());
            powerNetTwo.Nodes.Add(new Node());

            _database.PowerNets.Add(powerNetOne);
            _database.PowerNets.Add(powerNetTwo);
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

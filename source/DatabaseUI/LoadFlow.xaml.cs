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
        private readonly PowerNets _powerNets;

        public MainWindow()
        {
            InitializeComponent();
            _powerNets = FindResource("PowerNets") as PowerNets;

            if (_powerNets == null)
                throw new Exception("resource is missing");

            var powerNetOne = new PowerNet();
            var powerNetTwo = new PowerNet();
            powerNetOne.Nodes.Add(new Node());
            powerNetOne.Nodes.Add(new Node());
            powerNetTwo.Nodes.Add(new Node());
            powerNetTwo.Nodes.Add(new Node());
            powerNetTwo.Nodes.Add(new Node());

            _powerNets.Add(powerNetOne);
            _powerNets.Add(powerNetTwo);
            LoggingOutput.Text = "blub\nblob";
        }
    }
}

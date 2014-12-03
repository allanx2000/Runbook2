using Runbook2.ViewModels;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Runbook2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            TasksService.Service.Save();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            TasksService.LoadFromXML(@"C:\Download\test.xml");
            this.viewModel = new MainWindowViewModel();
            this.DataContext = viewModel;

        }
    }
}

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
using System.Windows.Shapes;

using ProductMonitor.Desktop.ViewModel;
using ProductMonitor.Domain.Model;

namespace ProductMonitor.Desktop
{
    /// <summary>
    /// Shell.xaml 的交互逻辑
    /// </summary>
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();
            this.Loaded += Shell_Loaded;
        }

        public ShellViewModel ViewModel
        {
            get { return this.DataContext as ShellViewModel; }
        }

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            ShellViewModel viewModel = new ShellViewModel();
            viewModel.GetViewModelCompleted += viewModel_GetViewModelCompleted;
            viewModel.GetLogCompleted += ViewModel_GetLogCompleted;
            viewModel.SendEmailCompleted += viewModel_SendEmailCompleted;
            this.DataContext = viewModel;
        }

        private void viewModel_SendEmailCompleted(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() => {
                this.IsEnabled = true;
                this.ViewModel.ShowRegisterView();
            });
        }

        private void ViewModel_GetLogCompleted(object sender, EventArgs e)
        {
            this.gridProducts.Dispatcher.Invoke(new Action(() =>
            {
                this.ViewModel.Logs.Add(sender as LogInfo);
                
            }));
        }

        public void viewModel_GetViewModelCompleted(object sender, EventArgs e)
        {
            this.gridProducts.Dispatcher.Invoke(new Action(() => {
                this.ViewModel.ProductViewModels.Add(sender as ProductViewModel);
            }));
        }
    }
}

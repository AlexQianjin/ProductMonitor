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

using ProductMonitor.Utility;
using ProductMonitor.Desktop.ViewModel;

namespace ProductMonitor.Desktop.View
{
    /// <summary>
    /// RegisterView.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterView : Window
    {
        public RegisterView()
        {
            InitializeComponent();
            this.Closed += RegisterView_Closed;
            this.Loaded += RegisterView_Loaded;
        }

        private void RegisterView_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterViewModel viewModel = this.DataContext as RegisterViewModel;
            viewModel.RegisterCompleted += viewModel_RegisterCompleted;
        }

        private void viewModel_RegisterCompleted(object sender, EventArgs e)
        {
            this.Close();
        }

        public void RegisterView_Closed(object sender, EventArgs e)
        {
           if(SoftReg.IsRegistered())
           {
               MessageBox.Show("注册成功");
           }
           else
           {
               Application.Current.Shutdown();
           }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Net;
using System.Management;
using System.IO;
using System.Media;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Commands;

using ProductMonitor.Desktop.ViewModel;
using ProductMonitor.Domain.Model;
using ProductMonitor.Domain.Interface;
using ProductMonitor.Domain.Implementation;
using ProductMonitor.Utility;
using ProductMonitor.Desktop.View;

namespace ProductMonitor.Desktop.ViewModel
{
    public class ShellViewModel : BaseViewModel
    {
        #region Private Fields

        //private readonly IUnityContainer _container;
        private readonly IProductManager _productManager;
        System.Timers.Timer _timer;

        private string _title = string.Empty;
        private int _limitCount = 20;
        private bool _isEnable = true;

        private ObservableCollection<ProductViewModel> _productViewModels = new ObservableCollection<ProductViewModel>();
        private ProductViewModel _selectedProductViewModel = new ProductViewModel();
        private ObservableCollection<LogInfo> _logs = new ObservableCollection<LogInfo>();

        #endregion

        #region Public / Protected Properties

        public string Title
        {
            get { return _title; }
            set 
            {
                if (_title != value)
                {
                    _title = value;
                    this.RaisePropertyChanged(() => this.Title);
                }
            }
        }

        public bool IsEnable
        {
            get { return _isEnable; }
            set 
            {
                if (_isEnable != value)
                {
                    _isEnable = value; 
                    this.RaisePropertyChanged(() => this.IsEnable);
                    this.RaisePropertyChanged(() => this.IsShowWaiting);
                }
            }
        }

        public Visibility IsShowWaiting
        {
            get 
            {
                return this.IsEnable ? Visibility.Collapsed : Visibility.Visible;
                //return  Visibility.Visible;
            }
        }

        public ObservableCollection<ProductViewModel> ProductViewModels
        {
            get { return _productViewModels; }
            set { _productViewModels = value; }
        }

        public ProductViewModel SelectedProductViewModel
        {
            get { return _selectedProductViewModel; }
            set { _selectedProductViewModel = value; }
        }

        public ObservableCollection<LogInfo> Logs
        {
            get { return _logs; }
            set { _logs = value; }
        }

        #endregion

        #region Command / Event

        public event EventHandler<EventArgs> GetViewModelCompleted;
        public event EventHandler<EventArgs> GetLogCompleted;
        public event EventHandler<EventArgs> SendEmailCompleted;

        public ICommand AddCommand { get; set; }

        public ICommand DoubleClickCommand { get; set; }

        #endregion

        #region Constructor

        public ShellViewModel()
        {
            _title = "商品监控 -- By Alex Email:qianjin.qin@outlook.com";
            _productManager = new ProductManager();
            
            IsRegistered();
        }

        #endregion

        #region Private Methods

        private void Start()
        {
            _timer = new System.Timers.Timer(1000 * 5);
            _timer.Elapsed += timer_Elapsed;
            InitializeCommand();
            InitializeData();
        }
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Monitor();
        }

        private void InitializeCommand()
        {
            this.AddCommand = new DelegateCommand(AddExecute);
            this.DoubleClickCommand = new DelegateCommand(DoubleClickExecute);
        }

        private void InitializeData()
        {
            //_productManager.Add(new Product { Id = null, Url = "http://www.rossmannversand.de/produkt/237517/milupa-aptamil-milchbrei-weizen-hirse-hafer.aspx?pos=11&componentid=1", CreateTime = DateTime.Now });
            //bool result = _productManager.Update(new Product { Id = 2, Url = "xxx", CreateTime = DateTime.Now });

            //List<Product> products = ReadFromFile();
            List<Product> products = _productManager.GetAll();
            this.ProductViewModels.Clear();
            foreach (Product item in products)
            {
                Product model = new Product { Id = item.Id, Url = item.Url, CreateTime = item.CreateTime };
                ProductViewModel viewModel = new ProductViewModel(model);
                viewModel.Index = this.ProductViewModels.Count + 1;
                viewModel.DeleteCommand = new DelegateCommand(DeleteExecute);
                if (this.ProductViewModels.Count < _limitCount)
                {
                    this.ProductViewModels.Add(viewModel);
                }
            }
            _timer.Start();
        }

        private void Monitor()
        {
            if (ProductViewModels.Count > 0)
            {
                Parallel.ForEach<ProductViewModel>(ProductViewModels, (p) =>
                {
                    try
                    {
                        Product model = _productManager.Get(p.Url);
                        p.Name = model.Name.Trim();
                        p.Price = model.Price;
                        p.HasProduct = model.HasProduct;
                        if (model.HasProduct)
                        {
                            //System.Media.SystemSounds.Asterisk.Play();
                            Utils.PlaySound();
                        }
                        LogInfo log = new LogInfo
                        {
                            CreateTime = DateTime.Now.ToString(Utils.GeneralDateTimeFormat),
                            Content = "已更新商品 序号 -- " + p.Index
                        };
                        OnGetLogCompleted(log, new EventArgs());
                    }
                    catch (Exception e)
                    {
                        LogInfo log = new LogInfo
                        {
                            CreateTime = DateTime.Now.ToString(Utils.GeneralDateTimeFormat),
                            Content = "更新失败商品 序号 -- " + p.Index
                        };
                        OnGetLogCompleted(log, new EventArgs());
                        Utils.LogError(e.Message, e);
                    }
                });
            }
        }

        private void AddExecute()
        {
            if (this.ProductViewModels.Count < _limitCount)
            {
                EditProductView view = new EditProductView();
                List<Product> list = new List<Product>();
                foreach (var item in ProductViewModels)
                {
                    list.Add(new Product { Url = item.Url });
                }
                EditProductViewModel viewModel = new EditProductViewModel(list);
                viewModel.SaveCompleted += viewModel_SaveCompleted;
                view.DataContext = viewModel;
                view.ShowDialog();
            }
            else
            {
                string message = string.Format("最多只能同时监控{0}个商品", _limitCount);
                MessageBox.Show(message);
            }
        }

        private void viewModel_SaveCompleted(object sender, EventArgs e)
        {
            //Product model = sender as Product;
            //ProductViewModel viewModel = new ProductViewModel(model);
            //viewModel.Index = this.ProductViewModels.Count + 1;
            //this.ProductViewModels.Add(viewModel);

            _timer.Stop();
            InitializeData();
        }

        private List<Product> ReadFromFile()
        {
            List<Product> products = new List<Product>();
            try
            {
                if (!File.Exists(Utils.DateFilePath))
                {
                    XmlHelper.XmlSerializeToFile(products, Utils.DateFilePath, System.Text.Encoding.UTF8);
                }
                products = XmlHelper.XmlDeserializeFromFile<List<Product>>(Utils.DateFilePath, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                MessageBox.Show("程序异常，请重启");
                Utils.LogError(e.Message, e);
            }
            return products;
        }

        private void WriteToFile()
        {
            List<Product> products = new List<Product>();
            foreach (var item in ProductViewModels)
            {
                products.Add(new Product { Url = item.Url });
            }
            XmlHelper.XmlSerializeToFile(products, Utils.DateFilePath, System.Text.Encoding.UTF8);
        }

        private void DeleteExecute()
        {
            MessageBoxResult result = MessageBox.Show("确定删除？", "商品详情网址", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _productManager.Delete(this.SelectedProductViewModel.Model.Id.Value);
                this.ProductViewModels.Remove(this.SelectedProductViewModel);
                //WriteToFile();
            }
        }

        private void DoubleClickExecute()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.SelectedProductViewModel.Url))
                {
                    System.Diagnostics.Process.Start(this.SelectedProductViewModel.Url);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("发生错误，请重启程序!");
                Utils.LogError(e.Message, e);
            }
        }

        private void SendEmail()
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            EmailHelper.SmtpContext netEaseSmtp = new EmailHelper.SmtpContext 
            {
                Server = Utils.EMAILSERVER,
                UserName = Utils.EMAILUSERNAME,
                Password = Utils.EMAILPASSWORD,
                EnableSSL = true    //发送邮件 (SMTP) 服务器 - 需要 TLS2 或 SSL
            };

            string strHostName = Dns.GetHostName(); //得到本机的主机名
            IPAddress[] arrIPAddress = Dns.GetHostAddresses(strHostName); //取得本机IP
            string strAddr = arrIPAddress[0].ToString(); //假设本地主机为单网卡
            string mac = string.Empty;

            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection queryCollection = query.Get();
            foreach (ManagementObject mo in queryCollection)
            {
                if (mo["IPEnabled"].ToString() == "True")
                { 
                    mac = mo["MacAddress"].ToString();
                }
            }

            StringBuilder sbHtmlBody = new StringBuilder();
            sbHtmlBody.AppendFormat("<h4>主机名：{0}</h4>", strHostName);
            sbHtmlBody.AppendFormat("<h4>IP地址：{0}</h4>", strAddr);
            sbHtmlBody.AppendFormat("<h4>Mac地址：{0}</h4>", mac);
            sbHtmlBody.AppendFormat("<h4>注册码：{0}</h4>", GetSerialNum());
            string title = string.Format("Product Monitor Activate {0}", DateTime.Now.ToString(Utils.GeneralDateTimeFormat));
            EmailHelper.SendMail(netEaseSmtp, new List<string> { "qianjin.qin@qq.com", "10078283@qq.com" }, "alexqianjin@163.com", "秦前进", title, sbHtmlBody.ToString());
            //EmailHelper.SendMail(netEaseSmtp, new List<string> { "qianjin.qin@qq.com" }, "alexqianjin@163.com", "秦前进", title, sbHtmlBody.ToString());
            //EmailHelper.SendMail(netEaseSmtp, new List<string> { "qianjin.qin@qq.com", "qqj19890124@163.com" }, "alexqianjin@163.com", "秦前进", title, sbHtmlBody.ToString());
            //sw.Stop();
            //MessageBox.Show((sw.ElapsedMilliseconds / 1000).ToString());
        }

        private string GetSerialNum()
        {
            string result = SoftReg.GetSerialNum();
            if (string.IsNullOrEmpty(result))
            {
                result = SoftReg.Instance.RegNum;
                SoftReg.Instance.SetEncryptNum(result);
            }
            return result;
        }

        private void IsRegistered()
        {
            if(SoftReg.IsRegistered())
            {
                this.IsEnable = true;
                Start();
            }
            else
            {
                this.IsEnable = false;
                Task t = Task.Run(new Action(SendEmail));
                t.ContinueWith((c) => {
                    OnSendEmailCompleted(this, new EventArgs());
                });
            }
        }

        private void viewModel_RegisterCompleted(object sender, EventArgs e)
        {
            Start();
        }

        private void OnGetViewModelCompleted(object sender, EventArgs e)
        {
            if (GetViewModelCompleted != null)
            {
                GetViewModelCompleted(sender, new EventArgs());
            }
        }

        private void OnGetLogCompleted(object sender, EventArgs e)
        {
            if (GetLogCompleted != null)
            {
                GetLogCompleted(sender, e);
            }
        }

        private void OnSendEmailCompleted(object sender, EventArgs e)
        {
            if (SendEmailCompleted != null)
            {
                SendEmailCompleted(sender, e);
            }
        }

        #endregion

        #region Public /Protected Methods

        public void ShowRegisterView()
        {
            this.IsEnable = true;
            RegisterView view = new RegisterView();
            RegisterViewModel viewModel = new RegisterViewModel();
            viewModel.RegisterCompleted += viewModel_RegisterCompleted;
            view.DataContext = viewModel;
            view.ShowDialog();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Practices.Prism.Commands;

using ProductMonitor.Domain.Model;
using ProductMonitor.Utility;
using ProductMonitor.Domain.Interface;
using ProductMonitor.Domain.Implementation;

namespace ProductMonitor.Desktop.ViewModel
{
    public class EditProductViewModel : BaseViewModel
    {
        #region Private Fields

        private string _title = string.Empty;
        private string _url = string.Empty;

        private Product _product = new Product();
        private List<Product> _products = new List<Product>();

        private readonly IProductManager _productManager;

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

        public string Url
        {
            get { return _product.Url; }
            set
            {
                if (_product.Url != value)
                {
                    _product.Url = value;
                    this.RaisePropertyChanged(() => this.Url);
                    this.RaisePropertyChanged(() => this.CanSubmit);
                }
            }
        }

        public bool CanSubmit {
            get { return this.Url == null ? false : !string.IsNullOrEmpty(this.Url.Trim()); }
        }

        #endregion

        #region Command / Event

        public ICommand SaveCommand { get; set; }

        public event EventHandler<EventArgs> SaveCompleted;

        #endregion

        #region Constructor

        public EditProductViewModel(List<Product> products)
        {
            _title = "添加商品详情网址";
            _products = products;

            _productManager = new ProductManager();

            InitializeCommand();
        }

        #endregion

        #region Private Methods

        private void InitializeCommand()
        {
            this.SaveCommand = new DelegateCommand(SaveExecute);
        }

        private void SaveExecute()
        {
            //_products.Add(_product);
            try
            {
                _product.Id = null;
                _product.CreateTime = DateTime.Now;
                _productManager.Add(_product);
                //XmlHelper.XmlSerializeToFile(_products, Utils.DateFilePath, System.Text.Encoding.UTF8);
                OnSaveCompleted(_product, new EventArgs());
            }
            catch (Exception e)
            {
                Utils.LogError(e.Message, e);
                //throw;
            }
        }

        private void OnSaveCompleted(object sender, EventArgs e)
        {
            if (SaveCompleted != null)
            {
                SaveCompleted(sender, e);
            }
        }

        #endregion

        #region Public /Protected Methods

        #endregion
    }
}

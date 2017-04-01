using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using ProductMonitor.Domain.Model;
using ProductMonitor.Domain.Interface;

namespace ProductMonitor.Desktop.ViewModel
{
    public class ProductViewModel : BaseViewModel
    {
        #region Private Fields

        private Product _product = new Product();
        private int _index = 0;

        #endregion

        #region Public / Protected Properties

        public Product Model
        {
            get { return _product; }
        }

        public int Index
        {
            get { return _index; }
            set
            {
                if (_index != value)
                {
                    _index = value;
                    this.RaisePropertyChanged(() => this.Index);
                }
            }
        }

        public string Name
        {
            get { return _product.Name; }
            set
            {
                if (_product.Name != value)
                {
                    _product.Name = value;
                    this.RaisePropertyChanged(() => this.Name);
                }
            }
        }

        public string Price
        {
            get { return _product.Price; }
            set
            {
                if (_product.Price != value)
                {
                    _product.Price = value;
                    this.RaisePropertyChanged(() => this.Price);
                }
            }
        }

        public bool HasProduct
        {
            get { return _product.HasProduct; }
            set
            {
                if (_product.HasProduct != value)
                {
                    _product.HasProduct = value;
                    this.RaisePropertyChanged(() => this.HasProduct);
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
                }
            }
        }

        #endregion

        #region Command / Event

        public ICommand DeleteCommand { get; set; }

        #endregion

        #region Constructor

        public ProductViewModel()
        {

        }

        public ProductViewModel(Product product)
        {
            _product = product;
        }

        #endregion

        #region Private Methods

        #endregion

        #region Public /Protected Methods

        #endregion
    }
}

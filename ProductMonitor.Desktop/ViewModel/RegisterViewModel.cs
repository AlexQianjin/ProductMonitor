using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Practices.Prism.Commands;

using ProductMonitor.Utility;
using System.Windows;

namespace ProductMonitor.Desktop.ViewModel
{
    public class RegisterViewModel : BaseViewModel
    {
        #region Private Fields

        private string _title = string.Empty;
        private string _serialNum = string.Empty;

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

        public string SerialNum
        {
            get { return _serialNum; }
            set
            {
                if (_serialNum != value)
                {
                    _serialNum = value;
                    this.RaisePropertyChanged(() => this.SerialNum);
                    this.RaisePropertyChanged(() => this.CanSubmit);
                }
            }
        }

        public bool CanSubmit {
            get { return this.SerialNum == null ? false : !string.IsNullOrEmpty(this.SerialNum.Trim()); }
        }

        #endregion

        #region Command / Event

        public ICommand SaveCommand { get; set; }

        public event EventHandler<EventArgs> RegisterCompleted;

        #endregion

        #region Constructor

        public RegisterViewModel()
        {
            _title = "软件注册";

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
            try
            {
                //SoftReg reg = new SoftReg(0, Utils.RegKey);
                SoftReg reg = SoftReg.Instance;
                if (this.SerialNum.Trim() == reg.RegNum)
                {
                    bool isSucceed = reg.Register(this.SerialNum.Trim());
                    if (isSucceed)
                    {
                        OnRegisterCompleted(this, new EventArgs());
                    }
                    else
                    {
                        MessageBox.Show("注册码写入失败");
                    }
                }
                else
                {
                    MessageBox.Show("注册码错误");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("注册失败");
                Utils.LogError(e.Message, e);
            }
        }

        private void OnRegisterCompleted(object sender, EventArgs e)
        {
            if (RegisterCompleted != null)
            {
                RegisterCompleted(sender, e);
            }
        }

        #endregion

        #region Public /Protected Methods

        #endregion
    }
}

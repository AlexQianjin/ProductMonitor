using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Management;

namespace ProductMonitor.Utility
{
    public class SoftReg
    {
        #region 声明字段
        private string _CPU;                        //  CPU
        private string _DiskVolumeSerialNumber;     //  DiskVolumeSerialNumber
        private string _MachineNum;                 //  MachineNum
        private string _RegNum;                     //  RegNum
        private int[] intCode = new int[127];       //  存储密钥
        private int[] intNumber = new int[25];      //  存机器码的Ascii值
        private char[] Charcode = new char[25];     //  存储机器码字
        private int _Times;                         //  设置软件使用的次数
        private string _regKey;                     //  设置注册表的Key值
        private bool _isRegNum;                        //  判断是否需要注册

        private static string _snKey = "SN";
        private static string _snValidate = "SNValidate";
        private static string _snExpireDate = "ExpireDate";
        private static double _snDays = 365;

        private static SoftReg _softReg = null;
        #endregion

        #region 完成只读属性
        /// <summary>
        /// CPU 字符串
        /// </summary>
        public string CPU
        {
            get { return _CPU; }
        }

        public string DiskVolumeSerialNumber
        {
            get { return _DiskVolumeSerialNumber; }
        }

        public string MachineNum
        {
            get { return _MachineNum; }
        }

        public string RegNum
        {
            get { return _RegNum; }
        }

        public int Times
        {
            get { return _Times; }
        }

        public string RegKey
        {
            set { _regKey = value; }
        }

        public bool IsRegNum
        {
            get { return _isRegNum; }
            set { _isRegNum = value; }
        }

        public static SoftReg Instance
        {
            get 
            {
                if (_softReg == null)
                {
                    _softReg = new SoftReg(0, Utils.RegKey);
                }
                return _softReg; 
            }
        }

        #endregion

        #region 构造函数
        private SoftReg()
        {
            this._CPU = getCPU();
            this._DiskVolumeSerialNumber = GetDiskVolumeSerialNumber();
            this._MachineNum = getMNum();
            this._RegNum = getRNum();
        }

        private SoftReg(string regKey)
        {
            this._regKey = regKey;
        }

        private SoftReg(int times, string regKey)
        {
            this._Times = times;
            this._regKey = regKey;
            this._CPU = getCPU();
            this._DiskVolumeSerialNumber = GetDiskVolumeSerialNumber();
            this._MachineNum = getMNum();
            this._RegNum = getRNum();
        }

        #endregion

        #region 函数
        /// <summary>
        /// 获取CPU
        /// </summary>
        /// <returns>CPU字符串</returns>
        public string getCPU()
        {
            string strCPU = null;
            ManagementClass myCPU = new ManagementClass("win32_Processor");
            ManagementObjectCollection myCPUConnection = myCPU.GetInstances();
            foreach (ManagementObject myObject in myCPUConnection)
            {
                strCPU = myObject.Properties["Processorid"].Value.ToString();
                break;
            }
            return strCPU;
        }

        /// <summary>
        /// 获取卷轴字符串
        /// </summary>
        /// <returns>卷轴字符串</returns>
        public string GetDiskVolumeSerialNumber()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        }

        /// <summary>
        /// 得到机器码
        /// </summary>
        /// <returns>机器码</returns>
        public string getMNum()
        {
            string strNum = getCPU() + GetDiskVolumeSerialNumber();     //获得24位Cpu和硬盘序列号
            string strMNum = strNum.Substring(0, 24);                   //从生成的字符串中取出前24个字符做为机器码
            return strMNum;
        }

        /// <summary>
        /// 给数组赋值小于10的数
        /// </summary>
        public void setIntCode()
        {
            for (int i = 1; i < intCode.Length; i++)
            {
                intCode[i] = i % 9;
            }
        }

        /// <summary>
        ///  获得注册码
        /// </summary>
        /// <returns>注册码字符串</returns>
        public string getRNum()
        {
            setIntCode();//初始化127位数组
            for (int i = 1; i < Charcode.Length; i++)//把机器码存入数组中
            {
                Charcode[i] = Convert.ToChar(getMNum().Substring(i - 1, 1));
            }
            for (int j = 1; j < intNumber.Length; j++)//把字符的ASCII值存入一个整数组中。
            {
                intNumber[j] = intCode[Convert.ToInt32(Charcode[j])] + Convert.ToInt32(Charcode[j]);
            }

            #region 此处代码 待优化
            string strAsciiName = "";//用于存储注册码
            for (int j = 1; j < intNumber.Length; j++)
            {
                if (intNumber[j] >= 48 && intNumber[j] <= 57)//判断字符ASCII值是否0－9之间
                {
                    strAsciiName += Convert.ToChar(intNumber[j]).ToString();
                }
                else if (intNumber[j] >= 65 && intNumber[j] <= 90)//判断字符ASCII值是否A－Z之间
                {
                    strAsciiName += Convert.ToChar(intNumber[j]).ToString();
                }
                else if (intNumber[j] >= 97 && intNumber[j] <= 122)//判断字符ASCII值是否a－z之间
                {
                    strAsciiName += Convert.ToChar(intNumber[j]).ToString();
                }
                else//判断字符ASCII值不在以上范围内
                {
                    if (intNumber[j] > 122)//判断字符ASCII值是否大于z
                    {
                        strAsciiName += Convert.ToChar(intNumber[j] - 10).ToString();
                    }
                    else
                    {
                        strAsciiName += Convert.ToChar(intNumber[j] - 9).ToString();
                    }
                }
            }
            #endregion
            return strAsciiName;
        }

        /// <summary>
        /// 是否需要注册
        /// </summary>
        /// <returns></returns>
        public bool IsReg()
        {
            _Times = LeftTimes();
            if (_Times == 0 || _Times == -1)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 升序的次数
        /// </summary>
        /// <returns>剩余的次数</returns>
        private int LeftTimes()
        {
            RegistryKey rootKey, regKey;
            rootKey = Registry.CurrentUser.OpenSubKey("Software", true);

            if ((regKey = rootKey.OpenSubKey(_regKey, true)) == null)
            {
                rootKey.CreateSubKey(_regKey);
                regKey = rootKey.OpenSubKey(_regKey, true);
                _Times = _Times - 1;
                regKey.SetValue("LeftTime", (object)(_Times));
                return _Times;
            }

            object leftTime = regKey.GetValue("LeftTime");
            _Times = Int32.Parse(leftTime.ToString()) - 1;

            if (_Times <= 0)
            {
                return -1;
            }
            else
            {
                regKey.SetValue("LeftTime", (object)(_Times));
                return _Times;
            }
        }

        public bool Register(string serialNum)
        {
            bool result = false;
            if (this.RegNum == serialNum)
            {
                RegistryKey rootKey, regKey;
                rootKey = Registry.CurrentUser.OpenSubKey("Software", true);

                if ((regKey = rootKey.OpenSubKey(_regKey, true)) == null)
                {
                    rootKey.CreateSubKey(_regKey);
                }
                regKey = rootKey.OpenSubKey(_regKey, true);
                regKey.SetValue(_snKey, (object)(serialNum));
                regKey.SetValue(_snValidate, (object)DESEncrypt.EncryptString(serialNum));
                string date = DateTime.Now.AddDays(_snDays).ToString(Utils.GeneralDateTimeFormat);
                regKey.SetValue(_snExpireDate, (object)DESEncrypt.EncryptString(date));
                result = true;
            }

            return result;
        }

        public void SetEncryptNum(string serialNum)
        {
            RegistryKey rootKey, regKey;
            rootKey = Registry.CurrentUser.OpenSubKey("Software", true);

            if ((regKey = rootKey.OpenSubKey(_regKey, true)) == null)
            {
                rootKey.CreateSubKey(_regKey);
            }
            regKey = rootKey.OpenSubKey(_regKey, true);
            regKey.SetValue(_snValidate, (object)DESEncrypt.EncryptString(serialNum));
        }

        public static string GetSerialNum()
        {
            string result = string.Empty;
            RegistryKey rootKey, regKey;
            rootKey = Registry.CurrentUser.OpenSubKey("Software", true);

            if ((regKey = rootKey.OpenSubKey(Utils.RegKey, true)) == null)
            {
                rootKey.CreateSubKey(Utils.RegKey);
            }
            regKey = rootKey.OpenSubKey(Utils.RegKey, true);
            object value = regKey.GetValue(_snValidate);

            if (value != null)
            {
                result = DESEncrypt.DecryptString(value.ToString());
            }

            return result;
        }

        public static bool IsRegistered()
        {
            bool result = false;
            RegistryKey rootKey, regKey;
            rootKey = Registry.CurrentUser.OpenSubKey("Software", true);

            if ((regKey = rootKey.OpenSubKey(Utils.RegKey, true)) != null)
            {
                regKey = rootKey.OpenSubKey(Utils.RegKey, true);
                object sn = regKey.GetValue(_snKey);
                object snValidate = regKey.GetValue(_snValidate);
                if (sn != null && snValidate != null)
                {
                    result = sn.ToString() == DESEncrypt.DecryptString(snValidate.ToString());
                }
            }

            return result;
        }

        public static bool IsExpired()
        {
            string result = string.Empty;
            RegistryKey rootKey, regKey;
            rootKey = Registry.CurrentUser.OpenSubKey("Software", true);

            if ((regKey = rootKey.OpenSubKey(Utils.RegKey, true)) == null)
            {
                rootKey.CreateSubKey(Utils.RegKey);
            }
            regKey = rootKey.OpenSubKey(Utils.RegKey, true);
            object value = regKey.GetValue(_snExpireDate);
            if (value == null)
            {
                return true;
            }
            else
            {
                DateTime validDate = DateTime.Parse(DESEncrypt.DecryptString(value.ToString()));
                DateTime compareDate = DateTime.Now.AddDays(_snDays);
                return validDate < compareDate;
            }
        }

        #endregion
    }
}

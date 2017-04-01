using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Media;

using log4net;

namespace ProductMonitor.Utility
{
    public class Utils
    {
        //TODO: Replace these with your email info
        public const string EMAILSERVER = "smtp.163.com";
        public const string EMAILUSERNAME = "qianjin.qin@qq.com";
        public const string EMAILPASSWORD = "yourpassword";

        public static readonly string GeneralDateTimeFormat = "yyyy-MM-dd hh:mm:ss";
        public static readonly string DateFilePath = AppDomain.CurrentDomain.BaseDirectory + "ProductData.xml";
        public static readonly string RegKey = "ProductMonitorKey";
        private static ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetDomainName(string url)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(url))
            {
                string pattern = @"(?<=://)([\w-]+\.)+[\w-]+(?<=/?)";
                Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);
                result = reg.Match(url, 0).Value.Replace("/", string.Empty);
            }
            
            return result;
        }

        public static void PlaySound()
        {
            SoundPlayer player = new SoundPlayer(System.Environment.CurrentDirectory + @"\Resources\Audio\Ring01.wav");
            player.Play();
        }

        public static string Unicode2Chinese(string strUnicode)
        {
            strUnicode = strUnicode.Replace("&#", "");
            string[] arr = strUnicode.Split(';');
            List<string> hexStr = new List<string>();
            if (arr.Count() > 0)
            {
                foreach (var item in arr)
                {
                    if (item.Trim().Length > 0)
                    {
                        string hex = Convert.ToInt32(item).ToString("x4");
                        hexStr.Add(hex);
                    }
                }
            }
            string[] unicodeArray = hexStr.ToArray();
            //string[] splitString = new string[1];
            //splitString[0] = "\\u";
            //splitString[0] = ";";
            //string[] unicodeArray = strUnicode.Split(splitString, StringSplitOptions.RemoveEmptyEntries);
            
            StringBuilder sb = new StringBuilder();

            foreach (string item in unicodeArray)
            {
                byte[] codes = new byte[2];
                int code1, code2;
                code1 = Convert.ToInt32(item.Substring(0, 2), 16);
                code2 = Convert.ToInt32(item.Substring(2), 16);
                codes[0] = (byte)code2;//必须是小端在前
                codes[1] = (byte)code1;
                sb.Append(Encoding.Unicode.GetString(codes));
            }

            return sb.ToString();
        }

        #region Log

        public static void LogError(string message, Exception exception)
        {
            _log.Error(message, exception);
        }

        public static void LogInfo(string message)
        {
            _log.Info(message);
        }

        #endregion
    }
}

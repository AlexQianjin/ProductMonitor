using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ProductMonitor.Utility
{
    /// <summary>
    /// DES加密/解密类。
    /// </summary>
    public class DESEncrypt
    {
        #region 密钥

        private static string key = "alex0124";                                   //密钥(长度必须8位以上)

        #endregion 密钥

        #region DES加密

        public static string EncryptString(string pToEncrypt)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(pToEncrypt);
            des.Key = UTF8Encoding.UTF8.GetBytes(key);
            des.IV = UTF8Encoding.UTF8.GetBytes(key);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            ret.ToString();

            return ret.ToString();
        }

        #endregion DES加密

        #region DES解密

        public static string DecryptString(string pToDecrypt)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            des.Key = UTF8Encoding.UTF8.GetBytes(key);
            des.IV = UTF8Encoding.UTF8.GetBytes(key);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        #endregion DES解密
    }
}
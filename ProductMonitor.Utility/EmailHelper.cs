using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace ProductMonitor.Utility
{
    public class EmailHelper
    {
        public class SmtpContext
        {
            public string Server { get; set; }
            public int Port { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public bool EnableSSL { get; set; }
        }

        private static Encoding myEncoding;
        public static Encoding MyEncoding
        {
            get
            {
                if (myEncoding == null)
                {
                    myEncoding = Encoding.UTF8;
                }
                return myEncoding;
            }
            set
            {
                myEncoding = value;
            }
        }
        public static void SendMail(SmtpContext smtp, List<string> toAddressList, string fromAddress, string fromDisplayName, string title, string htmlBody)
        {
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            foreach (string address in toAddressList)
            {
                msg.To.Add(address);
            }
            msg.From = new MailAddress(fromAddress, fromDisplayName, MyEncoding);//发件人地址，发件人姓名，编码   
            msg.Subject = title;    //邮件标题
            msg.SubjectEncoding = MyEncoding;
            msg.Body = htmlBody;    //邮件内容   
            msg.BodyEncoding = MyEncoding;
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.Normal;

            SmtpClient client = new SmtpClient();
            client.Host = smtp.Server;
            if (smtp.Port != 0)
            {
                client.Port = smtp.Port;//如果不指定端口，默认端口为25
            }
            if (!string.IsNullOrEmpty(smtp.UserName))
            {
                client.Credentials = new System.Net.NetworkCredential(smtp.UserName, smtp.Password);//向SMTP服务器提交认证信息
            }
            client.EnableSsl = smtp.EnableSSL;//默认为false
            client.Send(msg);
        }
    }
}

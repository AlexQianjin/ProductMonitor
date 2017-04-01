using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using ProductMonitor.Domain.Model;

namespace ProductMonitor.Domain.Infrastructrue
{
    public class WebsiteConfig
    {
        private static WebsiteConfig _websiteConfig = null;
        private IDictionary<string, Website> _dic = new Dictionary<string, Website>();
        private WebsiteConfig()
        {

        }

        public static WebsiteConfig Instance
        {
            get 
            {
                if (_websiteConfig == null)
                {
                    _websiteConfig = new WebsiteConfig();
                }
                return _websiteConfig;
            }
        }

        public IDictionary<string, Website> GetWebsites()
        {
            if (_dic.Count == 0)
            {
                foreach (var domain in ConfigurationManager.AppSettings.AllKeys)
                {
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[domain]))
                    {
                        string[] arr = ConfigurationManager.AppSettings[domain].ToString().Split('|');
                        if (arr.Length == 3)
                        {
                            _dic.Add(domain, new Website
                            {
                                Domain = domain,
                                NameXPath = arr[0].Replace("'","\""),
                                PriceXPath = arr[1].Replace("'", "\""),
                                HasProductXPath = arr[2].Replace("'", "\"")
                            });
                        }
                    }
                }
            }
            //IDictionary<string, Website> result = new Dictionary<string, Website>();
            //result.Add("www.rossmannversand.de", new Website
            //{
            //    Domain = "www.rossmannversand.de",
            //    NameXPath = "//h1[@itemprop=\"name\"]",
            //    PriceXPath = "//span[@itemprop=\"price\"]",
            //    HasProductXPath = "//span[@class=\"availability\"]"
            //});

            return _dic;
        }
    }
}

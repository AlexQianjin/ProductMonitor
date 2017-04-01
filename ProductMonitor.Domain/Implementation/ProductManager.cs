using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data;

using HtmlAgilityPack;
using System.Data.SQLite;
using Dapper;

using ProductMonitor.Domain.Model;
using ProductMonitor.Domain.Interface;
using ProductMonitor.Domain.Infrastructrue;
using ProductMonitor.Utility;

namespace ProductMonitor.Domain.Implementation
{
    public class ProductManager : IProductManager
    {
        private IDictionary<string, Website> _websiteDic = new Dictionary<string, Website>();
        private string _dbPath = AppDomain.CurrentDomain.BaseDirectory + "ProductManager.db3";
        private string _connectionString = string.Empty;
        private readonly string _amazonDe = "www.amazon.de";
        private readonly string _amazonDeSelfSupportXPath = "//div[@id='merchant-info']";
        private readonly string _amazonDeSelfSupportLable = "Verkauf und Versand durch Amazon";
        private readonly string _allyouneed = "www.allyouneedfresh.de";
        private readonly string _amazonJp = "www.amazon.co.jp";
        private readonly string _amazonJpSelfSupportXPath = "//*[@id='merchant-info']";
        private readonly string _amazonJpSelfSupportLable = "Amazon.co.jp";

        public ProductManager()
        {
            //_websiteDic = GetWebsites();
            _websiteDic = WebsiteConfig.Instance.GetWebsites();
            _connectionString = string.Format("Data Source={0}", _dbPath);

            CreateTable();
        }

        public Product Get(string url)
        {
            Product product = new Product();
            if (!string.IsNullOrEmpty(url))
            {
                Website website = GetWebsite(url);
                if (website != null)
                {
                    //string result = HttpHelper.SendHttpGetRequest(url);
                    string result = HttpHelper.GetWebHtml(url, null);
                    if (!string.IsNullOrEmpty(result))
                    {
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(result);
                        HtmlNode nameNode = doc.DocumentNode.SelectSingleNode(website.NameXPath);
                        product.Name = nameNode == null ? string.Empty : nameNode.InnerText;
                        HtmlNode priceNode = doc.DocumentNode.SelectSingleNode(website.PriceXPath);
                        product.Price = priceNode == null ? string.Empty : priceNode.InnerText;
                        HtmlNode hasProductNode = doc.DocumentNode.SelectSingleNode(website.HasProductXPath);
                        product.HasProduct = hasProductNode == null ? false : true;
                        if (website.Domain == _amazonDe)
                        {
                            HtmlNode isSelfSupport = doc.DocumentNode.SelectSingleNode(_amazonDeSelfSupportXPath);
                            if (isSelfSupport != null)
                            {
                                int index = isSelfSupport.InnerText.IndexOf(_amazonDeSelfSupportLable);
                                product.HasProduct = index > 0;
                            }
                        }
                        if (website.Domain == _amazonJp)
                        {
                            product.Name = Utils.Unicode2Chinese(product.Name);
                            HtmlNode isSelfSupport = doc.DocumentNode.SelectSingleNode(_amazonJpSelfSupportXPath);
                            if (isSelfSupport != null)
                            {
                                int index = isSelfSupport.InnerText.IndexOf(_amazonJpSelfSupportLable);
                                product.HasProduct = index > 0;
                            }
                        }
                        product.Url = url;
                    }
                }
            }

            return product;
        }

        public List<Product> GetAll()
        {
            List<Product> list = new List<Product>();
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT * FROM Product");
                using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                {
                    list = conn.Query<Product>(sql.ToString()).ToList();
                }

            }
            catch (Exception e)
            {
                Utils.LogError(e.Message, e);
            }
            return list;
        }

        public bool Add(Product model)
        {
            bool result = false;
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO Product (");
                sql.Append(" Id,Url,CreateTime");
                sql.Append(" ) VALUES (");
                sql.Append(":Id,:Url,:CreateTime) ");
                using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                {
                    int affectRows = conn.Execute(sql.ToString(), model);
                    result = affectRows > 0 ? true : false;
                }
                    
            }
            catch (Exception e)
            {
                Utils.LogError(e.Message, e);
            }

            return result;
        }

        public bool Update(Product model)
        {
            bool result = false;
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("UPDATE Product SET ");
                sql.Append(" Url=:Url,");
                sql.Append(" CreateTime=:CreateTime");
                sql.Append(" WHERE Id=:Id");
                using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                {
                    int affectRows = conn.Execute(sql.ToString(), model);
                    result = affectRows > 0 ? true : false;
                }
            }
            catch (Exception e)
            {
                Utils.LogError(e.Message, e);
            }
            return result;
        }

        public bool Delete(int id)
        {
            bool result = false;
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("DELETE FROM Product ");
                sql.Append(" WHERE Id=:Id");
                using (SQLiteConnection _conn = new SQLiteConnection(_connectionString))
                {
                    int affectRows = _conn.Execute(sql.ToString(), new Product { Id = id });
                    result = affectRows > 0 ? true : false;
                }
            }
            catch (Exception e)
            {
                Utils.LogError(e.Message, e);
            }
            return result;
        }

        public void CreateTable()
        {
            
            //如果不存在改数据库文件，则创建该数据库文件 
            if (!System.IO.File.Exists(_dbPath))
            {
                SQLiteDBHelper.CreateDB(_dbPath);
            }
            //SQLiteDBHelper db = new SQLiteDBHelper(dbPath);
            //string sql = "CREATE TABLE Product(Id integer NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,Url varchar(200),CreateTime datetime)";
            //db.ExecuteNonQuery(sql, null); 
        }

        private Website GetWebsite(string url)
        {
            Website website = null;
            if (!string.IsNullOrEmpty(url))
            {
                string key = Utils.GetDomainName(url);
                if (!string.IsNullOrEmpty(key))
                {
                    if (_websiteDic.Keys.Count == 0)
                    {
                        _websiteDic = WebsiteConfig.Instance.GetWebsites();
                    }
                    website = _websiteDic[key];
                }
            }
            return website;
        }

        private List<Product> ConvertDataTableToList(DataTable dt)
        {
            List<Product> list = new List<Product>();
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    Product model = new Product
                    {
                        Id = int.Parse(item[0].ToString()),
                        Url = item[1].ToString(),
                        CreateTime = DateTime.Parse(item[2].ToString())
                    };
                    list.Add(model);
                }  
            }

            return list;
        }
    }
}

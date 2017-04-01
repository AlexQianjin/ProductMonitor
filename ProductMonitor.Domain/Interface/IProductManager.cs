using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProductMonitor.Domain.Model;

namespace ProductMonitor.Domain.Interface
{
    public interface IProductManager
    {
        Product Get(string url);

        List<Product> GetAll();
        
        bool Add(Product model);

        bool Update(Product model);

        bool Delete(int id);

        void CreateTable();
    }
}

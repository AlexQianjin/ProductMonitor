using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ProductMonitor.Domain.Model
{
    [Serializable]
    public class Product
    {
        [XmlIgnore]
        public int? Id { get; set; }

        [XmlIgnore]
        public string Name { get; set; }

        [XmlIgnore]
        public string Price { get; set; }

        [XmlIgnore]
        public bool HasProduct { get; set; }

        public string Url { get; set; }

        [XmlIgnore]
        public DateTime CreateTime { get; set; }
    }
}

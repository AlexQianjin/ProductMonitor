using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductMonitor.Domain.Model
{
    public class Website
    {
        public string Domain { get; set; }

        public string NameXPath { get; set; }

        public string PriceXPath { get; set; }

        public string HasProductXPath { get; set; }
    }
}

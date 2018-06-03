using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Models
{
    public class OrderStatus
    {
        public bool success { get; set; }
        public string message { get; set; }
        public object result { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Models
{
    [Serializable]
    public class Order
    {
        public double Quantity { get; set; }
        public double Rate { get; set; }
    }
}

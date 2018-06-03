using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Models
{
    [Serializable]
    public class OrderBook
    {
        [JsonProperty(PropertyName="buy")]
        public Order[] Buy { get; set; }

        [JsonProperty(PropertyName = "sell")]
        public Order[] Sell { get; set; }

        internal static async Task<OrderBook> Empty()
        {
            return new OrderBook { Buy = new Order[] { }, Sell = new Order[] { } };
        }
    }
}

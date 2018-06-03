using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Models
{
    [Serializable]
    public class TradeHistory
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double Total { get; set; }
        public string FillType { get; set; }
        public string OrderType { get; set; }

        internal static async Task<TradeHistory[]> Empty()
        {
            return new TradeHistory[]{};
        }
    }
}

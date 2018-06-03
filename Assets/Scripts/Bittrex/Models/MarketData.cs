using Aurora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Common
{
    [Serializable]
    public class MarketData
    {
        public string MarketName { get; set; }

        public double Volume { get; set; }
        public double BaseVolume { get; set; }

        public double PercentRise { get; set; }
        public double Stability { get; set; }
        public double Spread { get; set; }
        public double TradesTillSwing { get; set; }

        public TradeHistory[] _TradeHistory { get; set; }
        public OrderBook _OrderBook { get; set; }
    }
}

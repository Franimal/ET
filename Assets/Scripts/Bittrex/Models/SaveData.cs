using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Common
{
    [Serializable]
    public class SaveData
    {
        public MarketData marketData { get; set; }
        public double PriceAfterFiveMinutes { get; set; }

    }
}

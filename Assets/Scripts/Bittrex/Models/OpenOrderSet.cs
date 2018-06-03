using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Models
{
    public class OpenOrderSet
    {
        //public bool Success { get; set; }
        //public string Message { get; set; }

        [JsonProperty(PropertyName = "result")]
        public List<OpenOrder> Result { get; set; }
    }
}

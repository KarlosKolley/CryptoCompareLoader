using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCompareLoader
{
    class CoinListHolder
    {
        public string Response { get; set; } 
        public string BaseLinkUrl { get; set; }
        public ListData[] data { get; set; }
    }
}

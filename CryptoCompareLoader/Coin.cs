using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCompareLoader
{
    public class Coin
    {
        public int Id { get; set; }
		public string Url { get; set; }
		public string Name { get; set; }
		public string CoinName { get; set; }
        public string Algorithm { get; set; }
        public string ProofType { get; set; }
    }
}

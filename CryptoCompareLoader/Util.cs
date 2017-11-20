using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCompareLoader
{
    public class Util
    {
        public static string GetStandardDt(DateTime dt)
        {
            string strYear = dt.Year.ToString();
            string strMonth = "";
            int intMonth = dt.Month;
            if (intMonth < 10) strMonth = "0" + intMonth.ToString();
            else strMonth = intMonth.ToString();
            string strDay = "";
            int intDay = dt.Day;
            if (intDay < 10) strDay = "0" + intDay.ToString();
            else strDay = intDay.ToString();
            string strRetDate = strYear + "-" + strMonth + "-" + strDay;
            return strRetDate;
        }
    }
}

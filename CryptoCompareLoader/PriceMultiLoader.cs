using System;
using System.Text;
using System.Configuration;
using ADOW;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections;
using System.Data;
using System.Threading;
using System.Globalization;

namespace CryptoCompareLoader
{
    public class PriceMultiLoader
    {
        public void LoadData() 
        {
            string DL = "|";
            string RD = "\n";
            //** Get IBM Date
            CommUtil cm = new CommUtil();
            string strDt = cm.GetDateInt();
            string strTimeHR = DateTime.Now.TimeOfDay.Hours.ToString();
            string strTimeMN = DateTime.Now.TimeOfDay.Minutes.ToString();
            if (strTimeHR.Equals("0")) strTimeHR = "";
            if (Int16.Parse(strTimeMN) < 10) strTimeMN = "0" + strTimeMN;
            string strTm = strTimeHR + strTimeMN;

            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            //DateTime dtDateTime = new DateTime(2017, 5, 6, 0, 0, 0, 0, System.DateTimeKind.Utc);
            DateTime dtDateTimeAdj = dtDateTime.AddSeconds(1500176348).ToLocalTime();
            string strUpdt = Util.GetStandardDt(dtDateTime);

            Logger log = Logger.Instance;

            //** Get Json data
            //string input = "https://min-api.cryptocompare.com/data/all/coinlist";
            string input = ""; // "https://min-api.cryptocompare.com/data/pricemultifull?fsyms=ETH&tsyms=BTC,USD";
            string prefix = "https://min-api.cryptocompare.com/data/pricemultifull?fsyms=";
            string suffix = "&tsyms=USD";
            WebClient client = new WebClient();

            string jsonInput = string.Empty;
            string strSymbol = "";
            string id = "";
            string coinid = "";
            string strLine = "";
            int intRecords = 0;

            DBConnect dbConn = DBConnect.Instance;
            JObject strRawUSD = null;
            StringBuilder sb = new StringBuilder();

            DataTable dtCoins = dbConn.GetDataSQL("select coincompid, symbol, coinid from coincaplistcp");
            if(dbConn.ErrNum > 0)
            {
                log.WriteLog(dbConn.ErrMes, 2);
                return;
            }

            log.WriteLog("Start cycle " + DateTime.Now.ToLongTimeString(), 1);
            
            foreach(DataRow dr in dtCoins.Rows)
            {
                id = dr[0].ToString();
                strSymbol = dr[1].ToString();
                coinid = dr[2].ToString();

                input = prefix + strSymbol + suffix;
                try { jsonInput = client.DownloadString(input); }
                catch (Exception ex)
                {
                    log.WriteLog(ex.Message, 2);
                    return;
                }

                strRawUSD = GetCurrentData(jsonInput, strSymbol);
                if (strRawUSD == null) continue;

                strLine = strDt + DL + strTm + DL + id + DL + coinid + DL + strSymbol + DL +
                        strRawUSD.GetValue("MARKET").ToString() + DL +
                        GetTrueValue(strRawUSD, "OPENDAY") + DL +
                        GetTrueValue(strRawUSD, "HIGHDAY") + DL +
                        GetTrueValue(strRawUSD, "LOWDAY") + DL +
                        GetTrueValue(strRawUSD, "OPEN24HOUR") + DL +
                        GetTrueValue(strRawUSD, "HIGH24HOUR") + DL +
                        GetTrueValue(strRawUSD, "LOW24HOUR") + DL +
                        GetTrueValue(strRawUSD.GetValue("MKTCAP").ToString()) + DL +
                        GetTrueValue(strRawUSD.GetValue("SUPPLY").ToString()) + DL +
                        GetTrueValue(strRawUSD.GetValue("TOTALVOLUME24HTO").ToString()) + "\n"; 
                
                sb.Append(strLine);
                intRecords++;

                Thread.Sleep(200);
            }

            string strDataFile = AppDomain.CurrentDomain.BaseDirectory + "pricemultifull.dat";
            try { File.WriteAllText(strDataFile, sb.ToString()); }
            catch (Exception ex)
            {
                log.WriteLog(ex.Message, 2);
                return;
            }

            //** Load datafile
            dbConn.LoadData("coincomplistprice", strDataFile, DL, RD, 1);
            if (dbConn.ErrNum > 0)
            {
                log.WriteLog(dbConn.ErrMes, 2);
                return;
            }

            log.WriteLog("Creating file and loading to database with " + intRecords.ToString() + " records", 0);
            log.WriteLog("Start cycle " + DateTime.Now.ToLongTimeString(), 1);
        }

        private JObject GetCurrentData(string input, string symbol)
        {
            JObject allData = JObject.Parse(input);
            string response = "";
            try { response = allData["Response"].ToString(); }
            catch (Exception ex)
            {
                response = "0";
            }

            if (!response.Equals("0"))
            {
                return null;
            }

            JObject retobject = JObject.Parse(
                        JObject.Parse(
                        JObject.Parse(allData["RAW"].ToString())[symbol].ToString())["USD"].ToString());

            return retobject;
        }

        private string GetTrueValue(string value)
        {
            string result = "";
            if (value.IndexOf('.') > 0) result = value.Substring(0, value.IndexOf('.'));
            else result = value;
            return result;
        }

        private string GetTrueValue(JObject data, string key)
        {
            string result = "";
            try { result = data.GetValue(key).ToString(); }
            catch (Exception ex) { result = "0"; return result; }

            if (result.IndexOf("E") > 0)
            {
                decimal h = Decimal.Parse(result, NumberStyles.Any, CultureInfo.InvariantCulture);
                result = h.ToString();
            }

            return result;
        }
    }
}

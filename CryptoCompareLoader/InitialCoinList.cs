using System;
using System.Text;
using System.Configuration;
using ADOW;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections;

namespace CryptoCompareLoader
{
    class InitialCoinList
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
            string input = "https://min-api.cryptocompare.com/data/all/coinlist";
            //** string input = "https://www.cryptocompare.com/api/data/coinlist/";
            WebClient client = new WebClient();

            string jsonInput = string.Empty;
            try { jsonInput = client.DownloadString(input); }
            catch (Exception ex) {
                log.WriteLog(ex.Message, 2);
                return;
            }

            JObject allCoins = JObject.Parse(jsonInput);
            //int allRecords = allCoins.Count;
            string response = allCoins["Response"].ToString();

            string strLine = "";
            int intRecords = 0;

            StringBuilder sb = new StringBuilder();
            JObject currentCoin = null;

            long lngTotalSupply = 0;

            foreach (JProperty jcoin in allCoins["Data"])
            {
                currentCoin = JObject.Parse(jcoin.Value.ToString());
                bool isres = Int64.TryParse(currentCoin.GetValue("TotalCoinSupply").ToString(), out lngTotalSupply);
                if (!isres) lngTotalSupply = 0;

                strLine = strDt + DL + strTm + DL + currentCoin.GetValue("Id").ToString() + DL +
                          currentCoin.GetValue("CoinName").ToString() + DL +
                          currentCoin.GetValue("Name").ToString() + DL +
                          currentCoin.GetValue("Algorithm").ToString() + DL +
                          currentCoin.GetValue("ProofType").ToString() + DL +
                          lngTotalSupply.ToString() + DL + 
                          currentCoin.GetValue("Url").ToString() + "\n";
                sb.Append(strLine);
                intRecords++;
            }

            //** Create data file
            string strDataFile = AppDomain.CurrentDomain.BaseDirectory + "coinompare.dat";
            try { File.WriteAllText(strDataFile, sb.ToString()); }
            catch (Exception ex) {
                log.WriteLog(ex.Message, 2);
                return;
            }

            DBConnect dbConn = DBConnect.Instance;

            //** Load datafile
            dbConn.LoadData("coincaplistcp", strDataFile, DL, RD, 1);
            if (dbConn.ErrNum > 0) {
                log.WriteLog(dbConn.ErrMes, 2);
                return;
            }

            log.WriteLog("Creating file and loading to database with " + intRecords.ToString() + " records", 0);
        }

    }

    //         "Id": "3808",
    //"Url": "/coins/ltc/overview",
    //"ImageUrl": "/media/19782/ltc.png",
    //"Name": "LTC",
    //"CoinName": "Litecoin",
    //"FullName": "Litecoin (LTC)",
    //"Algorithm": "Scrypt",
    //"ProofType": "PoW",
    //"SortOrder": "2"
}

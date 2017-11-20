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
    public class CoinSnapshot
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
            string prefix = "https://www.cryptocompare.com/api/data/coinsnapshotfullbyid/?id=";
            string input = "";
            WebClient client = new WebClient();

            string jsonInput = string.Empty;
            string strSymbol = "";
            string id = "";
            string coinid = "";
            string strLine = "";
            string strDescr = "";
            string strIcoStatus = "";
            int flag = 0;
            int intRecords = 0;

            DBConnect dbConn = DBConnect.Instance;
            StringBuilder sb = new StringBuilder();

            DataTable dtCoins = dbConn.GetDataSQL("select coincompid, symbol, coinid from coincaplistcp order by coincompid");
            if (dbConn.ErrNum > 0)
            {
                log.WriteLog(dbConn.ErrMes, 2);
                return;
            }

            log.WriteLog("Start cycle " + DateTime.Now.ToLongTimeString(), 1);

            foreach (DataRow dr in dtCoins.Rows)
            {
                id = dr[0].ToString();
                strSymbol = dr[1].ToString();
                coinid = dr[2].ToString();

                input = prefix + id;
                try { jsonInput = client.DownloadString(input); }
                catch (Exception ex)
                {
                    log.WriteLog(ex.Message, 2);
                    return;
                }

                strDescr = GetDescription(jsonInput, out strIcoStatus, out flag);

                if(flag > 0)
                {
                    log.WriteLog(strDescr, 2);
                    return;
                }

                strLine = strDt + DL + strTm + DL + id + DL + coinid + DL + strSymbol + DL + strIcoStatus + DL + strDescr + "\n";

                sb.Append(strLine);
                intRecords++;

                Thread.Sleep(200);
            }

            string strDataFile = AppDomain.CurrentDomain.BaseDirectory + "coinsnapshot.dat";
            try { File.WriteAllText(strDataFile, sb.ToString()); }
            catch (Exception ex)
            {
                log.WriteLog(ex.Message, 2);
                return;
            }

            //** Load datafile
            dbConn.LoadData("coincompldescr", strDataFile, DL, RD, 1);
            if (dbConn.ErrNum > 0)
            {
                log.WriteLog(dbConn.ErrMes, 2);
                return;
            }

            log.WriteLog("Creating file and loading to database with " + intRecords.ToString() + " records", 0);
            log.WriteLog("End cycle " + DateTime.Now.ToLongTimeString(), 1);
        }

        private string GetDescription(string input, out string icostatus, out int flag)
        {
            JObject allData = null;
            JObject retobject = null;
            string response = "";
            string response2 = "";
            flag = 0;

            try {allData = JObject.Parse(input); }
            catch(Exception exx)
            {
                response = exx.Message;
                icostatus = "";
                flag = 1;
                return response;
            }

            icostatus = "";
            try { response = allData["Response"].ToString(); }
            catch (Exception ex)
            {
                response = "0";
            }

            if(response.Equals("Success"))
            {
                retobject = JObject.Parse(
                JObject.Parse(allData["Data"].ToString())["General"].ToString());

                try{ response = retobject.GetValue("Description").ToString(); }
                catch(Exception ex)
                {
                    response = "";
                }

                response = response.Replace("\r\n", "");

                try { response2 = retobject.GetValue("Features").ToString(); }
                catch (Exception ex)
                {
                    response2 = "";
                }

                response2 = response2.Replace("\r\n", "");

                response += response2;

                retobject = JObject.Parse(
                JObject.Parse(allData["Data"].ToString())["ICO"].ToString());

                try { icostatus = retobject.GetValue("Status").ToString(); }
                catch(Exception ex3)
                {
                    icostatus = "";
                }

                response += response2;
            }
            else if (response.Equals("0"))
            {
                response = "";
            }

            return response;
        }
    }
}

using System.Configuration;
using ADOW;
using System.Collections;

namespace CryptoCompareLoader
{
    public class DBConnect
    {
        private int intErr = 0;
        private string strErr = string.Empty;
        private static DBConnect instance;
        private SqlClientBuilder sqMain;

        private DBConnect() {
            sqMain = new SqlClientBuilder(ConfigurationManager.AppSettings["conn"], false);
            if (sqMain.ErrNum > 0) {
                intErr = sqMain.ErrNum;
                strErr = sqMain.ErrMes;
            }
        }

        public static DBConnect Instance
        {
            get {
                if (instance == null) {
                    instance = new DBConnect();
                }
                return instance;
            }

        }

        public int LoadData(string table, string filepath, string fieldbrake, string rowbrake, int truncate) {
            ClearErrors();

            Hashtable htParms = new Hashtable();
            htParms.Add("@table", table);
            htParms.Add("@filepath", filepath);
            htParms.Add("@fieldbreak", fieldbrake);
            htParms.Add("@rowbreak", rowbrake);
            htParms.Add("@truncate", truncate);
            sqMain.SetData("load_table", htParms, false);

            if (sqMain.ErrNum > 0) {
                intErr = sqMain.ErrNum;
                strErr = sqMain.ErrMes;
            }

            return intErr;
        }

        public int ErrNum { get { return intErr; } }
        public string ErrMes { get { return strErr; } }
        private void ClearErrors() {
            intErr = 0;
            strErr = string.Empty;
        }

        public void CloseConn() {
            sqMain.ConnClose();
        }
    }
}

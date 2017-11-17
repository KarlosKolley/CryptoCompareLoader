using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using ADOW;

namespace CryptoCompareLoader
{
    class Logger
    {
        private int intErr = 0;
        private string strErr = string.Empty;
        private string strLogFile = string.Empty;

        private static Logger instance;
        private StreamWriter logWriter;

        private Logger() {
            strLogFile = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["logfile"];
            logWriter = new StreamWriter(strLogFile, true);
        }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
                return instance;
            }

        }

        public void WriteHeader(string header) {
            string strTm = DateTime.Now.TimeOfDay.Hours.ToString() +DateTime.Now.TimeOfDay.Minutes.ToString(); 
            CommUtil cm = new CommUtil();
            string leadtrail = "********************";
            string blank = " ";
            string strDt = cm.GetDateString("-");
            logWriter.WriteLine(leadtrail + blank + header + blank + strDt + blank + "at" + blank + strTm + blank + leadtrail);
        }

        public void WriteLog(string message, int level) {
            string tagOpen = string.Empty;
            string tagClose = string.Empty;
            switch (level) {
                case 0:
                    tagOpen = "<success>";
                    tagClose = "</success>";
                    break;
                case 1:
                    tagOpen = "<warning>";
                    tagClose = "</warning>";
                    break;
                case 2:
                    tagOpen = "<error>";
                    tagClose = "</error>";
                    break;
            }

            logWriter.WriteLine(tagOpen + message + tagClose);
        }

        public void CloseLogger() {
            logWriter.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCompareLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[1];
            args[0] = "1";

            Logger log = Logger.Instance;
            log.WriteHeader("COINCOMPARE LOG");

            DBConnect dconn = DBConnect.Instance;
            if (dconn.ErrNum > 0) {
                log.WriteLog(dconn.ErrMes, 2);
                dconn.CloseConn();
                log.CloseLogger();
                return;
            }

            if (args[0] == "1")
            {
                InitialCoinList icl = new InitialCoinList();
                icl.LoadData();
            }

            //** Cleanup
            try { dconn.CloseConn(); } catch (Exception ex) { }
            try { log.CloseLogger(); } catch (Exception ex) { }
        }
    }
}

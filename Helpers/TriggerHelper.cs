using System;
using System.Data;
using System.Threading;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Twig
{
    public static class TriggerHelper
    {
        /*
            Logic:
             - get all rows from transaction table of type 'trigger'
         */
        public static DataTable GetTriggers()
        {
            var query = SqlHelper.CreateSqlCommand();
            query.CommandText = @"SELECT * FROM transactions 
                                    WHERE type = 'trigger' 
                                ";
            query.Prepare();

            var dt = new DataTable();
            dt.Load(query.ExecuteReader());

            return dt;
        }

        public static void UpdateStockPrice(String stockSymbol)
        {
            //TODO - this
            /*
                Get stock prices from quote cache api

             */
        }
    }
}
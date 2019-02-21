using System;
using System.Data;

namespace Twig
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlHelper.OpenSqlConnection();

            Console.WriteLine("Twig running...");

            var dt = new DataTable();
            dt = TriggerHelper.GetTriggers();

            foreach(DataRow row in dt.Rows){
                // if (row['transactiontime'] 
                // var price = TriggerHelper.UpdateStockPrice(stockSymbol);
            }
        }
    }
}

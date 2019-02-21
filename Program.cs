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

            // if time of tansaction is older than 60 seconds:
            //      updated stock price
            //      check that user has enough stocks to sell
            //       or check if they have enough money to buy
            //      set db row type from trigger to pending
            foreach(DataRow row in dt.Rows){
                // if (row['transactiontime'] 
                // var price = TriggerHelper.UpdateStockPrice(stockSymbol);
            }
        }
    }
}

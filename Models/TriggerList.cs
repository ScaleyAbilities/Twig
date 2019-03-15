using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Twig
{
    public class TriggerList : Dictionary<String, Dictionary<String, Dictionary<String, decimal>>>
    {
        public void Add(String Symbol, String Choice, decimal Price, String u)
        {
            Choice = Choice.ToLower();
            if (!Choice.Equals("buy") && !Choice.Equals("sell")) return;

            // Checks if the StockSymbol and Buy/Sell keys are in the object, adds if not
            if (!this.ContainsKey(Symbol))
            {
                this[Symbol] = new Dictionary<string, Dictionary<String, decimal>>();
                this[Symbol]["buy"] = new Dictionary<String, decimal>();
                this[Symbol]["sell"] = new Dictionary<String, decimal>();
            }

            // Adds the price and user data
            if (this[Symbol][Choice].ContainsKey(u))
            {
                this[Symbol][Choice][u] += Price;
            }
            else
            {
                this[Symbol][Choice].Add(u, Price);
            }
        }

        public void CheckStockTriggers(String Symbol, decimal StockPrice)
        {
            var BuyStock = this[Symbol]["buy"].ToList();
            BuyStock.Sort((x, y) => x.Value.CompareTo(y.Value));

            var SellStock = this[Symbol]["sell"].ToList();
            SellStock.Sort((x, y) => x.Value.CompareTo(y.Value));
            SellStock.Reverse();

            while (BuyStock.Count > 0 && BuyStock[0].Value >= StockPrice)
            {
                System.Console.WriteLine(BuyStock[0]);
                
                JObject twigTrigger = new JObject();
                JObject twigParams = new JObject();
                twigTrigger.Add("usr", BuyStock[0].Key);
                twigTrigger.Add("cmd", "BUY");
                twigParams.Add("stock", Symbol);
                twigParams.Add("price", StockPrice);
                twigTrigger.Add("params", twigParams);
                RabbitHelper.PushTrigger(twigTrigger);
                
                BuyStock.Remove(BuyStock[0]);
            }

            while (SellStock.Count > 0 && SellStock[0].Value <= StockPrice)
            {
                System.Console.WriteLine(SellStock[0]);
                
                JObject twigTrigger = new JObject();
                JObject twigParams = new JObject();
                twigTrigger.Add("usr", SellStock[0].Key);
                twigTrigger.Add("cmd", "SELL");
                twigParams.Add("stock", Symbol);
                twigParams.Add("price", StockPrice);
                twigTrigger.Add("params", twigParams);
                RabbitHelper.PushTrigger(twigTrigger);
                
                SellStock.Remove(SellStock[0]);
            }
        }

        static void Main(string[] args)
        {
            // Display the number of command line arguments:
            System.Console.WriteLine(args.Length + " args");
            String me = "Rick";
            TriggerList obj = new TriggerList();
            obj.Add("aks", "buy", 3.90m, me);
            obj.Add("aks", "buy", 2.99m, me);
            obj.Add("aks", "buy", 5m, me);
            obj.Add("aks", "buy", 1m, me);
            obj.Add("aks", "buy", 2.2m, "ned");
            obj.Add("kek","sell", 4m, "ged");

            obj.CheckStockTriggers("aks", 1m);
            obj.CheckStockTriggers("kek",5m);
            System.Console.WriteLine("Done.");
        }
    }
}
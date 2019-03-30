using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Twig
{
    public class TriggerList : Dictionary<String, Dictionary<String, Dictionary<String, decimal>>>
    {
        public void Add(String Symbol, String Choice, String u, decimal Price)
        {
            Choice = Choice.ToUpper();
            Symbol = Symbol.ToUpper();
            if (!Choice.Equals("BUY") && !Choice.Equals("SELL")) return;

            // Checks if the StockSymbol and Buy/Sell keys are in the object, adds if not
            if (!this.ContainsKey(Symbol))
            {
                this[Symbol] = new Dictionary<string, Dictionary<String, decimal>>();
                this[Symbol]["BUY"] = new Dictionary<String, decimal>();
                this[Symbol]["SELL"] = new Dictionary<String, decimal>();
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

        public async Task CheckStockTriggers(String Symbol, decimal StockPrice)
        {
            Task Buy = Task.Run(() => this.CheckBuy(Symbol,StockPrice));
            Task Sell = Task.Run(() => this.CheckSell(Symbol,StockPrice));
            Task.WaitAll(Buy, Sell);

            // Removes Symbol if there are no Triggers
            await Task.Run(() => {
                if (this[Symbol]["BUY"].Count == 0 && this[Symbol]["SELL"].Count == 0)
                    this.Remove(Symbol);
            });
        }


        public void CheckBuy(String Symbol, decimal StockPrice) {

            var BuyStock = this[Symbol]["BUY"].ToList();
            BuyStock.Sort((x, y) => x.Value.CompareTo(y.Value));

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
                
                this[Symbol]["BUY"].Remove(BuyStock[0].Key);
                BuyStock.Remove(BuyStock[0]);
            }
        }

        public void CheckSell(String Symbol, decimal StockPrice) {

            var SellStock = this[Symbol]["SELL"].ToList();
            SellStock.Sort((x, y) => x.Value.CompareTo(y.Value));
            SellStock.Reverse();

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
                
                this[Symbol]["SELL"].Remove(SellStock[0].Key);
                SellStock.Remove(SellStock[0]);
            }
        }

        public void Remove(String Symbol, String Command, String u) {
            // Does a trigger have to be canceled before they can set a new one?
            if(Command.Equals("CANCEL_BUY")) {
                this[Symbol]["BUY"].Remove(u);
            } else if(Command.Equals("CANCEL_SELL")) {
                this[Symbol]["SELL"].Remove(u);
            }
        }
    }
}
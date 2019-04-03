using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Twig
{
    public class TriggerList : Dictionary<String, Dictionary<String, SortedSet<Trigger>>>
    {
        public void Add(String Symbol, String Choice, String u, decimal Price, String tid)
        {
            Choice = Choice.ToUpper();
            Symbol = Symbol.ToUpper();
            if (!Choice.Equals("BUY") && !Choice.Equals("SELL")) return;

            // Checks if the StockSymbol and Buy/Sell keys are in the object, adds if not
            if (!this.ContainsKey(Symbol))
            {
                this[Symbol] = new Dictionary<string, SortedSet<Trigger>>();
                this[Symbol]["BUY"] = new SortedSet<Trigger>();
                this[Symbol]["SELL"] = new SortedSet<Trigger>();
            }

            // Adds the price and user data
            this[Symbol][Choice].Add(new Trigger(){User = u, Price = Price, Tid = tid});
        }

        public async Task CheckStockTriggers(String Symbol, decimal StockPrice)
        {
            Task Buy = Task.Run(() => CheckBuy(Symbol, StockPrice));
            Task Sell = Task.Run(() => CheckSell(Symbol, StockPrice));
            Task.WaitAll(Buy, Sell);

            // Removes Symbol if there are no Triggers
            await Task.Run(() => {
                if (this[Symbol]["BUY"].Count == 0 && this[Symbol]["SELL"].Count == 0)
                    this.Remove(Symbol);
            });
        }


        public void CheckBuy(String Symbol, decimal StockPrice) {

            var BuyStock = this[Symbol]["BUY"];

            while (BuyStock.Count > 0 && BuyStock.Max.Price >= StockPrice)
            {
                var buy = BuyStock.Max;
                System.Console.WriteLine(buy);
                
                JObject twigTrigger = new JObject();
                JObject twigParams = new JObject();
                twigTrigger.Add("usr", buy.User);
                twigTrigger.Add("cmd", "BUY");
                twigTrigger.Add("queue", buy.Queue);
                twigParams.Add("stock", Symbol);
                twigParams.Add("price", StockPrice);
                twigTrigger.Add("params", twigParams);
                RabbitHelper.PushTrigger(twigTrigger);
                
                BuyStock.Remove(buy);
            }
        }

        public void CheckSell(String Symbol, decimal StockPrice) {

            var SellStock = this[Symbol]["SELL"];

            while (SellStock.Count > 0 && SellStock.Min.Price <= StockPrice)
            {
                var sell = SellStock.Min;
                System.Console.WriteLine(sell);
                
                JObject twigTrigger = new JObject();
                JObject twigParams = new JObject();
                twigTrigger.Add("usr", sell.User);
                twigTrigger.Add("cmd", "SELL");
                twigTrigger.Add("queue", sell.Queue);
                twigParams.Add("stock", Symbol);
                twigParams.Add("price", StockPrice);
                twigTrigger.Add("params", twigParams);
                RabbitHelper.PushTrigger(twigTrigger);
                
                SellStock.Remove(sell);
            }
        }

        public void Remove(String Symbol, String Command, string u) {
            // Does a trigger have to be canceled before they can set a new one?
            if(!this.ContainsKey(Symbol)) return;
            if(Command.Equals("CANCEL_BUY")) {
                this[Symbol]["BUY"].RemoveWhere(t => t.User == u);
            } else if(Command.Equals("CANCEL_SELL")) {
                this[Symbol]["SELL"].RemoveWhere(t => t.User == u);
            }
        }
    }
}
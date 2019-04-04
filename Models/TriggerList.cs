using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Twig
{
    public class TriggerList : ConcurrentDictionary<String, Dictionary<String, SortedSet<Trigger>>>
    {
        public void Add(String Symbol, String Choice, String u, decimal Price, String tid)
        {
            Choice = Choice.ToUpper();
            Symbol = Symbol.ToUpper();
            if (!Choice.Equals("BUY") && !Choice.Equals("SELL")) return;

            // Checks if the StockSymbol and Buy/Sell keys are in the object, adds if not
            if (!this.ContainsKey(Symbol))
                this[Symbol] = new Dictionary<string, SortedSet<Trigger>>();

            if(!this[Symbol].ContainsKey(Choice))
                this[Symbol][Choice] = new SortedSet<Trigger>(new TriggerPriceComparer());

            // Adds the price and user data
            this[Symbol][Choice].Add(new Trigger() { User = u, Price = Price, Tid = tid });
        }

        public void CheckBuy(String Symbol, decimal StockPrice)
        {

            var BuyStock = this[Symbol]["BUY"];

            while (BuyStock.Count > 0 && BuyStock.Max.Price >= StockPrice)
            {
                var buy = BuyStock.Max;

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

        public void CheckSell(String Symbol, decimal StockPrice)
        {
            var SellStock = this[Symbol]["SELL"];

            while (SellStock.Count > 0 && SellStock.Min.Price <= StockPrice)
            {
                var sell = SellStock.Min;

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

        public void Remove(String Symbol, String Command, string u)
        {
            if (!this.ContainsKey(Symbol)) return;
            if (Command.Equals("CANCEL_BUY") && this[Symbol].ContainsKey("BUY"))
            {
                this[Symbol]["BUY"].RemoveWhere(t => t.User == u);
                if(this[Symbol]["BUY"].Count == 0)
                    this[Symbol].Remove("BUY");
            }
            else if (Command.Equals("CANCEL_SELL") && this[Symbol].ContainsKey("SELL"))
            {
                this[Symbol]["SELL"].RemoveWhere(t => t.User == u);
                if(this[Symbol]["SELL"].Count == 0)
                    this[Symbol].Remove("SELL");
            }

            if(this[Symbol].Keys.Count == 0) {
                var s = new Dictionary<string, SortedSet<Trigger>>();
                this.TryRemove(Symbol, out s);
            }
        }
    }
}
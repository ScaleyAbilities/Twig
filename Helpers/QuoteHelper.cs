using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Twig
{
    public static class QuoteHelper
    {
        private static string quoteApi = Environment.GetEnvironmentVariable("QUOTE_API") ?? "http://localhost:5588";

        private static ConcurrentDictionary<string, Tuple<decimal, DateTime>> quoteCache = new ConcurrentDictionary<string, Tuple<decimal, DateTime>>();

        public static async Task<decimal> GetQuote(string stockSymbol, string user, string tid) {
            Tuple<decimal, DateTime> cachedQuote = null;
            quoteCache.TryGetValue(stockSymbol, out cachedQuote);
            
            if (cachedQuote != null && cachedQuote.Item2.AddMinutes(1) <= DateTime.Now) {
                return cachedQuote.Item1;
            }

            // Get value from Cobra
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{quoteApi}/quote/{user}/{stockSymbol}/{tid}");
                response.EnsureSuccessStatusCode();
                var json = new JObject(await response.Content.ReadAsStringAsync());
                return (decimal)json["amount"];
            }
        }
    }
}

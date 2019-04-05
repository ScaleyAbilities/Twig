using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Twig
{
    class Program
    {
        internal static readonly string ServerName = Environment.GetEnvironmentVariable("SERVER_NAME") ?? "Twig";

        static async Task RunCommands(JObject json, TriggerList tl)
        {
            try
            {
                ParamHelper.ValidateParamsExist(json, "cmd", "usr");
            }                
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Error in Queue JSON: {ex.Message}");
                return;
            }

            string command = json["cmd"].ToString().ToUpper();
            string user = json["usr"].ToString();
            JObject commandParams = (JObject)json["params"];
            string stock = commandParams["stock"].ToString();

            Console.WriteLine(command);
            await Task.Run(() => {
                if (command.Equals("BUY") || command.Equals("SELL")) {
                    var tid = json["tid"]?.ToString();
                    var price = (decimal)commandParams["price"];
                    tl.Add(stock, command, user, price, tid);
                } else if (command.Equals("CANCEL_BUY") || command.Equals("CANCEL_SELL")) {
                    tl.Remove(stock, command, user);
                }
            });
        }

        static async Task Main(string[] args)
        {
            var quitSignalled = new TaskCompletionSource<bool>();
            Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, eventArgs) => {
                quitSignalled.SetResult(true);
                eventArgs.Cancel = true; // Prevent program from quitting right away
            });

            TriggerList tl = new TriggerList();
            
            RabbitHelper.CreateConsumer(RunCommands, tl);
            
            Console.WriteLine("Twig running...");
            Console.WriteLine("Press Ctrl-C to exit.");

            while (true)
            {
                var completed = await Task.WhenAny(quitSignalled.Task, Task.Delay(40000));
                Console.WriteLine("Polling");

                if (completed == quitSignalled.Task)
                    break;

                var tasks = tl.Keys.Select(async sym => {
                    if(tl[sym].ContainsKey("BUY")) {
                        Trigger t = tl[sym]["BUY"].Max;
                        if(t != null)
                            tl.CheckBuy(sym, await QuoteHelper.GetQuote(t.User, sym, t.Tid));
                    }
                    if(tl[sym].ContainsKey("SELL")) {
                        Trigger t = tl[sym]["SELL"].Min;
                        if(t != null)
                            tl.CheckSell(sym, await QuoteHelper.GetQuote(t.User, sym, t.Tid));
                    }
                    return;
                });
                await Task.WhenAll(tasks);
            }

            Console.WriteLine("Quitting...");
            Console.WriteLine("Done.");
            Environment.Exit(0);
        }
    }
}

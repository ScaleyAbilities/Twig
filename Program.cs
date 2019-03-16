using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Twig
{
    class Program
    {
        internal static readonly string ServerName = Environment.GetEnvironmentVariable("SERVER_NAME") ?? "Twig";

        static void RunCommands(JObject json)
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
            decimal price = (decimal)commandParams["price"];

            TriggerList tl = new TriggerList();

            if (command.Equals("BUY") || command.Equals("SELL")) {
                tl.Add(stock, command, user, price);
            } else if (command.Equals("CANCEL_BUY") || command.Equals("CANCEL_SELL")) {
                tl.Remove(stock, command, user);
            }
        }

        static async Task Main(string[] args)
        {
            RabbitHelper.CreateConsumer(RunCommands);
            
            Console.WriteLine("Twig running...");

            if (args.Contains("--no-input"))
            {
                while (true)
                {
                    await Task.Delay(int.MaxValue);
                }
            } 
            else 
            {
                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}

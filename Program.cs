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
            string username = json["usr"].ToString();
            string stock = json["stock"].ToString();    
            JObject commandParams = (JObject)json["params"];

            TriggerList tl = new TriggerList();

            // If add command
                // Add
            // If cancel
                // Cancel
            // Run the command to add trigger
            
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

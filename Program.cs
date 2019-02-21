using System;

namespace Twig
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlHelper.OpenSqlConnection();

            Console.WriteLine("Twig running...");

        }
    }
}

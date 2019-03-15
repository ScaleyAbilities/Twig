using System;

namespace Twig
{
    public class Trigger
    {
        public String User { get; set; }
        public decimal? Price { get; set; }

        public Trigger(String user, decimal? price)
        {
            User = user;
            Price = price;
        }
    }
}
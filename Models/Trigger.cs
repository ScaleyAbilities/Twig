using System.Collections.Generic;

namespace Twig
{
    public class Trigger
    {
        public string User { get; set; }
        public decimal Price { get; set; }
        public string Tid { get; set; }
        public string Queue { get; set; }
    }
    internal class TriggerPriceComparer : IComparer<Trigger>
    {
        public int Compare(Trigger x, Trigger y)
        {
            return x.Price.CompareTo(y.Price);
        }
    }
}
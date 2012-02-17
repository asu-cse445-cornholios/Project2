using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project2ModelingLib
{
    public class PriceCutEventArgs : EventArgs
    {
        public double Price { get; set; }

        public PriceCutEventArgs(double price)
        {
            Price = price;
        }
    }
}

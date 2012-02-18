// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;

namespace Project2
{
    public class PriceCutEventArgs : EventArgs
    {
        public PriceCutEventArgs(double price)
        {
            Price = price;
        }

        public double Price { get; set; }
    }
}
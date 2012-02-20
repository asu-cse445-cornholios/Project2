// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;

namespace Project2
{
    /// <summary>
    /// Event arguments to provide the price cut.
    /// </summary>
    public class PriceCutEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceCutEventArgs"/> class.
        /// </summary>
        /// <param name="price">The price.</param>
        public PriceCutEventArgs(double price)
        {
            Price = price;
        }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double Price { get; set; }
    }
}
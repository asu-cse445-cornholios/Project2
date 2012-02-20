﻿// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System.Threading;

namespace Project2
{
    //An Operation Scenario of the e-commerce system is outlined below:
    //(1) The ChickenFarm uses a pricing model to calculate the chicken price. If the new price is lower
    //than the previous price, it emits an event and calls the event handlers in the retailers that have
    //subscribed to the event.
    //(2) A Retailer evaluates the price, generates an OrderObject (consisting of multiple values), and
    //sends the order to the Encoder to convert the order object into a plain string.
    //(3) The Encoder converts the object into a string.
    //(4) The Encoder sends the encoded string back to the caller.
    //(5) The Retailer sends the encoded string to one of the free cells in the MultiCellBuffer.
    //(6) The ChickenFarm receives the encoded string from the MultiCellBuffer and sends the string to
    //the Decoder for decoding.
    //(7) The Decoder sends the OrderObject to the ChickenFarm. The decoded object must contain the
    //same values generated by the Retailer.
    //(8) The ChickenFarm creates a new thread to process the order;
    //(9) The OrderProcessingThread processes the order, e.g., checks the credit card number and
    //calculates the amount.
    //(10) The OrderProcessingThread sends a confirmation to the retailer and prints the order.


    /// <summary>
    /// The Main thread will perform necessary preparation, create the buffer classes, 
    /// instantiate the objects, create threads, and start threads.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            var n = 10;

            var chickenFarm = new ChickenFarm();
            var token = chickenFarm.GetToken();
            var chickenFarmer = new Thread(chickenFarm.FarmSomeChickens) {Name = "TheChickenFarmer"};
            var chickenStore = new Retailer(token);
            chickenFarm.PriceCut += chickenStore.OnPriceCut;
            var retailerThreads = new Thread[n];
            for (var index = 0; index < retailerThreads.Length; index++)
            {
                retailerThreads[index] = new Thread(chickenStore.RunStore) {Name = "Retailer" + (index + 1)};
                retailerThreads[index].Start();
                while (!retailerThreads[index].IsAlive)
                {
                    ;
                }
            }
            chickenFarmer.Start();
            chickenFarmer.Join();
            foreach (var retailerThread in retailerThreads)
            {
                retailerThread.Join();
            }
        }
    }
}
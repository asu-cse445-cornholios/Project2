// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Project2
{
    /// <summary>
    /// Retailer1 through RetailerN, each retailer is a thread instantiated from the same class
    /// (or the same method) in a class. The retailers’ actions are event-driven. Each retailer
    /// contains a call-back method (event handler) for the CheckenFarm to call when a price-cut
    /// event occurs. The retailer will calculate the number of chickens to order, for example,
    /// based on the need and the difference between the previous price and the current price. The
    /// thread will terminate after the ChickenFarm thread has terminated. Each order is an
    /// OrderClass object. The object is sent to the Encoder for encoding. The encoded string is
    /// sent back to the retailer. Then, the retailer will send the order in String format to the
    /// MultiCellBuffer. Before sending the order to the MultiCellBuffer, a time stamp must be
    /// saved. When the confirmation of order completion is received, the time of the order will
    /// be calculated and saved (or printed).
    /// </summary>
    public class Retailer
    {
        private readonly ManualResetEvent priceCutManualResetEvent = new ManualResetEvent(false);
        private readonly object syncRoot = new object();
        private double chickenPrice;
        private DateTime timeSent;
        private CancellationToken token;

        /// <summary>
        /// Initializes a new instance of the <see cref="Retailer"/> class.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Retailer(CancellationToken cancellationToken)
        {
            token = cancellationToken;
        }

        /// <summary>
        /// Called when [price cut].
        /// </summary>
        /// <param name="src">The SRC.</param>
        /// <param name="e">The <see cref="Project2.PriceCutEventArgs"/> instance containing the event data.</param>
        public void OnPriceCut(object src, PriceCutEventArgs e) //event handler
        {
            lock (syncRoot)
            {
                chickenPrice = e.Price;
            }
            priceCutManualResetEvent.Set();
        }

        /// <summary>
        /// Runs the store to send orders to the chicken farm.
        /// </summary>
        public void RunStore()
        {
            var orderTimes = new List<long>();
            var baseChickens = Thread.CurrentThread.ManagedThreadId * 23;
            var chickenDemand = Thread.CurrentThread.ManagedThreadId * 7 / 2;
            // Loop until stop is requested
            while (!token.IsCancellationRequested)
            {
                WaitHandle.WaitAny(new[] { priceCutManualResetEvent, token.WaitHandle });
                if (!token.IsCancellationRequested) //did cancellation wake us? 
                {
                    int numChickens;
                    lock (syncRoot)
                    {
                        // Determine what to order based on price and demand.
                        numChickens = baseChickens - chickenDemand * (int)(chickenPrice);
                    }

                    // Put in order for chickens
                    if (numChickens > 0)
                    {
                        int ccNumber = Math.Min(7000, 5000 + Thread.CurrentThread.ManagedThreadId + numChickens);
                        var OrderObject = new OrderClass
                                              {
                                                  Amount = numChickens,
                                                  SenderId = Thread.CurrentThread.Name,
                                                  CardNo = ccNumber
                                              };
                        //sends order to encoder
                        string encoded = OrderObject.Encode();
                        timeSent = DateTime.UtcNow;
                        //send encoded string to free cell in multiCellBuffer
                        var cell = new MultiCellBuffer(token);
                        try
                        {
                            cell.SetOneCell(encoded);

                            // Wait for order confirmation
                            var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset,
                                                                      Thread.CurrentThread.Name);
                            WaitHandle.WaitAny(new[] { eventWaitHandle, token.WaitHandle });

                            DateTime timeReceive = DateTime.UtcNow;
                            TimeSpan elapsedTime = timeReceive - timeSent;

                            Console.WriteLine("The order for {0} took {1} ms.", Thread.CurrentThread.Name,
                                              elapsedTime.Milliseconds);
                            orderTimes.Add(elapsedTime.Milliseconds);
                        }
                        catch (OperationCanceledException e)
                        {
                            Debug.WriteLine("A cancellation for {0} is requested.", Thread.CurrentThread.Name);
                        }
                    }
                }
            }
            double averageOrderTime = orderTimes.Count == 0 ? 0.0 : orderTimes.Average();
            Console.WriteLine("{0}: ARTOC: {1} ms", Thread.CurrentThread.Name, averageOrderTime);
        }
    }
}
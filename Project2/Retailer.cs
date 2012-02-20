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
    public class Retailer
    {
        private readonly ManualResetEvent priceCutManualResetEvent = new ManualResetEvent(false);
        private readonly object syncRoot = new object();
        private double chickenPrice;
        private DateTime timeSent;
        private CancellationToken token;

        public Retailer(CancellationToken cancellationToken)
        {
            token = cancellationToken;
        }

        public void OnPriceCut(object src, PriceCutEventArgs e) //event handler
        {
            lock (syncRoot)
            {
                chickenPrice = e.Price;
            }
            priceCutManualResetEvent.Set();
        }

        public void RunStore()
        {
            var orderTimes = new List<long>();
            var baseChickens = Thread.CurrentThread.ManagedThreadId*23;
            var chickenDemand = Thread.CurrentThread.ManagedThreadId*7/2;
            // Loop until stop is requested
            while (!token.IsCancellationRequested)
            {
                WaitHandle.WaitAny(new[] {priceCutManualResetEvent, token.WaitHandle});
                if (!token.IsCancellationRequested) //did cancellation wake us? 
                {
                    int numChickens;
                    lock (syncRoot)
                    {
                        // Determine what to order based on price and demand.
                        numChickens = baseChickens - chickenDemand*(int) (chickenPrice);
                    }

                    // Put in order for chickens
                    if (numChickens > 0)
                    {
                        var rand = new Random();
                        var OrderObject = new OrderClass
                                              {
                                                  Amount = numChickens,
                                                  SenderId = Thread.CurrentThread.Name,
                                                  CardNo = rand.Next(5000, 7000)
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
                            WaitHandle.WaitAny(new[] {eventWaitHandle, token.WaitHandle});

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
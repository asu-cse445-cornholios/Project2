// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Project2
{
    public class Retailer
    {
        private double chickenPrice;
        private readonly object syncRoot = new object();
        private bool shouldOrder;
        private volatile bool shouldStop;
        private CancellationTokenSource cancelSource = new CancellationTokenSource();
        private DateTime timeSent;

        public Retailer()
        {
        }

        public void OnPriceCut(object src, PriceCutEventArgs e) //event handler
        {
            lock (syncRoot)
            {
                chickenPrice = e.Price;
                shouldOrder = true;
                Monitor.PulseAll(syncRoot);
            }
        }

        public void RequestStop()
        {
            shouldStop = true;
           //cancelSource.Cancel();
            lock (syncRoot)
            {
                shouldOrder = true;
                Monitor.PulseAll(syncRoot);
            }
        }

        public void RunStore()
        {
            var orderTimes = new List<long>();
            var random = new Random();
            var baseChickens = random.Next(1, 10);
            var chickenDemand = random.Next(1, 10);
            // Wait until stop is requested
            while (!shouldStop)
            {
                // Wait for price cut
                lock (syncRoot)
                {
                    while (!shouldOrder)
                    {
                        Monitor.Wait(syncRoot);
                    }
                    shouldOrder = false;
                    Monitor.PulseAll(syncRoot);
                }
                if (shouldStop)
                    return;
                // Determine what to order

                // Put in order

                int numChickens = baseChickens - chickenDemand * (int)(chickenPrice);
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
                    var cell = new MultiCellBuffer();
                    cell.SetOneCell(encoded, cancelSource.Token);


                    // Wait for order confirmation
                    var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset,
                                                              Thread.CurrentThread.Name);
                    eventWaitHandle.WaitOne();

                    DateTime timeReceive = DateTime.UtcNow;
                    TimeSpan elapsedTime = timeReceive - timeSent;

                    Console.WriteLine("Time of order for {0}: {1}", Thread.CurrentThread.Name, elapsedTime);
                    orderTimes.Add(elapsedTime.Milliseconds);
                }
            }
            double averageOrderTime = orderTimes.Average();
            System.Console.WriteLine("{0}: ARTOC: {1}", Thread.CurrentThread.Name, averageOrderTime);
        }
    }
}
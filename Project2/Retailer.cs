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
        private volatile bool shouldStop;
        private DateTime timeSent;
        private CancellationToken token;
        private ManualResetEvent priceCutManualResetEvent = new ManualResetEvent(false);

        public Retailer(CancellationToken cancellationToken)
        {
            this.token = cancellationToken;
        }

        public void OnPriceCut(object src, PriceCutEventArgs e) //event handler
        {
            lock (syncRoot)
            {
                chickenPrice = e.Price;
                priceCutManualResetEvent.Set();
            }
        }

        public void RunStore()
        {
            var orderTimes = new List<long>();
            var random = new Random();
            var baseChickens = random.Next(1, 10);
            var chickenDemand = random.Next(1, 10);
            // Wait until stop is requested
            while (!token.IsCancellationRequested)
            {
                WaitHandle.WaitAny(new[] { priceCutManualResetEvent, token.WaitHandle });
                if (!token.IsCancellationRequested) //did cancellation wake us? 
                {

                    // Determine what to order

                    int numChickens = baseChickens - chickenDemand*(int) (chickenPrice);

                    // Put in order

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
                            eventWaitHandle.WaitOne();

                            DateTime timeReceive = DateTime.UtcNow;
                            TimeSpan elapsedTime = timeReceive - timeSent;

                            Console.WriteLine("Time of order for {0}: {1}", Thread.CurrentThread.Name, elapsedTime);
                            orderTimes.Add(elapsedTime.Milliseconds);

                        }
                        catch (OperationCanceledException e)
                        {
                            System.Console.WriteLine("Thread {0} is cancelled.", Thread.CurrentThread.Name);
                        }
                    }
                }
            }
            double averageOrderTime = orderTimes.Average();
            System.Console.WriteLine("{0}: ARTOC: {1}", Thread.CurrentThread.Name, averageOrderTime);
        }
    }
}
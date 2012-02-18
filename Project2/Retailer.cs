// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Threading;

namespace Project2
{
    public class Retailer
    {
        private readonly int numberOfOrders;
        private double previousPrice;
        private object syncRoot = null;

        private DateTime timeSent;

        public Retailer(int numberOfOrders)
        {
            this.numberOfOrders = numberOfOrders;
        }

        public void OnPriceCut(object src, PriceCutEventArgs e) //event handler
        {
            int numChickens;

            numChickens = 100 + 2 * (int)(previousPrice - e.Price);

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
            cell.SetOneCell(encoded);

            lock (syncRoot)
            {
                previousPrice = e.Price;
            }
        }

        public void RunStore()
        {
            var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset,
                                                      Thread.CurrentThread.Name);
            eventWaitHandle.WaitOne();
            DateTime timeReceive = DateTime.UtcNow;
            TimeSpan elapsedTime = timeReceive - timeSent;

            Console.WriteLine("Time of order{0}: {1}", Thread.CurrentThread.Name, elapsedTime);
        }
    }
}
// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace Project2
{
    public class ChickenFarm
    {
        private readonly ReaderWriterLockSlim chickenPriceReaderWriterLockSlim = new ReaderWriterLockSlim();
        private readonly List<double> prices = new List<double>();
        private double chickenPrice = 10.00;

        private int p; //farm lifelength
        private DateTime timeFarmStarted;

        public ChickenFarm()
        {
            prices.Add(11.45);
            prices.Add(14.29);
            prices.Add(32.34);
            prices.Add(24.29);
            prices.Add(13.07);
            prices.Add(36.75);
            prices.Add(38.49);
            prices.Add(13.26);
            prices.Add(35.53);
            prices.Add(12.54);
            prices.Add(20.83);
            prices.Add(43.70);
            prices.Add(25.50);
            prices.Add(32.80);
            prices.Add(13.09);
            prices.Add(19.97);
            prices.Add(31.85);
            prices.Add(26.80);
            prices.Add(24.11);
            prices.Add(26.67);
            prices.Add(40.32);
            prices.Add(41.39);
            prices.Add(28.54);
            prices.Add(35.95);
            prices.Add(30.46);
            prices.Add(34.65);
            prices.Add(38.96);
            prices.Add(12.23);
            prices.Add(24.12);
            prices.Add(41.71);
            prices.Add(29.20);
        }

        public event EventHandler<PriceCutEventArgs> PriceCut;

        public void OnPriceCut(PriceCutEventArgs e)
        {
            var handler = PriceCut;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private double PricingModel()
        {
            return prices[0];
        }

        public void FarmSomeChickens()
        {
            timeFarmStarted = DateTime.UtcNow;
            UpdatePrice();
            var multiCellBuffer = new MultiCellBuffer();
            var updatePriceTimer = new System.Timers.Timer {Interval = 500};
            updatePriceTimer.Elapsed += UpdatePriceTimerElapsed;
            while (p < 10)
            {
                var newOrderEncoded = multiCellBuffer.GetOneCell();
                var newOrder = OrderClass.Decode(newOrderEncoded);
                var orderProcessing = new OrderProcessing(newOrder, GetPrice());
                var OrderProcessingThread = new Thread(orderProcessing.ProcessOrder);
                OrderProcessingThread.Start();
                UpdatePrice();
            }

            Console.WriteLine("Total Time: {0}", DateTime.UtcNow - timeFarmStarted);
        }

        private void UpdatePriceTimerElapsed(object sender, ElapsedEventArgs e)
        {
            UpdatePrice();
        }

        private double GetPrice()
        {
            chickenPriceReaderWriterLockSlim.EnterReadLock();
            try
            {
                return chickenPrice;
            }
            finally
            {
                chickenPriceReaderWriterLockSlim.ExitReadLock();
            }
        }

        private void UpdatePrice()
        {
            chickenPriceReaderWriterLockSlim.EnterWriteLock();
            try
            {
                double previousPrice = chickenPrice;
                chickenPrice = PricingModel();
                if (chickenPrice < previousPrice)
                {
                    OnPriceCut(new PriceCutEventArgs(chickenPrice));
                    Interlocked.Increment(ref p);
                }
            }
            finally
            {
                chickenPriceReaderWriterLockSlim.ExitWriteLock();
            }
        }
    }
}
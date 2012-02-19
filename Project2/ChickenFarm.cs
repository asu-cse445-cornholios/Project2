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
        private double chickenPrice = 10.00;
        private int p; //farm lifelength
        private DateTime timeFarmStarted;
        private DateTime timeLastOrdered;
        private System.Timers.Timer updatePriceTimer = new System.Timers.Timer { Interval = 500 };
        private double averageNumberOfChickens;
        private CancellationTokenSource cancelSource = new CancellationTokenSource();
        private object syncRoot = new object();
        private bool shouldRun = true;

        public ChickenFarm()
        {
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
            if ((DateTime.UtcNow - timeLastOrdered).Milliseconds > 500)
            {
                return chickenPrice - 1.50;
            }
            else
            {
                return 1 + 1 * (averageNumberOfChickens);
            }
        }

        public void FarmSomeChickens()
        {
            timeFarmStarted = DateTime.UtcNow;
            UpdatePrice();
            var multiCellBuffer = new MultiCellBuffer();
            
            updatePriceTimer.Start();
            updatePriceTimer.Elapsed += UpdatePriceTimerElapsed;
            while (shouldRun)
            {
                try
                {
                    var newOrderEncoded = multiCellBuffer.GetOneCell(cancelSource.Token);
                    var newOrder = OrderClass.Decode(newOrderEncoded);
                    averageNumberOfChickens = (newOrder.Amount + averageNumberOfChickens) / 2;
                    timeLastOrdered = DateTime.UtcNow;
                    var orderProcessing = new OrderProcessing(newOrder, GetPrice());
                    var OrderProcessingThread = new Thread(orderProcessing.ProcessOrder) { Name = "OrderProcessing" + newOrder.SenderId };
                    OrderProcessingThread.Start();
                }
                catch (OperationCanceledException e)
                {
                }
            }

            updatePriceTimer.Stop();

            Console.WriteLine("Total Time: {0}", DateTime.UtcNow - timeFarmStarted);
        }

        private void UpdatePriceTimerElapsed(object sender, ElapsedEventArgs e)
        {
            updatePriceTimer.Enabled = false;
            UpdatePrice();
            updatePriceTimer.Enabled = true;
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
                    lock (syncRoot)
                    {
                        p++;
                        if (p > 10)
                        {
                            shouldRun = false;
                            if (!cancelSource.IsCancellationRequested)
                            {
                                cancelSource.Cancel();
                            }
                        }
                    }
                }
            }
            finally
            {
                chickenPriceReaderWriterLockSlim.ExitWriteLock();
            }
        }
    }
}
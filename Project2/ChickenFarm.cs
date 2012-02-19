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
        private object syncRoot = new object();
        private CancellationTokenSource cancelSource = new CancellationTokenSource();

        public event EventHandler<PriceCutEventArgs> PriceCut;

        public void OnPriceCut(PriceCutEventArgs e)
        {
            var handler = PriceCut;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public CancellationToken GetToken()
        {
            return cancelSource.Token;
        }

        private double PricingModel()
        {
            if ((DateTime.UtcNow - timeLastOrdered).Milliseconds > 500)
            {
                return chickenPrice - 1.50;
            }
            else
            {
                return 6.50 + 1.3 * (averageNumberOfChickens - 1);
            }
        }

        public void FarmSomeChickens()
        {
            CancellationToken token = cancelSource.Token;
            timeFarmStarted = DateTime.UtcNow;
            UpdatePrice();
            var multiCellBuffer = new MultiCellBuffer(token);
            
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string newOrderEncoded;
                    bool completed = multiCellBuffer.GetOneCell(out newOrderEncoded);
                    if (completed)
                    {
                        var newOrder = OrderClass.Decode(newOrderEncoded);
                        averageNumberOfChickens = (newOrder.Amount + averageNumberOfChickens)/2;
                        timeLastOrdered = DateTime.UtcNow;
                        var orderProcessing = new OrderProcessing(newOrder, GetPrice());
                        var OrderProcessingThread = new Thread(orderProcessing.ProcessOrder)
                                                        {Name = "OrderProcessing" + newOrder.SenderId};
                        OrderProcessingThread.Start();
                    }
                    UpdatePrice();
                }
                catch (OperationCanceledException e)
                {

                }
            }
            Console.WriteLine("Total Time: {0}", DateTime.UtcNow - timeFarmStarted);
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
                            cancelSource.Cancel();
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
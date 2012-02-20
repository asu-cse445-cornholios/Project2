// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading;

namespace Project2
{
    public class ChickenFarm
    {
        private readonly CancellationTokenSource cancelSource = new CancellationTokenSource();
        private readonly ReaderWriterLockSlim chickenPriceReaderWriterLockSlim = new ReaderWriterLockSlim();
        private readonly object syncRoot = new object();
        private double averageNumberOfChickens;
        private double chickenPrice = 10.00;
        private int p; //farm lifelength
        private DateTime timeFarmStarted;
        private DateTime timeLastOrdered;

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
            if ((DateTime.UtcNow - timeLastOrdered).Milliseconds > 85)
            {
                return chickenPrice - .65;
            }
            else
            {
                return 6.50 + .5*(averageNumberOfChickens - 1);
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
                    if (multiCellBuffer.GetOneCell(out newOrderEncoded))
                    {
                        var newOrder = OrderClass.Decode(newOrderEncoded);
                        averageNumberOfChickens = (newOrder.Amount + averageNumberOfChickens)/2.0;
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
                    Debug.WriteLine("A cancellation for {0} is requested.", Thread.CurrentThread.Name);
                }
            }
            Console.WriteLine("Total Time: {0} ms", (DateTime.UtcNow - timeFarmStarted).Milliseconds);
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
                chickenPrice = Math.Round(PricingModel(), 2);
                if (chickenPrice < previousPrice)
                {
                    lock (syncRoot)
                    {
                        p++;
                        if (p > 10)
                        {
                            cancelSource.Cancel();
                        }
                        else
                        {
                            OnPriceCut(new PriceCutEventArgs(chickenPrice));
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
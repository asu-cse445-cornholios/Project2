// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading;

namespace Project2
{
    /// <summary>
    /// ChickenFarm is a class on the server side: It will be started as a thread by the Main
    /// method and will perform a number of service functions. It uses a PricingModel to determine
    /// the chicken prices. It defines a price-cut event that can emit an event and call the event
    /// handlers in the Retailers if there is a price-cut according to the PricingModel. It
    /// receives the orders (in string) from the MultiCellBuffer. It calls the Decoder to convert
    /// the string into the order object. For each order*, it starts a new thread (resulting in
    /// multiple threads for processing multiple orders*) from OrderProcessing class (or method)
    /// to process the order based on the current price. There is a counter p in the ChickenFarm.
    /// After p (e.g., p = 10) price cuts have been made, the ChickenFarm thread will terminate.
    /// Before generating the first price, a time stamp must be saved. Before the thread
    /// terminates, the total time used will be calculated and saved (or printed).
    /// </summary>
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

        /// <summary>
        /// Occurs when [price cut].
        /// </summary>
        public event EventHandler<PriceCutEventArgs> PriceCut;

        /// <summary>
        /// Raises the <see cref="E:PriceCut"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Project2.PriceCutEventArgs"/> instance containing the event data.</param>
        public void OnPriceCut(PriceCutEventArgs e)
        {
            var handler = PriceCut;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <returns>A CancellationToken.</returns>
        public CancellationToken GetToken()
        {
            return cancelSource.Token;
        }

        /// <summary>
        /// Decides the price of chickens. It can increase price or cut the price. You can define a
        /// mathematical model* (formula) to determine the price based on the order received within a
        /// given time period and the number of chickens the farm can produce in the same time period.
        /// You can use a simple hard-coded table (a sequence of prices). However, you must make sure
        /// that your model will allow the price goes up some time and goes down some other time.
        /// </summary>
        /// <returns></returns>
        private double PricingModel()
        {
            if ((DateTime.UtcNow - timeLastOrdered).Milliseconds > 85)
            {
                return chickenPrice - .65;
            }
            else
            {
                return 6.50 + .5 * (averageNumberOfChickens - 1);
            }
        }

        /// <summary>
        /// Farms some chickens.
        /// </summary>
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
                        averageNumberOfChickens = (newOrder.Amount + averageNumberOfChickens) / 2.0;
                        timeLastOrdered = DateTime.UtcNow;
                        var orderProcessing = new OrderProcessing(newOrder, GetPrice());
                        var OrderProcessingThread = new Thread(orderProcessing.ProcessOrder) { Name = "OrderProcessing" + newOrder.SenderId };
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

        /// <summary>
        /// Gets the price in a thread-safe way.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the price in a thread-safe way.
        /// </summary>
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
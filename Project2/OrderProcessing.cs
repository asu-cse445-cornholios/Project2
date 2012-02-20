// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Threading;

namespace Project2
{
    /// <summary>
    /// OrderProcessing is a class or a method in a class on server side. Whenever an order needs
    /// to be processed, a new thread is instantiated from this class (or method) to process the
    /// order. It will check the validity of the credit card number. You can define your credit
    /// card format, for example, the credit card number from the retailers must be a number
    /// registered to the ChickenFarm, or a number between two given numbers (e.g., between 5000
    /// and 7000). Each OrderProcessing thread will calculate the total amount of charge, e.g.,
    /// unitPrice*NoOfChickens + Tax + shippingHandling. It will send a confirmation to the
    /// retailer when an order is completed.
    /// </summary>
    public class OrderProcessing
    {
        private const double TaxRate = .08;
        private const double ShippingHandlingRatePerChicken = .50;

        private readonly OrderClass OrderObject;
        private readonly double chickenPrice;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderProcessing"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="price">The price.</param>
        public OrderProcessing(OrderClass order, double price)
        {
            OrderObject = order;
            chickenPrice = price;
        }

        /// <summary>
        /// Checks the credit card.
        /// </summary>
        /// <param name="ccNumber">The cc number.</param>
        /// <returns></returns>
        private bool CheckCreditCard(int ccNumber)
        {
            return ccNumber >= 5000 && ccNumber <= 7000;
        }

        /// <summary>
        /// Calculates the charge.
        /// </summary>
        /// <param name="chickens">The chickens.</param>
        /// <param name="unitPrice">The unit price.</param>
        /// <returns></returns>
        private double CalculateCharge(int chickens, double unitPrice)
        {
            double total = chickens * (unitPrice + ShippingHandlingRatePerChicken);
            return TaxRate * total + total;
        }

        /// <summary>
        /// Processes the order.
        /// </summary>
        public void ProcessOrder()
        {
            if (CheckCreditCard(OrderObject.CardNo))
            {
                double finalCharge = CalculateCharge(OrderObject.Amount, chickenPrice);
                Console.WriteLine(
                    "Order from {0}\n\tAmount: {1}\n\tCredit Card Number: {2}\n\tChicken Price: {3}\n\tOrder Cost: {4}",
                    OrderObject.SenderId, OrderObject.Amount, OrderObject.CardNo, chickenPrice, finalCharge);
            }
            else
            {
                Console.WriteLine(
                    "Order from {0} has invalid credit card number {1}.", OrderObject.SenderId, OrderObject.CardNo);
                throw new Exception("Invalid Credit Card.");
            }
            var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset,
                                                      OrderObject.SenderId);
            eventWaitHandle.Set();
        }
    }
}
// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Threading;

namespace Project2
{
    public class OrderProcessing
    {
        private static readonly double TaxRate = .08;
        private static readonly double ShippingHandlingRatePerChicken = .50;

        private readonly OrderClass OrderObject;
        private readonly double chickenPrice;

        public OrderProcessing(OrderClass order, double price)
        {
            OrderObject = order;
            chickenPrice = price;
        }

        private bool CheckCreditCard(int ccNumber)
        {
            return ccNumber >= 5000 && ccNumber <= 7000;
        }

        private double CalculateCharge(int chickens, double unitPrice)
        {
            double total = chickens * (unitPrice + ShippingHandlingRatePerChicken);
            return TaxRate * total + total;
        }

        public void ProcessOrder()
        {
            if (!CheckCreditCard(OrderObject.CardNo))
            {
                throw new Exception("Invalid Credit Card.");
            }
            double finalCharge = CalculateCharge(OrderObject.Amount, chickenPrice);
            var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset,
                                                       OrderObject.SenderId);
            eventWaitHandle.Set();
            System.Console.WriteLine("Order from {0}\n\tAmount: {1}\n\tCredit Card Number: {2}\n\tChicken Price: {3}\n\tOrder Cost: {4}\n", OrderObject.SenderId, OrderObject.Amount, OrderObject.CardNo, chickenPrice, finalCharge);
        }
    }
}
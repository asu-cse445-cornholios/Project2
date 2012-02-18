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
            double total = chickens*(unitPrice + ShippingHandlingRatePerChicken);
            return TaxRate*total + total;
        }

        public void ProcessOrder()
        {
            int chickens;
            int ccNumber;
            string senderId;
            lock (OrderObject)
            {
                chickens = OrderObject.Amount;
                ccNumber = OrderObject.CardNo;
                senderId = OrderObject.SenderId;
            }
            if (!CheckCreditCard(ccNumber))
            {
                throw new Exception("Invalid Credit Card.");
            }
            double finalCharge = CalculateCharge(chickens, chickenPrice);
            var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset,
                                                      senderId);
            eventWaitHandle.Reset();
        }
    }
}
﻿// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.Serialization;

namespace Project2
{
    public class OrderClass
    {
        /// <summary>
        ///   the number of chickens to purchase.
        /// </summary>
        private int amount;

        /// <summary>
        ///   the credit card number.
        /// </summary>
        private int cardNo;

        /// <summary>
        ///   The identity of the sender.
        /// </summary>
        private string senderId;

        /// <summary>
        ///   The identity of the sender.
        /// </summary>
        public string SenderId
        {
            get { return senderId; }
            set { senderId = value; }
        }

        /// <summary>
        ///   the credit card number.
        /// </summary>
        public int CardNo
        {
            get { return cardNo; }
            set { cardNo = value; }
        }

        /// <summary>
        ///   the number of chickens to purchase.
        /// </summary>
        public int Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public DateTime CreatedTime { get; set; }


        public TimeSpan OrderTime { get; set; }

        /// <summary>
        ///   Encodes the order into a string type.
        /// </summary>
        /// <returns> The order encoded as a string. </returns>
        public string Encode()
        {
            var serializer =
                new XmlSerializer(typeof (OrderClass));
            var writer = new StringWriter();
            serializer.Serialize(writer, this);
            return writer.ToString();
        }

        /// <summary>
        ///   Creates an order object from a encoded order string.
        /// </summary>
        /// <param name="orderString"> The string representing the order. </param>
        /// <returns> The order. </returns>
        public static OrderClass Decode(string orderString)
        {
            var serializer =
                new XmlSerializer(typeof (OrderClass));
            var reader = new StringReader(orderString);
            return (OrderClass) serializer.Deserialize(reader);
        }


        // These are to meet requirements. C# uses properties so use the properties instead.
        public void setID(string id)
        {
            senderId = id;
        }

        public string getID()
        {
            return senderId;
        }

        public void setCardNo(int number)
        {
            cardNo = number;
        }

        public int getCardNo()
        {
            return cardNo;
        }

        public int getAmount()
        {
            return amount;
        }

        public void setAmount(int amt)
        {
            amount = amt;
        }
    }
}
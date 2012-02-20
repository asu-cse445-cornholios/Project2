// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Project2
{
    /// <summary>
    /// MultiCellBuffer class is used for the communication between the retailers (clients) and
    /// the chicken farm (server): This class has n (e.g., n = 4) data cells. The number of cells
    /// available must be less than the max number of retailers in your experiment. A setOneCell
    /// and getOneCell methods can be defined to write data into and to read data from one of the
    /// available cells. You can use a semaphore of value n to manage the cells. You cannot use a
    /// queue for the buffer, which is a different data structure.
    /// </summary>
    public class MultiCellBuffer
    {
        private static readonly BlockingCollection<string> buffer =
            new BlockingCollection<string>(8);


        private CancellationToken token;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiCellBuffer"/> class.
        /// </summary>
        /// <param name="token">The token.</param>
        public MultiCellBuffer(CancellationToken token)
        {
            this.token = token;
        }

        /// <summary>
        /// Gets the one cell in a thread-safe way.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns></returns>
        public bool GetOneCell(out string cell)
        {
            if (token.IsCancellationRequested)
            {
                // observe cancellation 
                throw new OperationCanceledException(token); // acknowledge cancellation 
            }

            return buffer.TryTake(out cell, 500, token);
        }
        /// <summary>
        /// Sets an available cell in the buffer call is blocked if no available cells
        /// </summary>
        /// <param name="order">The order.</param>
        public void SetOneCell(string order)
        {
            if (token.IsCancellationRequested)
            {
                // observe cancellation 
                throw new OperationCanceledException(token); // acknowledge cancellation 
            }
            buffer.Add(order, token);
        }
    }
}
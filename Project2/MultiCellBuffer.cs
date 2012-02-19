// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Threading;
using System.Collections.Concurrent;

namespace Project2
{
    public class MultiCellBuffer
    {
        private static readonly BlockingCollection<string> buffer =
            new BlockingCollection<string>(8);


        private CancellationToken token;

        public MultiCellBuffer(CancellationToken token)
        {
            this.token = token;
        }

        public bool GetOneCell(out string cell)
        {
            if (token.IsCancellationRequested)
            {
                // observe cancellation 
                throw new OperationCanceledException(token); // acknowledge cancellation 
            }

            return buffer.TryTake(out cell, 500, token); ;
        }

        //sets an available cell in the buffer
        //call is blocked if no available cells
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
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
            new BlockingCollection<string>(10);


        public string GetOneCell()
        {
            return buffer.Take();
        }

        //sets an available cell in the buffer
        //call is blocked if no available cells
        public void SetOneCell(string order)
        {
            buffer.Add(order);
        }
    }
}
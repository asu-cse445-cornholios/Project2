// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System;
using System.Threading;

namespace Project2
{
    public class MultiCellBuffer
    {
        private static readonly String[] buffer = new String[10];
        private static readonly SemaphoreSlim Sem = new SemaphoreSlim(buffer.Length);
        private static readonly Random cell = new Random();

        public string GetOneCell()
        {
            //choose a random cell for fairness (?)
            int i = cell.Next(0, buffer.Length - 1);

            //temp string variable to be returned
            string order;

            if (Sem.CurrentCount < buffer.Length)
            {
                Monitor.Enter(buffer);

                //spin around the buffer looking for a full cell to return
                while (true)
                {
                    if (buffer[i] != null)
                    {
                        order = buffer[i];
                        buffer[i] = null;
                        Monitor.Exit(buffer);
                        Sem.Release();
                        return order;
                    }

                    //increase/reset cell index
                    i = (i >= buffer.Length - 1 ? 0 : i + 1);
                }
            }

            //if buffer empty, return null
            //(shouldnt happen since getOneCell is called after an order is placed and an event is raised)
            return null;
        }

        //sets an available cell in the buffer
        //call is blocked if no available cells
        public void SetOneCell(string order)
        {
            Sem.Wait();
            int i = cell.Next(0, buffer.Length - 1);

            //spin around the buffer looking for an empty cell
            //(there should be one since we entered the semaphore)
            Monitor.Enter(buffer);
            while (true)
            {
                if (buffer[i] == null)
                {
                    buffer[i] = order;
                    Monitor.Exit(buffer);
                    return;
                }

                i = (i == buffer.Length - 1 ? 0 : i + 1);
            }
        }
    }
}
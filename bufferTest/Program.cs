using System;
using System.Threading;

namespace bufferTest
{
    

    public class Example
    {
        
        public static void Main()
        {
            Thread[] p = new Thread[15];
            Thread[] c = new Thread[15];
            // Create and start five numbered threads. 
            //
            for (int i = 0; i < 15; i++)
            {
                p[i] = new Thread(new ParameterizedThreadStart(producer));
                c[i] = new Thread(new ParameterizedThreadStart(consumer));

                // Start the thread, passing the number.
                //
                
            }

            for (int i = 0; i < 15; i++) {
                p[i].Start(i);
            }
            for (int i = 0; i < 15; i++)
            {
                c[i].Start(i);
            }
            // Wait for half a second, to allow all the
            // threads to start and to block on the semaphore.
            //
            Thread.Sleep(1000);

            Console.WriteLine("Main thread exits.");
        }

        private static void producer(object num)
        {
            MultiCellBuffer b = new MultiCellBuffer();
            b.SetOneCell(num.ToString());
            Console.WriteLine("Producer {0} set one cell", num);

        }
        private static void consumer(object num)
        {
            MultiCellBuffer b = new MultiCellBuffer();
            string a = b.GetOneCell();
            Console.WriteLine("Consumer {0} got cell {1}", num, a);

        }

    }
}

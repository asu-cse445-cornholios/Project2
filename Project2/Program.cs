// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using System.Threading;

namespace Project2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var n = 10;// = int.Parse(args[0]); //retailer

            var chickenFarm = new ChickenFarm();
            var chickenFarmer = new Thread(chickenFarm.FarmSomeChickens) {Name = "TheChickenFarmer"};
            
            var chickenStore = new Retailer();
            chickenFarm.PriceCut += chickenStore.OnPriceCut;
            var retailerThreads = new Thread[n];
            for (var index = 0; index < retailerThreads.Length - 1; index++)
            {
                retailerThreads[index] = new Thread(chickenStore.RunStore) {Name = "Retailer" + (index + 1)};
                retailerThreads[index].Start();
                while (!retailerThreads[index].IsAlive);
            }
            chickenFarmer.Start();
            chickenFarmer.Join();
            chickenStore.RequestStop();

        }
    }
}
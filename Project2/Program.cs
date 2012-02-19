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
            var token = chickenFarm.GetToken();
            var chickenFarmer = new Thread(chickenFarm.FarmSomeChickens) {Name = "TheChickenFarmer"};
            var chickenStore = new Retailer(token);
            chickenFarm.PriceCut += chickenStore.OnPriceCut;
            var retailerThreads = new Thread[n];
            for (var index = 0; index < retailerThreads.Length; index++)
            {
                retailerThreads[index] = new Thread(chickenStore.RunStore) {Name = "Retailer" + (index + 1)};
                retailerThreads[index].Start();
                while (!retailerThreads[index].IsAlive);
            }
            chickenFarmer.Start();
            chickenFarmer.Join();
            System.Console.WriteLine("Done");
        }
    }
}
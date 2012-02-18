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
            var m = 5;// int.Parse(args[1]); //number of orders

            var chickenFarm = new ChickenFarm();
            var chickenFarmer = new Thread(chickenFarm.FarmSomeChickens) {Name = "TheChickenFarmer"};
            
            var chickenStore = new Retailer(m);
            chickenFarm.PriceCut += chickenStore.OnPriceCut;
            var retailers = new Thread[n];
            for (var index = 0; index < retailers.Length; index++)
            {
                retailers[index] = new Thread(chickenStore.RunStore) {Name = "Retailer" + (index + 1)};

                retailers[index].Start();
            }
            chickenFarmer.Start();
            chickenFarmer.Join();

            // TODO: Exit Retailer threads
        }
    }
}
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
            var n = int.Parse(args[0]);
            var m = int.Parse(args[1]);

            var chickenFarm = new ChickenFarm();
            var chickenFarmer = new Thread(chickenFarm.FarmSomeChickens) {Name = "TheChickenFarmer"};
            chickenFarmer.Start();
            var chickenStore = new Retailer(m);
            chickenFarm.PriceCut += chickenStore.OnPriceCut;
            var retailers = new Thread[n];
            for (var index = 0; index < retailers.Length; index++)
            {
                retailers[index] = new Thread(chickenStore.RunStore) {Name = "Retailer" + (index + 1)};

                retailers[index].Start();
            }
            chickenFarmer.Join();

            // TODO: Exit Retailer threads
        }
    }
}
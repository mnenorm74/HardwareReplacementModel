using System;
using System.Collections.Generic;

namespace IDZ
{
    class Program
    {
        static void Main(string[] args)
        {
            var solver = new HardwareReplacementSolver();
            solver.ShowMenu();
            var firstCenter = new List<int> {10, 5, 7, 12, 6, 4}; 
            var secondCenter = new List<int> {7, 9, 5, 3, 8, 10}; 
            var thirdCenter = new List<int> {4, 15, 27, 14, 6, 9}; 
            var fourthCenter = new List<int> {7, 10, 17, 6, 8, 12};
            var model = new List<List<int>>{firstCenter, secondCenter, thirdCenter, fourthCenter};
            solver.SetRegionCenters(model);
            Console.WriteLine("ok");
        }
    }
}

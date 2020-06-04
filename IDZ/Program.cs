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
            var citiesDistances = new List<List<int>> {firstCenter, secondCenter, thirdCenter, fourthCenter};
            var firstRegion = new List<int> {0, 50, 80, 100};
            var secondRegion = new List<int> {50, 0, 75, 94};
            var thirdRegion = new List<int> {80, 75, 0, 68};
            var fourthRegion = new List<int> {100, 94, 68, 0};
            var regionDistances = new List<List<int>> {firstRegion, secondRegion, thirdRegion, fourthRegion};
            solver.SetRegionCenters(citiesDistances, regionDistances);
            solver.Solve();
        }
    }
}
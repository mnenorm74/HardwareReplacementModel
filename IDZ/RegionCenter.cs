using System.Collections.Generic;

namespace IDZ
{
    public class RegionCenter
    {
        public readonly List<City> Cities;

        public RegionCenter(IEnumerable<int> distances)
        {
            var counter = 1;
            Cities = new List<City>();
            foreach (var distance in distances)
            {
                Cities.Add(new City(distance, counter));
                counter++;
            }
        }
    }
}
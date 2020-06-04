using System.Collections.Generic;

namespace IDZ
{
    public class RegionCenter
    {
        public readonly List<City> Cities;

        public RegionCenter(IEnumerable<int> distances)
        {
            Cities = new List<City>();
            foreach (var distance in distances)
            {
                Cities.Add(new City(distance));
            }
        }
    }
}
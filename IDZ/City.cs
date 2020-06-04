namespace IDZ
{
    public class City
    {
        public readonly int Distance;
        public bool Visited;
        public int Position;

        public City(int distance, int position)
        {
            Distance = distance;
            Position = position;
            Visited = false;
        }
    }
}
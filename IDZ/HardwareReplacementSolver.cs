using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDZ
{
    public class HardwareReplacementSolver
    {
        private const int maxWorkTime = 720;
        private double pastDayTime;
        private readonly List<double> pastMonthTime;
        private Tuple<int, int> position;
        private readonly Tuple<int, int> storagePosition;
        private readonly Tuple<int, int> homePosition;
        private readonly int carSpeed;
        private readonly int replacementTime;
        private readonly List<RegionCenter> regionCenters;
        private int weeksCount;
        private int brigadeCount;
        private int carCapacity;
        private int[] drivingSequence;
        private int resources;
        private List<List<int>> regionDistances;
        private readonly List<Tuple<int, int>> path;
        private int engineerMoney;
        private int driverMoney;
        private int businessTripMoney;
        private int hotelMoney;

        public HardwareReplacementSolver(int carSpeed = 50, int replacementTime = 70)
        {
            this.carSpeed = carSpeed;
            this.replacementTime = replacementTime;
            position = new Tuple<int, int>(1, 0);
            storagePosition = new Tuple<int, int>(1, 0);
            homePosition = new Tuple<int, int>(1, 0);
            pastMonthTime = new List<double>();
            regionCenters = new List<RegionCenter>();
            regionDistances = new List<List<int>>();
            drivingSequence = new int[4];
            path = new List<Tuple<int, int>>();
        }

        public void Solve()
        {
            foreach (var centerNumber in drivingSequence)
            {
                var nextCenter = centerNumber;
                RefreshResources();
                ReplaceCar(position, new Tuple<int, int>(centerNumber, 0));
                ServeRegionCenter(nextCenter);
            }

            Console.WriteLine("Конец");
            ShowResult();
        }

        private void ShowResult()
        {
            Console.WriteLine($"Маршрут: {GetPath(path)}");
            Console.WriteLine("Где {номер регионального центра}/{номер города (1=А...6=F)}");
            Console.WriteLine($"Количество бригад: {brigadeCount}");
            Console.WriteLine($"Зарплата инженеров: {engineerMoney}");
            Console.WriteLine($"Зарплата водителей: {driverMoney}");
            Console.WriteLine($"Расходы на автомобили: {pastMonthTime.Count * 5000 * brigadeCount}");
            Console.WriteLine($"Командировочные: {businessTripMoney}");
            Console.WriteLine($"Затраты на отель {hotelMoney}");
        }

        private string GetPath(List<Tuple<int, int>> sequence)
        {
            var result = new StringBuilder();
            foreach (var position in sequence)
            {
                result.Append($"{position.Item1}/{position.Item2} ");
            }

            return result.ToString();
        }

        private void ServeRegionCenter(int number)
        {
            var cities = regionCenters[number - 1].Cities;
            ServeCities(cities);
            ServeCenter(position.Item1);
        }

        private void ServeCenter(int centerNumber)
        {
            var centerPosition = new Tuple<int, int>(centerNumber, 0);
            ReplaceCar(position, centerPosition);
            for (var i = 0; i < 3; i++)
            {
                RefreshResources();
                if (pastDayTime + replacementTime > maxWorkTime)
                {
                    ReplaceCar(position, new Tuple<int, int>(centerNumber, 1));
                    DelayForSleep();
                }

                ReplaceCar(position, centerPosition);
                DelayForFixing();
            }
        }

        private void ServeCities(List<City> cities)
        {
            while (!cities.TrueForAll(x => x.Visited))
            {
                foreach (var city in cities.Where(city => !city.Visited))
                {
                    RefreshResources();
                    var cityPosition = new Tuple<int, int>(position.Item1, city.Position);
                    var distanceToCity = regionCenters[position.Item1 - 1].Cities[city.Position - 1].Distance;
                    var timeToCity = GetReplacementTime(distanceToCity, carSpeed);
                    if (pastDayTime + timeToCity + replacementTime > maxWorkTime)
                    {
                        DelayForSleep();
                    }

                    ReplaceCar(position, cityPosition);
                    DelayForFixing();
                    path.Add(position);
                    city.Visited = true;
                    CheckWorkTime();
                }
            }
        }

        private void CheckWorkTime()
        {
            var workTime = pastDayTime;
            var timeToHome = GetReplacingTime(position, homePosition);
            if (workTime + timeToHome > maxWorkTime)
            {
                DelayForSleep();
            }
        }

        private double GetReplacingTime(Tuple<int, int> from, Tuple<int, int> to)
        {
            var time = 0.0;
            var currentPosition = new Tuple<int, int>(from.Item1, from.Item2);
            var homePosition = to;

            if (currentPosition.Item2 != 0)
            {
                var distance = regionCenters[currentPosition.Item1 - 1].Cities[currentPosition.Item2 - 1].Distance;
                var timeToRegionCenter = GetReplacementTime(distance, carSpeed);
                time += timeToRegionCenter;
                currentPosition = new Tuple<int, int>(currentPosition.Item1, 0);
            }

            if (currentPosition.Item1 != homePosition.Item1)
            {
                var distance = regionDistances[currentPosition.Item1 - 1][homePosition.Item1 - 1];
                var timeToCenter = GetReplacementTime(distance, carSpeed);
                time += timeToCenter;
                currentPosition = new Tuple<int, int>(homePosition.Item1, 0);
            }

            if (currentPosition.Item2 != homePosition.Item2)
            {
                var distance = regionCenters[position.Item1 - 1].Cities[homePosition.Item2 - 1].Distance;
                var timeToCity = GetReplacementTime(distance, carSpeed);
                time += timeToCity;
                currentPosition = homePosition;
            }

            return time;
        }

        private void DelayForFixing()
        {
            Console.WriteLine($"Починка {position.Item1}/{position.Item2}");
            pastDayTime += replacementTime;
            resources--;
        }

        private void DelayForSleep()
        {
            Console.WriteLine($"Гостиница {position.Item1}/{position.Item2}");
            pastMonthTime.Add(replacementTime);
            pastDayTime = 0;
            businessTripMoney += 1000 * 2;
            hotelMoney += 800 * 2;
        }

        private void RefreshResources()
        {
            if (resources < 1)
            {
                ReceiveResources();
            }
        }

        private void ReceiveResources()
        {
            if (position.Equals(storagePosition))
            {
                resources = carCapacity;
            }
            else
            {
                var timeToStorage = GetReplacingTime(position, storagePosition);
                var oldPosition = position;
                if (pastDayTime + 2 * timeToStorage > maxWorkTime)
                {
                    DelayForSleep();
                }

                ReplaceCar(position, storagePosition);
                resources = carCapacity;
                ReplaceCar(position, oldPosition);
            }

            Console.WriteLine($"Ресурсы обновлены: {resources}");
        }

        private void ReplaceCar(Tuple<int, int> from, Tuple<int, int> to)
        {
            if (from.Equals(to))
            {
                return;
            }

            var time = 0.0;

            if (from.Item2 != 0)
            {
                var distance = regionCenters[from.Item1 - 1].Cities[from.Item2 - 1].Distance;
                var timeToRegionCenter = GetReplacementTime(distance, carSpeed);
                time += timeToRegionCenter;
                position = new Tuple<int, int>(from.Item1, 0);
            }

            if (position.Item1 != to.Item1)
            {
                var distance = regionDistances[from.Item1 - 1][to.Item1 - 1];
                var timeToCenter = GetReplacementTime(distance, carSpeed);
                time += timeToCenter;
                position = new Tuple<int, int>(to.Item1, 0);
            }

            if (position.Item2 != to.Item2)
            {
                var distance = regionCenters[position.Item1 - 1].Cities[to.Item2 - 1].Distance;
                var timeToCity = GetReplacementTime(distance, carSpeed);
                time += timeToCity;
                position = to;
            }

            pastDayTime += time;
        }

        private double GetReplacementTime(int distance, int speed)
        {
            return (double) distance / speed * 60;
        }

        public void SetRegionCenters(IEnumerable<List<int>> distances,
            List<List<int>> regionDistances)
        {
            foreach (var distance in distances)
            {
                regionCenters.Add(new RegionCenter(distance));
            }

            this.regionDistances = regionDistances;
        }

        public void ShowMenu()
        {
            brigadeCount = GetBrigadeCount();
            weeksCount = GetWeeksCount();
            carCapacity = GetCarCapacity();
            drivingSequence = GetDrivingSequence();
            ShowModelSettings();
        }

        private int GetWeeksCount()
        {
            Console.WriteLine("Замена блоков может быть реализована в следующие сроки:");
            Console.WriteLine("Вариант 1 - 2 недели");
            Console.WriteLine("Вариант 2 - 4 недели");
            Console.WriteLine("Вариант 3 - 6 недель");
            Console.WriteLine("Для выбора одного из вариантов введите число от 1 до 3:");
            var timeVariant = Console.ReadLine();
            var isValidVariant = int.TryParse(timeVariant, out var weeksCount);
            if (!isValidVariant || weeksCount < 1 || weeksCount > 3)
            {
                Console.WriteLine("Некорректный ввод, попробуйте снова");
                GetWeeksCount();
            }

            if (weeksCount == 1)
            {
                engineerMoney = 25000 * brigadeCount;
                driverMoney = 22500 * brigadeCount;
            }

            if (weeksCount == 2)
            {
                engineerMoney = 50000 * brigadeCount;
                driverMoney = 45000 * brigadeCount;
            }

            if (weeksCount == 3)
            {
                engineerMoney = 75000 * brigadeCount;
                driverMoney = 67500 * brigadeCount;
            }

            return weeksCount * 2;
        }

        private int GetCarCapacity()
        {
            Console.WriteLine("Укажите вместимость автомобиля (минимальное значение: 3)");
            var capacityVariant = Console.ReadLine();
            var isValidVariant = int.TryParse(capacityVariant, out var capacity);
            if (!isValidVariant || capacity < 3)
            {
                Console.WriteLine("Некорректный ввод, попробуйте снова");
                GetCarCapacity();
            }

            return capacity;
        }

        private int[] GetDrivingSequence()
        {
            Console.WriteLine("Укажите порядок движения по региональным центрам.");
            Console.WriteLine("Существует 4 региональных центра: от 1 до 4.");
            Console.WriteLine("Пример ввода: 1234");
            var sequenceVariant = Console.ReadLine();
            var sequence = new int[4];
            if (sequenceVariant.Length == 4)
            {
                for (var i = 0; i < 4; i++)
                {
                    var isValidVariant = int.TryParse(sequenceVariant[i].ToString(), out var currentCenter);
                    if (isValidVariant && currentCenter > 0 && currentCenter < 5)
                    {
                        sequence[i] = currentCenter;
                    }
                    else
                    {
                        Console.WriteLine("Некорректный ввод, попробуйте снова");
                        GetDrivingSequence();
                    }
                }
            }
            else
            {
                Console.WriteLine("Некорректный ввод, попробуйте снова");
                GetDrivingSequence();
            }

            return sequence;
        }

        private void ShowModelSettings()
        {
            Console.WriteLine($"Срок замены блоков (в неделях): {weeksCount}");
            Console.WriteLine($"Количество бригад: {brigadeCount}");
            Console.WriteLine($"Вместимость автомобиля: {carCapacity}");
            Console.Write("Порядок движения по региональным центрам: ");
            foreach (var centerNumber in drivingSequence)
            {
                Console.Write(centerNumber);
            }

            Console.WriteLine();
        }
        
        private int GetBrigadeCount()
        {
            Console.WriteLine("Введите необходимое число рабочих бригад (минимальное значение: 1)");
            var brigadeVariant = Console.ReadLine();
            var isValidVariant = int.TryParse(brigadeVariant, out var brigadeCount);
            if (!isValidVariant || brigadeCount < 1)
            {
                Console.WriteLine("Некорректный ввод, попробуйте снова");
                GetBrigadeCount();
            }

            return brigadeCount;
        }
    }
}
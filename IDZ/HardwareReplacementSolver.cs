using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDZ
{
    public class HardwareReplacementSolver
    {
        private const int maxWorkTime = 720;
        private double[] pastDayTime;
        private List<double>[] pastMonthTime;
        private Tuple<int, int>[] position;
        private readonly Tuple<int, int> storagePosition;
        private readonly Tuple<int, int> homePosition;
        private readonly int carSpeed;
        private readonly int replacementTime;
        private readonly List<RegionCenter> regionCenters;
        private int weeksCount;
        private int brigadeCount;
        private int carCapacity;
        private int[] drivingSequence;
        private int[] resources;
        private List<List<int>> regionDistances;
        private List<Tuple<int, int>>[] path;
        private int engineerMoney;
        private int driverMoney;
        private int[] businessTripMoney;
        private int[] hotelMoney;

        public HardwareReplacementSolver(int carSpeed = 50, int replacementTime = 70)
        {
            this.carSpeed = carSpeed;
            this.replacementTime = replacementTime;
            storagePosition = new Tuple<int, int>(1, 0);
            homePosition = new Tuple<int, int>(1, 0);
            regionCenters = new List<RegionCenter>();
            regionDistances = new List<List<int>>();
            drivingSequence = new int[4];
        }

        public void Solve()
        {
            var brigadeNumber = 0;
            foreach (var centerNumber in drivingSequence)
            {
                var nextCenter = centerNumber;
                RefreshResources(brigadeNumber);
                ReplaceCar(position[brigadeNumber], new Tuple<int, int>(centerNumber, 0), brigadeNumber);
                ServeRegionCenter(nextCenter, brigadeNumber);
                brigadeNumber = GetNextBrigadeNumber(brigadeNumber);
            }

            Console.WriteLine("Конец");
            ShowResult();
        }

        private int GetNextBrigadeNumber(int number)
        {
            return number + 1 < brigadeCount ? number + 1 : 0;
        }

        private void ShowResult()
        {
            Console.WriteLine($"Количество бригад: {brigadeCount}");
            Console.WriteLine($"Суммарные расходы на автомобили: {pastMonthTime.Sum(time => time.Count * 5000)}");
            Console.WriteLine($"Суммарные командировочные {businessTripMoney.Sum()}");
            Console.WriteLine($"Суммарные затраты на отель: {hotelMoney.Sum()}");
            Console.WriteLine(
                $"Суммарная зарплата сотрудников: {engineerMoney * brigadeCount + driverMoney * brigadeCount}");
            for (var i = 0; i < brigadeCount; i++)
            {
                Console.WriteLine();
                ShowBrigadeResult(i);
            }
        }

        private void ShowBrigadeResult(int brigadeNumber)
        {
            Console.WriteLine($"Бригада{brigadeNumber + 1}");
            Console.WriteLine($"Маршрут: {GetPath(path[brigadeNumber])}");
            Console.WriteLine("Где {номер регионального центра}/{номер города (1=А...6=F)}");
            Console.WriteLine($"Расходы на автомобиль: {pastMonthTime[brigadeNumber].Count * 5000}");
            Console.WriteLine($"Командировочные: {businessTripMoney[brigadeNumber]}");
            Console.WriteLine($"Затраты на отель: {hotelMoney[brigadeNumber]}");
            Console.WriteLine($"Зарплата инженера: {engineerMoney}");
            Console.WriteLine($"Зарплата водителя: {driverMoney}");
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

        private void ServeRegionCenter(int number, int brigadeNumber)
        {
            var cities = regionCenters[number - 1].Cities;
            ServeCities(cities, brigadeNumber);
            ServeCenter(position[brigadeNumber].Item1, brigadeNumber);
        }

        private void ServeCenter(int centerNumber, int brigadeNumber)
        {
            var centerPosition = new Tuple<int, int>(centerNumber, 0);
            ReplaceCar(position[brigadeNumber], centerPosition, brigadeNumber);
            for (var i = 0; i < 3; i++)
            {
                RefreshResources(brigadeNumber);
                if (pastDayTime[brigadeNumber] + replacementTime > maxWorkTime)
                {
                    ReplaceCar(position[brigadeNumber], new Tuple<int, int>(centerNumber, 1), brigadeNumber);
                    DelayForSleep(brigadeNumber);
                }

                ReplaceCar(position[brigadeNumber], centerPosition, brigadeNumber);
                DelayForFixing(brigadeNumber);
            }
        }

        private void ServeCities(List<City> cities, int brigadeNumber)
        {
            while (!cities.TrueForAll(x => x.Visited))
            {
                foreach (var city in cities.Where(city => !city.Visited))
                {
                    RefreshResources(brigadeNumber);
                    var cityPosition = new Tuple<int, int>(position[brigadeNumber].Item1, city.Position);
                    var distanceToCity = regionCenters[position[brigadeNumber].Item1 - 1].Cities[city.Position - 1]
                        .Distance;
                    var timeToCity = GetReplacementTime(distanceToCity, carSpeed);
                    if (pastDayTime[brigadeNumber] + timeToCity + replacementTime > maxWorkTime)
                    {
                        DelayForSleep(brigadeNumber);
                    }

                    ReplaceCar(position[brigadeNumber], cityPosition, brigadeNumber);
                    DelayForFixing(brigadeNumber);
                    path[brigadeNumber].Add(position[brigadeNumber]);
                    city.Visited = true;
                    CheckWorkTime(brigadeNumber);
                }
            }
        }

        private void CheckWorkTime(int brigadeNumber)
        {
            var workTime = pastDayTime[brigadeNumber];
            var timeToHome = GetReplacingTime(position[brigadeNumber], homePosition, brigadeNumber);
            if (workTime + timeToHome > maxWorkTime)
            {
                DelayForSleep(brigadeNumber);
            }
        }

        private double GetReplacingTime(Tuple<int, int> from, Tuple<int, int> to, int brigadeNumber)
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
                var distance = regionCenters[position[brigadeNumber].Item1 - 1].Cities[homePosition.Item2 - 1].Distance;
                var timeToCity = GetReplacementTime(distance, carSpeed);
                time += timeToCity;
                currentPosition = homePosition;
            }

            return time;
        }

        private void DelayForFixing(int brigadeNumber)
        {
            Console.WriteLine(
                $"Починка бригада{brigadeNumber + 1} {position[brigadeNumber].Item1}/{position[brigadeNumber].Item2}");
            pastDayTime[brigadeNumber] += replacementTime;
            resources[brigadeNumber]--;
        }

        private void DelayForSleep(int brigadeNumber)
        {
            Console.WriteLine(
                $"Гостиница бригада{brigadeNumber + 1} {position[brigadeNumber].Item1}/{position[brigadeNumber].Item2}");
            pastMonthTime[brigadeNumber].Add(replacementTime);
            pastDayTime[brigadeNumber] = 0;
            businessTripMoney[brigadeNumber] += 1000 * 2;
            hotelMoney[brigadeNumber] += 800 * 2;
        }

        private void RefreshResources(int brigadeNumber)
        {
            if (resources[brigadeNumber] < 1)
            {
                ReceiveResources(brigadeNumber);
            }
        }

        private void ReceiveResources(int brigadeNumber)
        {
            if (position.Equals(storagePosition))
            {
                resources[brigadeNumber] = carCapacity;
            }
            else
            {
                var timeToStorage = GetReplacingTime(position[brigadeNumber], storagePosition, brigadeNumber);
                var oldPosition = position[brigadeNumber];
                if (pastDayTime[brigadeNumber] + 2 * timeToStorage > maxWorkTime)
                {
                    DelayForSleep(brigadeNumber);
                }

                ReplaceCar(position[brigadeNumber], storagePosition, brigadeNumber);
                resources[brigadeNumber] = carCapacity;
                ReplaceCar(position[brigadeNumber], oldPosition, brigadeNumber);
            }

            Console.WriteLine($"Ресурсы обновлены бригада{brigadeNumber + 1}: {resources[brigadeNumber]}");
        }

        private void ReplaceCar(Tuple<int, int> from, Tuple<int, int> to, int brigadeNumber)
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
                position[brigadeNumber] = new Tuple<int, int>(from.Item1, 0);
            }

            if (position[brigadeNumber].Item1 != to.Item1)
            {
                var distance = regionDistances[from.Item1 - 1][to.Item1 - 1];
                var timeToCenter = GetReplacementTime(distance, carSpeed);
                time += timeToCenter;
                position[brigadeNumber] = new Tuple<int, int>(to.Item1, 0);
            }

            if (position[brigadeNumber].Item2 != to.Item2)
            {
                var distance = regionCenters[position[brigadeNumber].Item1 - 1].Cities[to.Item2 - 1].Distance;
                var timeToCity = GetReplacementTime(distance, carSpeed);
                time += timeToCity;
                position[brigadeNumber] = to;
            }

            pastDayTime[brigadeNumber] += time;
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
                engineerMoney = 25000;
                driverMoney = 22500;
            }

            if (weeksCount == 2)
            {
                engineerMoney = 50000;
                driverMoney = 45000;
            }

            if (weeksCount == 3)
            {
                engineerMoney = 75000;
                driverMoney = 67500;
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

            InitializeBrigades(brigadeCount);

            return brigadeCount;
        }

        private void InitializeBrigades(int brigadeCount)
        {
            pastDayTime = new double[brigadeCount];
            pastMonthTime = new List<double>[brigadeCount];
            position = new Tuple<int, int>[brigadeCount];
            resources = new int[brigadeCount];
            path = new List<Tuple<int, int>>[brigadeCount];
            businessTripMoney = new int[brigadeCount];
            hotelMoney = new int[brigadeCount];
            for (var i = 0; i < brigadeCount; i++)
            {
                pastDayTime[i] = 0;
                pastMonthTime[i] = new List<double>();
                position[i] = homePosition;
                resources[i] = 0;
                path[i] = new List<Tuple<int, int>>();
                businessTripMoney[i] = 0;
                hotelMoney[i] = 0;
            }
        }
    }
}
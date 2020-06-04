using System;
using System.Collections.Generic;

namespace IDZ
{
    public class HardwareReplacementSolver
    {
        private readonly int minWorkTime = 480;
        private readonly int maxWorkTime = 720;
        private double pastDayTime;
        private List<double> pastMonthTime;
        private int expenses;
        private Tuple<int, int> position;
        private Tuple<int, int> storagePosition;
        private readonly int carSpeed;
        private readonly int replacementTime;
        private List<RegionCenter> regionCenters;
        private int weeksCount;
        private int brigadeCount;
        private int carCapacity;
        private int[] drivingSequence;
        private int resources;
        private List<List<int>> regionDistances;

        public HardwareReplacementSolver(int carSpeed = 50, int replacementTime = 70)
        {
            this.carSpeed = carSpeed;
            this.replacementTime = replacementTime;
            position = new Tuple<int, int>(4, 0);
            storagePosition = new Tuple<int, int>(1, 0);
            pastMonthTime = new List<double>();
            regionCenters = new List<RegionCenter>();
            regionDistances = new List<List<int>>();
            drivingSequence = new int[4];
        }

        public void Solve()
        {
            foreach (var centerNumber in drivingSequence)
            {
                var nextCenter = centerNumber;
                ServeRegionCenter(nextCenter);
            }
        }

        private void ServeRegionCenter(int number)
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
                ReplaceCar(position, storagePosition);
                resources = carCapacity;
            }

            Console.WriteLine($"Ресурсы обновлены: {resources}");
            Console.WriteLine($"Рабочее время: {pastDayTime}");
        }

        private void ReplaceCar(Tuple<int, int> from, Tuple<int, int> to)
        {
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
            weeksCount = GetWeeksCount();
            brigadeCount = GetBrigadeCount();
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

            return weeksCount * 2;
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
            Console.WriteLine($"Число рабочих бригад: {brigadeCount}");
            Console.WriteLine($"Вместимость автомобиля: {carCapacity}");
            Console.Write("Порядок движения по региональным центрам: ");
            foreach (var centerNumber in drivingSequence)
            {
                Console.Write(centerNumber);
            }

            Console.WriteLine();
        }
    }
}
using System;
using System.Collections.Generic;

namespace IDZ
{
    public class HardwareReplacementSolver
    {
        private List<RegionCenter> regionCenters = new List<RegionCenter>();
        private int weeksCount;
        private int brigadeCount;
        private int carCapacity;
        private int[] drivingSequence;

        public void SetRegionCenters(IEnumerable<IEnumerable<int>> distances)
        {
            foreach (var distance in distances)
            {
                regionCenters.Add(new RegionCenter(distance));
            }
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
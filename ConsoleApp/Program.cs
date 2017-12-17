using DynamicProgramming.KnapsackTask;
using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var dynamic = new KnapsackInteger("input.txt");

            if (dynamic.IsValidData)
            {
                dynamic.Calculating();

                Console.WriteLine("Рассчет окончен.");
            }
            else
            {
                Console.WriteLine("Ошибка! Некорректные данные в файле или файл не удалось открыть.");
            }

            Console.ReadLine();
        }
    }
}

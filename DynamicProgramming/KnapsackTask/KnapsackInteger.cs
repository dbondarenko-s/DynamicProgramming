using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DynamicProgramming.KnapsackTask
{
    public class KnapsackInteger
    {
        #region Private Fields

        /// <summary>
        /// Количество предметов.
        /// </summary>
        private int N { get; set; }

        /// <summary>
        /// Общий вес.
        /// </summary>
        private int SumWeight { get; set; }

        /// <summary>
        /// Значение веса предметов из исходного файла.
        /// </summary>
        private List<int> Weight { get; set; }

        /// <summary>
        /// Значение стоимости предметов из исходного файла.
        /// </summary>
        private List<int> Price { get; set; }

        /// <summary>
        /// Признак правильных считанных данных.
        /// </summary>
        public bool IsValidData { get; private set; }

        private List<DataTable> ResultTable { get; set; }

        #endregion

        #region Constructors

        public KnapsackInteger(string filePath)
        {
            if (!File.Exists(filePath))
            {
                IsValidData = false;

                return;
            }

            File.Delete("output.txt");

            using (var streamReader = new StreamReader(filePath, Encoding.ASCII))
            {
                if (!streamReader.EndOfStream)
                {
                    var lineString = streamReader.ReadLine();

                    if (!string.IsNullOrEmpty(lineString))
                    {
                        SumWeight = Convert.ToInt32(lineString);
                    }

                    lineString = streamReader.ReadLine();

                    if (!string.IsNullOrEmpty(lineString))
                    {
                        Weight = lineString.Split(' ').Select(int.Parse).ToList();

                        N = Weight.Count();
                    }

                    lineString = streamReader.ReadLine();

                    if (!string.IsNullOrEmpty(lineString))
                    {
                        Price = lineString.Split(' ').Select(int.Parse).ToList();
                    }
                }

                streamReader.Close();
            }

            IsValidData = Weight != null && Weight.Count > 0 && Price != null && Price.Count > 0 && N == Price.Count && N == Weight.Count;
        }

        #endregion

        #region Private Methods

        private List<int> BelmanFirst(int n, int price, List<int> result = null)
        {
            if (result == null)
                result = new List<int>();

            var array = new List<int>();

            for (int i = 0; i <= n; i++)
            {
                array.Add(i * price);
            }

            result.Add(array.Max());

            if (n > 0)
                BelmanFirst(n - 1, price, result);

            return result;
        }

        private void DisplayResultTableToStringList(int id, string addInformation = null)
        {
            var data = ResultTable.Where(x => x.Id == id).OrderBy(x => x.L).ToList();

            var stringOne = FormStringTable(data.Select(x => $" {x.L} ").ToList(), $" L");
            var stringTwo = FormStringTable(data.Select(x => $" {x.F} ").ToList(), $" f{id}(L)");
            var stringThree = FormStringTable(data.Select(x => $" {x.X} ").ToList(), $" x");

            using(var streamWriter = new StreamWriter("output.txt", File.Exists("output.txt")))
            {
                streamWriter.WriteLine($"Таблица №{id}.");
                streamWriter.WriteLine($"Доп. информация: {addInformation}.");
                streamWriter.WriteLine(stringOne);
                streamWriter.WriteLine(stringTwo);
                streamWriter.WriteLine(stringThree);
                streamWriter.WriteLine();

                streamWriter.Close();
            }
        }

        private void SaveAnswerToFile(List<int> x)
        {
            using (var streamWriter = new StreamWriter("output.txt", File.Exists("output.txt")))
            {
                streamWriter.WriteLine();
                streamWriter.WriteLine($"В итоге наилучший вариант достигается при следующих значениях (x{1} - x{x.Count}): {String.Join(", ", x.Select(op => $" {op}").ToArray())}");
                streamWriter.WriteLine();
                streamWriter.Close();
            }
        }

        private string FormStringTable(List<string> input, string nameRow)
        {
            var output = nameRow.PadRight(7, ' ') + '|';

            input.ForEach(str =>
            {
                output += str.PadRight(10, ' ') + "|";
            });

            return output;
        }

        private void FormResultTable(List<int> iteration, int indexIteration, int weight)
        {
            int iterator = 0, indexX = 0;

            for (int i = 0; i <= SumWeight; i++)
            {
                iterator++;

                ResultTable.Add(new DataTable()
                {
                    L = i,
                    F = iteration[i],
                    X = indexX,
                    Id = indexIteration
                });

                if (iterator == weight)
                {
                    iterator = 0;
                    indexX++;
                }
            }
        }

        #endregion

        public void Calculating()
        {
            ResultTable = new List<DataTable>();

            #region Условная оптимизация

            #region Начальная итерация

            var iteration = BelmanFirst((int)Math.Truncate((double)SumWeight / Weight[N - 1]), N);

            iteration.Reverse();

            var indexX = 0;
            var iterator = 0;
            for(int i = 0; i <= SumWeight; i++)
            {
                iterator++;
                
                ResultTable.Add(new DataTable()
                {
                    L = i,
                    F = iteration[indexX],
                    X = indexX,
                    Id = N
                });

                if (iterator == Weight[N - 1])
                {
                    iterator = 0;
                    indexX++;
                }
            }

            DisplayResultTableToStringList(N, $" f{N}(L) = max({N}*x{N}); 0 < x{N} < [{SumWeight}/{Weight[N - 1]}]; x8 = 0..{(int)Math.Truncate((double)SumWeight / Weight[N - 1])}");

            #endregion

            #region Последующие итерации

            var ident = N;

            for (int idx = N - 2; idx >= 0; idx--)
            {
                ident--;

                iteration = new List<int>();

                int x = -1;

                for (int i = 0; i < SumWeight; i += Weight[idx])
                {
                    x++;

                    for (int j = i; j < i + Weight[idx]; j++)
                    {
                        if (iteration.Count > SumWeight)
                            break;

                        if (x == 0)
                        {
                            var L = j - Weight[idx] * x;

                            var f = ResultTable.Where(op => op.Id == ident - 1 && op.L == L).Select(op => op.F).FirstOrDefault();

                            iteration.Add(Price[idx] * x + f);
                        }
                        else
                        {
                            var fList = new List<int>();

                            for (int k = 0; k <= x; k++)
                            {
                                var L = j - Weight[idx] * k;

                                var f = ResultTable.Where(op => op.Id == ident - 1 && op.L == L).Select(op => op.F).FirstOrDefault();

                                fList.Add(Price[idx] * k + f);
                            }

                            iteration.Add(fList.Max());
                        }
                    }
                }

                FormResultTable(iteration, ident, Weight[idx]);

                DisplayResultTableToStringList(ident, $" f{ident}(L) = max[{Price[idx]}*x{ident} + f{ident + 1}(L - {Weight[idx]} * x{ident})]; 0 < x{ident} < [{SumWeight}/{Weight[idx]}]; x{ident} = 0..{(int)Math.Truncate((double)SumWeight / Weight[idx])}");
            }

            #endregion

            #endregion

            #region Безусловная оптимизация

            var maxItems = ResultTable.Where(y => y.F == ResultTable.Max(x => x.F)).ToList();

            var resX = new List<int>() { maxItems.First().X };

            var resL = SumWeight - maxItems.Count * resX.First();

            for(int i = 2; i <= N; i++)
            {
                var resF = ResultTable.Where(x => x.Id == 2 && x.L == resL).First();

                resX.Add(resF.X);

                resL = resL - Weight[i-1] * resX.Last();
            }

            SaveAnswerToFile(resX);

            #endregion
        }
    }
}

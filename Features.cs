using System;
using System.Collections.Generic;
using System.Linq;

namespace ClickVisualizer
{
    internal class Features
    {
        public static double CalculateStandardDeviation(IEnumerable<double> data)
        {
            double mean = data.Average();

            double sumOfSquaresOfDifferences = data.Select(val => (val - mean) * (val - mean)).Sum();
            double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / data.Count());

            return standardDeviation;
        }

        public static double CalculateSkewness(IEnumerable<double> values)
        {
            double avg = CalculateMean(values);
            double stdDev = CalculateStandardDeviation(values);
            double sum = values.Sum(value => Math.Pow(value - avg, 3));

            int n = values.Count();
            double skewness = (n / (double)((n - 1) * (n - 2))) * (sum / Math.Pow(stdDev, 3));
            return skewness;
        }

        public static double CalculateEntropy(IEnumerable<double> data)
        {
            var histogram = new Dictionary<double, int>();
            foreach (var value in data)
            {
                var roundedValue = Math.Round(value, 2);

                if (histogram.ContainsKey(roundedValue))
                    histogram[roundedValue]++;
                else
                    histogram[roundedValue] = 1;
            }

            double total = data.Count();
            double entropy = -histogram.Values.Select(v => v / total * Math.Log(v / total)).Sum();
            return entropy;
        }

        public static double CalculateAutocorrelation(IEnumerable<double> data)
        {
            double mean = data.Average();
            double variance = data.Select(x => (x - mean) * (x - mean)).Sum() / data.Count();
            double covariance = data.Zip(data.Skip(1), (x1, x2) => (x1 - mean) * (x2 - mean)).Sum() / (data.Count() - 1);
            return covariance / variance;
        }

        public static double CalculateGiniCoefficient(IEnumerable<double> data)
        {
            double[] delays = data.ToArray();

            int n = delays.Length;

            Array.Sort(delays);

            double totalSum = 0;
            for (int i = 0; i < n; i++)
            {
                totalSum += delays[i];
            }

            double giniCoefficient = 0;
            double cumulativeWeightedSum = 0;
            for (int i = 0; i < n; i++)
            {
                cumulativeWeightedSum += (i + 1) * delays[i];
            }

            giniCoefficient = (2.0 * cumulativeWeightedSum) / (n * totalSum) - (n + 1.0) / n;
            return giniCoefficient;
        }

        public static double CalculateKurtosis(IEnumerable<double> data)
        {
            double mean = CalculateMean(data);
            double stdDev = CalculateStandardDeviation(data);
            int n = data.Count();

            double sumFourthMoment = data.Sum(value => Math.Pow((value - mean) / stdDev, 4));

            double numerator = n * (n + 1);
            double denominator = (n - 1) * (n - 2) * (n - 3);
            double firstFactor = numerator / denominator;
            double secondFactor = 3 * Math.Pow(n - 1, 2) / ((n - 2) * (n - 3));

            return firstFactor * (sumFourthMoment / n) - secondFactor;
        }

        public static double CalculateMean(IEnumerable<double> values)
        {
            return values.Average();
        }

        public static double CalculateMedian(IEnumerable<double> values)
        {
            var sortedValues = values.OrderBy(v => v).ToList();
            int count = sortedValues.Count;
            if (count % 2 == 0)
            {
                return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
            }
            else
            {
                return sortedValues[count / 2];
            }
        }

        public static double CalculateMode(IEnumerable<double> values)
        {
            return values.GroupBy(v => v)
                         .OrderByDescending(g => g.Count())
                         .ThenBy(g => g.Key)
                         .First()
                         .Key;
        }

        public static double CalculateIQR(IEnumerable<double> values)
        {
            var sortedValues = values.OrderBy(v => v).ToList();
            int count = sortedValues.Count;

            int percentile25th = (int)Math.Ceiling(0.25 * (count + 1)) - 1;
            int percentile75th = (int)Math.Ceiling(0.75 * (count + 1)) - 1;

            double Q1 = sortedValues[percentile25th];
            double Q3 = sortedValues[Math.Min(count - 1, percentile75th)];

            return Q3 - Q1;
        }
    }
}

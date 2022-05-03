using System;

namespace WpfCustomUtilities.Extensions
{
    public static class NumberExtension
    {
        public static int Sign(this double number)
        {
            return number >= 0 ? 1 : -1;
        }

        public static int Sign(this float number)
        {
            return number >= 0 ? 1 : -1;
        }

        /// <summary>
        /// Returns the absolute value of the number
        /// </summary>
        public static double Abs(this double number)
        {
            return Math.Abs(number);
        }

        /// <summary>
        /// Returns the absolute value of the number
        /// </summary>
        public static float Abs(this float number)
        {
            return Math.Abs(number);
        }

        public static int Clip(this int number, int lowLimit = 0, int highLimit = 1)
        {
            if (lowLimit > highLimit)
                throw new ArgumentException("Invalid limits int.Clip()");

            return Math.Min(Math.Max(lowLimit, number), highLimit);
        }

        public static double Clip(this double number, double lowLimit = 0, double highLimit = 1)
        {
            if (lowLimit > highLimit)
                throw new ArgumentException("Invalid limits double.Clip()");

            return Math.Min(Math.Max(lowLimit, number), highLimit);
        }

        public static double Clips(this double number, params double[] intervals)
        {
            if (intervals.Length % 2 != 0)
                throw new ArgumentException("Invalid clipping intervals");

            var lowLimit = double.MaxValue;
            var highLimit = double.MinValue;

            var follower = double.MinValue;

            for (int index = 0; index < intervals.Length - 1; index++)
            {
                if (number.Between(intervals[index], intervals[index + 1], true))
                    return number;

                if (!(follower <= intervals[index]) ||
                    !(follower <= intervals[index + 1]))
                    throw new Exception("Invalid clips intervals - must be ordered ascendingly");

                follower = intervals[index];

                if (intervals[index] < lowLimit)
                    lowLimit = intervals[index];

                if (intervals[index + 1] < lowLimit)
                    lowLimit = intervals[index + 1];

                if (intervals[index] > highLimit)
                    highLimit = intervals[index];

                if (intervals[index + 1] > highLimit)
                    highLimit = intervals[index + 1];
            }

            return number.Clip(lowLimit, highLimit);
        }

        public static float Clip(this float number, float lowLimit = 0, float highLimit = 1)
        {
            if (lowLimit > highLimit)
                throw new ArgumentException("Invalid limits double.Clip()");

            return Math.Min(Math.Max(lowLimit, number), highLimit);
        }

        public static byte Clip(this byte number, byte lowLimit = 0, byte highLimit = 1)
        {
            if (lowLimit > highLimit)
                throw new ArgumentException("Invalid limits int.Clip()");

            return Math.Min(Math.Max(lowLimit, number), highLimit);
        }
        public static int ToNearest(this int number, int division, bool roundUp)
        {
            var remainder = number % division;

            if (roundUp)
                return number + (division - remainder);

            else
                return number - remainder;
        }
        public static double ToNearest(this double number, double division, bool roundUp)
        {
            var remainder = number % division;

            if (roundUp)
                return number + (division - remainder);

            else
                return number - remainder;
        }

        public static double LowLimit(this double number, double lowLimit = 0)
        {
            return Math.Max(number, lowLimit);
        }

        public static float LowLimit(this float number, float lowLimit = 0)
        {
            return Math.Max(number, lowLimit);
        }

        public static int LowLimit(this int number, int lowLimit = 0)
        {
            return Math.Max(number, lowLimit);
        }

        public static double HighLimit(this double number, double highLimit = 1)
        {
            return Math.Min(number, highLimit);
        }

        public static float HighLimit(this float number, float highLimit = 1)
        {
            return Math.Min(number, highLimit);
        }

        public static int HighLimit(this int number, int highLimit = 1)
        {
            return Math.Min(number, highLimit);
        }

        /// <summary>
        /// Creates a linearly scaled number using a mapping from [0,1] -> [low, high]
        /// </summary>
        public static double Scale(this double number, double low, double high)
        {
            if (number < 0 || number > 1)
                throw new ArgumentException("Improper scale for the input to NumberExtension.Scale. Number must be between [0,1]");

            return (number * (high - low)) + low;
        }

        public static bool Between(this double number, double low, double high, bool orEqual = false)
        {
            if (orEqual)
                return number >= low && number <= high;

            else
                return number > low && number < high;
        }

        public static bool Between(this int number, int low, int high, bool orEqual = false)
        {
            if (orEqual)
                return number >= low && number <= high;

            else
                return number > low && number < high;
        }

        /// <summary>
        /// Rounds the number to the nearest fractional digit
        /// </summary>
        public static double Round(this double number, int digits)
        {
            return System.Math.Round(number, digits);
        }

        /// <summary>
        /// Rounds the number to the nearest fractional digit
        /// </summary>
        public static float Round(this float number, int digits)
        {
            return (float)System.Math.Round(number, digits);
        }

        public static double RoundOrderMagnitudeUp(this double number)
        {
            // Round up the log_10 of the number - which gives the inverse order-of-magnitude of
            // the number (or, the number of digits in the exponent as a ceiling)
            var numberDigits = Math.Ceiling(Math.Log10(number));

            return Math.Pow(10, numberDigits);
        }


        public static float RoundOrderMagnitudeUp(this float number)
        {
            // Round up the log_10 of the number - which gives the inverse order-of-magnitude of
            // the number (or, the number of digits in the exponent as a ceiling)
            var numberDigits = Math.Ceiling(Math.Log10(number));

            return (float)Math.Pow(10, numberDigits);
        }

        /// <summary>
        /// Returns the number's ordinal string
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToOrdinal(this int number)
        {
            // Start with the most common extension.
            string extension = "th";

            // Examine the last 2 digits.
            int last_digits = number % 100;

            // If the last digits are 11, 12, or 13, use th. Otherwise:
            if (last_digits < 11 || last_digits > 13)
            {
                // Check the last digit.
                switch (last_digits % 10)
                {
                    case 1:
                        extension = "st";
                        break;
                    case 2:
                        extension = "nd";
                        break;
                    case 3:
                        extension = "rd";
                        break;
                }
            }

            return number.ToString() + extension;
        }
    }
}

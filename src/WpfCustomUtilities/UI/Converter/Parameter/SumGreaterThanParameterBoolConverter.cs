﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace WpfCustomUtilities.UI.Converter
{
    public class SumGreaterThanParameterBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null ||
                values.Any(x => x == DependencyProperty.UnsetValue) ||
                parameter == null)
                return Binding.DoNothing;

            // Using System.Convert - TypeConverter (?)
            var sum = values.Sum(x => System.Convert.ToDouble(x));
            var limit = System.Convert.ToDouble(parameter);

            return sum > limit;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

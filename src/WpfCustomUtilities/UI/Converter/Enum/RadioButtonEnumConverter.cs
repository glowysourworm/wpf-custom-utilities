﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfCustomUtilities.UI.Converter
{
    public class RadioButtonEnumConverter : IValueConverter
    {
        // Parameter contains the targeted enumeration
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            return value.Equals(parameter);
        }

        // Parameter contains the targeted enumeration
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? parameter : Binding.DoNothing;
        }
    }
}

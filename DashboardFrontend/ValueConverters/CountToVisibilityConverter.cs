﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DashboardFrontend.ValueConverters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// A converter for the visibility of UI elements.
        /// </summary>
        /// <returns>True if value > 1, otherwise false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}

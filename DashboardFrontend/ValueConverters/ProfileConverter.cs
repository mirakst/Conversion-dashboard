﻿using DashboardBackend.Settings;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DashboardFrontend.ValueConverters
{
    public class ProfileConverter : IValueConverter
    {
        /// <returns>True, if value is a profile.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Profile;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

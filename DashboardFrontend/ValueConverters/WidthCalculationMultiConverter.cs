﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace DashboardFrontend.ValueConverters
{
    public class WidthCalculationMultiConverter : IMultiValueConverter
    {
        /// <summary>
        /// A converter for the width of a datagrid column, used for wrapping.
        /// </summary>
        /// <returns>A width based on the width of the container and all other elements in the datagrid.</returns>
        public object Convert(object[] values, Type targetType,
            object parameter, CultureInfo culture)
        {
            double otherColumnsTotalWidth = 22; //Scrollbar width (17) + padding (5)
            double.TryParse(values[0].ToString(), out var totalContainerWidth);

            if (values[1] is IList<GridViewColumn> arrayOfColumns)
                for (int i = 0; i < arrayOfColumns.Count - 1; i++)
                {
                    otherColumnsTotalWidth += arrayOfColumns[i].Width;
                }

            return (totalContainerWidth - otherColumnsTotalWidth) < 0 ?
                0 : (totalContainerWidth - otherColumnsTotalWidth);
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

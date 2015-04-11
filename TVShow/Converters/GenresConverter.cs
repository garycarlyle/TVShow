using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace TVShow.Converters
{
    /// <summary>
    /// Convert from rating string ("0" to "10") to an int (0 to 5)
    /// </summary>
    public class GenresConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Convert from window size to poster size
        /// </summary>
        /// <param name="culture">culture</param>
        /// <param name="parameter">parameter</param>
        /// <param name="targetType">targetType</param>
        /// <param name="value">value</param>
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                List<string> result = value as List<string>;
                string res = String.Empty;
                int index = 0;
                foreach (string item in result)
                {
                    index++;

                    res += item;
                    if (index != result.Count())
                    {
                        res += " / ";
                    }
                }
                return res;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// ConvertBack from rating int (0 to 5) to string ("0" to "10")
        /// </summary>
        /// <param name="culture">culture</param>
        /// <param name="parameter">parameter</param>
        /// <param name="targetType">targetType</param>
        /// <param name="value">value</param>
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
        #endregion
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TVShow.Converters
{
    /// <summary>
    /// Used to convert raw runtime movie to minutes
    /// </summary>
    public class RuntimeConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Convert value string to value string + " min"
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string result = String.Empty;
            if (value != null)
            {
                result = value + " min";
            }

            return result;
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
        #endregion
    }
}

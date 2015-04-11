using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TVShow.Converters
{
    /// <summary>
    /// Convert from rating string ("0" to "10") to an int (0 to 5)
    /// </summary>
    public class PosterSizeConverter : IValueConverter
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
            double result = 0.0;
            double param = 0;
            if (value != null && parameter != null)
            {
                result = (double) value;
                param = double.Parse(parameter.ToString(), CultureInfo.InvariantCulture);
                return result / param;
            }
            return result;
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

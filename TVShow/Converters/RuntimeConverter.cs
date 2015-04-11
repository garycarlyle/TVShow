using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Convert method
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
                string result = value.ToString();
                return result + " min";
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// ConvertBack method
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

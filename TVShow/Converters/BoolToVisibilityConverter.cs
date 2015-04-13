using System;
using System.Windows;
using System.Windows.Data;

namespace TVShow.Converters
{
    /// <summary>
    /// Use to convert boolean to a Visibility class property
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Convert method
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a VisibilityProperty");

            if (!(bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;

        }

        /// <summary>
        /// ConvertBack method
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
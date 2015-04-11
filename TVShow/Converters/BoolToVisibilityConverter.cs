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
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a VisibilityProperty");
            if (parameter == null)
            {
                if (!(bool)value)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
            else
            {
                if ((bool)value)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        /// <summary>
        /// ConvertBack method
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
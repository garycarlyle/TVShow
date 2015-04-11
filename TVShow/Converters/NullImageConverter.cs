using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TVShow.Converters
{
    /// <summary>
    /// Used to check if the path to the image file is empty or not
    /// </summary>
    public class NullImageConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Convert method
        /// </summary>
        /// <param name="culture">culture</param>
        /// <param name="parameter">parameter</param>
        /// <param name="targetType">targetType</param>
        /// <param name="value">value</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = value as string;
            if (String.IsNullOrEmpty(result))
                return DependencyProperty.UnsetValue;
            return value;
        }

        /// <summary>
        /// ConvertBack method
        /// </summary>
        /// <param name="culture">culture</param>
        /// <param name="parameter">parameter</param>
        /// <param name="targetType">targetType</param>
        /// <param name="value">value</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

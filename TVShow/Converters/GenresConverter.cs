using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace TVShow.Converters
{
    /// <summary>
    /// Format string genres to add "/" character between each genre
    /// </summary>
    public class GenresConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Used to add "/" character at the end of each genre
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                List<string> genres = value as List<string>;
                string res = String.Empty;
                if (genres != null)
                {
                    int index = 0;
                    foreach (string genre in genres)
                    {
                        index++;

                        res += genre;
                        // Add the slash at the end of each genre.
                        if (index != genres.Count())
                        {
                            res += " / ";
                        }
                    }
                }
                return res;
            }

            return String.Empty;
        }

        /// <summary>
        /// Not implemented
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

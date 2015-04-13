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
    /// Format string genres to add "/" character between each genre
    /// </summary>
    public class GenresConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Used to add "/" character at the end of each genre of the string
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
                List<string> result = value as List<string>;
                string res = String.Empty;
                int index = 0;
                foreach (string item in result)
                {
                    index++;

                    res += item;
                    // Add the slash at the end of each genre.
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
        /// ConvertBack method
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

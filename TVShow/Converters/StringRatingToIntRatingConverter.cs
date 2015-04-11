using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TVShow.Converters
{
    /// <summary>
    /// Convert from rating string ("0" to "10") to an int (0 to 5)
    /// </summary>
    public class StringRatingToIntRatingConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Convert from rating string ("0" to "10") to an int (0 to 5)
        /// </summary>
        /// <param name="culture">culture</param>
        /// <param name="parameter">parameter</param>
        /// <param name="targetType">targetType</param>
        /// <param name="value">value</param>
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            double result = 0.0;
            if (value != null)
            {
                string rating = value.ToString();
                try
                {
                    result = System.Convert.ToDouble(rating, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {

                }

                if (!double.Equals(result, 0.0))
                {
                    result = result / 2.0;
                    result = Math.Round(result);
                }
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
            string result = (((int)value) * 2).ToString(CultureInfo.InvariantCulture);
            
            return result;
        }
        #endregion
    }
}

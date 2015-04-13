using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace TVShow.Converters
{
    /// <summary>
    /// Convert double to double using a ratio parameter
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class RatioConverter : MarkupExtension, IValueConverter
    {
        private static RatioConverter _instance;

        public RatioConverter() { }

        #region IValueConverter Members
        /// <summary>
        /// Modify value with a ratio parameter
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            double size = System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return size.ToString("G0", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// NotImplemented
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new RatioConverter());
        }

    }
}

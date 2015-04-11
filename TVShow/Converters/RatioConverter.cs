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
    /// Used to resize the main window depending on a ratio parameter
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class RatioConverter : MarkupExtension, IValueConverter
    {
        private static RatioConverter _instance;

        public RatioConverter() { }

        #region IValueConverter Members
        /// <summary>
        /// Convert method
        /// </summary>
        /// <param name="culture">culture</param>
        /// <param name="parameter">parameter</param>
        /// <param name="targetType">targetType</param>
        /// <param name="value">value</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { // do not let the culture default to local to prevent variable outcome re decimal syntax
            double size = System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return size.ToString("G0", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// ConvertBack method
        /// </summary>
        /// <param name="culture">culture</param>
        /// <param name="parameter">parameter</param>
        /// <param name="targetType">targetType</param>
        /// <param name="value">value</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { // read only converter...
            throw new NotImplementedException();
        }
        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new RatioConverter());
        }

    }
}

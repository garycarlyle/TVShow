using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TVShow.Converters
{
    /// <summary>
    /// Used to convert font size to a width (used in the main page for sliding movies' title)
    /// </summary>
    public class FontToWidthConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Convert method
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Size sz = MeasureString((string)value);
                if (sz.Width > 160)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ConvertBack method
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
        #endregion

        /// <summary>
        /// MeasureString method
        /// </summary>
        /// <param name="candidate">candidate</param>
        private static Size MeasureString(string candidate)
        {
            FontFamily ff = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Agency FB");
            FontStyle fs = FontStyles.Normal;
            FontWeight fw = FontWeights.Bold;
            FontStretch fstrech = FontStretches.Normal;
            const double fontSize = 18;
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(ff, fs, fw, fstrech),
                fontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}

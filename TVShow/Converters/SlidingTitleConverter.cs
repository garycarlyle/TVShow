using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TVShow.Converters
{
    /// <summary>
    /// Check if a title has to slide regarding of its size
    /// </summary>
    public class SlidingTitleConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Check if a title has to slide regarding of its size 
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Size sz = MeasureString((string)value);
                if (sz.Width > Helpers.Constants.MaxWidthBeforeSlidingTitle)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
        #endregion

        /// <summary>
        /// MeasureString method
        /// </summary>
        /// <param name="title">Title to measure</param>
        private static Size MeasureString(string title)
        {
            FontFamily ff = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Agency FB");
            FontStyle fs = FontStyles.Normal;
            FontWeight fw = FontWeights.Bold;
            FontStretch fstrech = FontStretches.Normal;
            const double fontSize = Helpers.Constants.MovieTitleFontSize;
            var formattedText = new FormattedText(
                title,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(ff, fs, fw, fstrech),
                fontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using TVShow.Events;

namespace TVShow.CustomPanels
{
    /// <summary>
    /// Custom panel which resize elements and stretch them in itself when window is resizing
    /// </summary>
    public class ElasticWrapPanel : Panel
    {
        #region DependencyProperties
        #region DependencyProperty -> DesiredColumnWidthProperty
        /// <summary>
        /// Identifies the <see cref="DesiredColumnWidth"/> dependency property. 
        /// </summary>
        internal static readonly DependencyProperty DesiredColumnWidthProperty = DependencyProperty.Register("DesiredColumnWidth", typeof(double), typeof(ElasticWrapPanel), new PropertyMetadata(230d, new PropertyChangedCallback(OnDesiredColumnWidthChanged)));
        #endregion
        #endregion

        #region Properties
        #region Property -> Columns

        /// <summary>
        /// Columns
        /// </summary>
        private int _columns;
        public int Columns
        {
            get
            {
                return _columns;
            }
            set
            {
                if (_columns != value)
                {
                    _columns = value;
                    OnNumberOfColumnsChanged(new NumberOfColumnChangedEventArgs(value));
                }
            }
        }
        #endregion
        #region Property -> DesiredColumnWidth
        /// <summary>
        /// DesiredColumnWidth 
        /// </summary>
        public double DesiredColumnWidth
        {
            get
            {
                return (double)GetValue(DesiredColumnWidthProperty);
            }

            set
            {
                SetValue(DesiredColumnWidthProperty, value);
            }
        }
        #endregion
        #endregion

        #region Methods
        #region Method -> MeasureOverride
        /// <summary>
        /// Ovverides the MeasureOverride base method to calculate the required number of columns
        /// </summary>
        /// <param name="availableSize">availableSize</param>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize.Height.Equals(0))
            {
                availableSize.Height = MaxHeight;
            }

            Columns = (int)(availableSize.Width / DesiredColumnWidth);

            foreach (UIElement item in this.Children)
            {
                item.Measure(availableSize);
            }

            return base.MeasureOverride(availableSize);
        }
        #endregion

        #region Method -> ArrangeOverride
        /// <summary>
        /// Ovverides the ArrangeOverride base method to calculate the size of each element
        /// </summary>
        /// <param name="finalSize">finalSize</param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Columns != 0)
            {
                double columnWidth = Math.Floor(finalSize.Width / Columns);
                double totalHeight = 0;
                double top = 0;
                double rowHeight = 0;
                int column = 0;
                int index = 0;
                double overflow = 0;
                bool overflowAlreadyCount = false;
                foreach (UIElement item in this.Children)
                {
                    item.Arrange(new Rect(columnWidth * column, top, columnWidth, item.DesiredSize.Height));
                    column++;
                    if (Children.Count >= Columns)
                    {
                        rowHeight = Math.Max(rowHeight, item.DesiredSize.Height);
                    }
                    else
                    {
                        rowHeight = Math.Min(rowHeight, item.DesiredSize.Height);
                    }
                    index++;

                    // Check if the current element is at the end of a row and add an height overflow to get enough space for the next elements of the next row
                    if (column == Columns && Children.Count != index && (this.Children.Count - index + 1) <= Columns && !overflowAlreadyCount)
                    {
                        overflow = rowHeight;
                        totalHeight += rowHeight;
                        overflowAlreadyCount = true;
                    }
                    else
                    {
                        if (!overflowAlreadyCount)
                        {
                            totalHeight += rowHeight;
                        }
                    }
                    if (column == Columns)
                    {
                        column = 0;
                        top += rowHeight;
                        rowHeight = 0;
                    }
                }
                if (Children.Count >= Columns)
                {
                    totalHeight = totalHeight/Columns + overflow;
                }

                base.Height = totalHeight;
                finalSize.Height = totalHeight;

            }
            return base.ArrangeOverride(finalSize);
        }
        #endregion
        #region Method -> OnDesiredColumnWidthChanged
        /// <summary>
        /// Fired when DependencyProperty DesiredColumnWidthProperty is changed
        /// </summary>
        /// <param name="e">e</param>
        /// <param name="obj">obj</param>
        private static void OnDesiredColumnWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var panel = (ElasticWrapPanel)obj;
            panel.InvalidateMeasure();
            panel.InvalidateArrange();
        }
        #endregion

        #region Events
        #region Event -> NumberOfColumnsChanged
        /// <summary>
        /// NumberOfColumnsChanged event
        /// </summary>
        public event EventHandler<NumberOfColumnChangedEventArgs> NumberOfColumnsChanged;
        /// <summary>
        /// Advertise when the current number of columns changed in the ElasticWrapPanel
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnNumberOfColumnsChanged(NumberOfColumnChangedEventArgs e)
        {
            EventHandler<NumberOfColumnChangedEventArgs> handler = NumberOfColumnsChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion
        #endregion
        #endregion
    }
}

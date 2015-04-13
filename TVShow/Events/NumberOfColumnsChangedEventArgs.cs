using System;

namespace TVShow.Events
{
    /// <summary>
    /// NumberOfColumnChangedEventArgs
    /// </summary>
    public class NumberOfColumnChangedEventArgs : EventArgs
    {
        private readonly int _numberOfColumns;

        #region Constructor
        public NumberOfColumnChangedEventArgs(int numberOfColumns)
        {
            _numberOfColumns = numberOfColumns;
        }
        #endregion

        #region Properties
        #region Property -> NumberOfColumns
        public int NumberOfColumns
        {
            get
            {
                return _numberOfColumns;
            }
        }
        #endregion
        #endregion
    }
}

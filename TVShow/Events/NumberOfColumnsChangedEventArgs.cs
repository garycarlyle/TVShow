using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShow.Events
{
    public class NumberOfColumnChangedEventArgs : EventArgs
    {
        private readonly int _numberOfColumns;
        public NumberOfColumnChangedEventArgs(int numberOfColumns)
        {
            _numberOfColumns = numberOfColumns;
        }

        public int NumberOfColumns
        {
            get
            {
                return _numberOfColumns;
            }
        }
    }
}

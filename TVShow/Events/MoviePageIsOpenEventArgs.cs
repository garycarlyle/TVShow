using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShow.Events
{
    public class MoviePageIsOpenEventArgs : EventArgs
    {
        private readonly bool _isOpen;
        public MoviePageIsOpenEventArgs(bool isOpen)
        {
            _isOpen = isOpen;
        }

        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
        }
    }
}

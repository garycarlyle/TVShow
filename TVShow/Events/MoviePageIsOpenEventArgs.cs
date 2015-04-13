using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShow.Events
{
    /// <summary>
    /// MoviePageIsOpenEventArgs
    /// </summary>
    public class MoviePageIsOpenEventArgs : EventArgs
    {
        private readonly bool _isOpen;

        #region Constructor
        public MoviePageIsOpenEventArgs(bool isOpen)
        {
            _isOpen = isOpen;
        }
        #endregion

        #region Properties
        #region Property -> IsOpen
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
        }
        #endregion
        #endregion
    }
}

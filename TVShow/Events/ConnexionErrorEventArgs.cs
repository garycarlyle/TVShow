using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShow.Events
{
    /// <summary>
    /// ConnectionErrorEventArgs
    /// </summary>
    public class ConnectionErrorEventArgs : EventArgs
    {
        private readonly bool _isInError;

        #region Constructor
        public ConnectionErrorEventArgs(bool isInError)
        {
            _isInError = isInError;
        }
        #endregion

        #region Properties
        #region Property -> IsInError
        public bool IsInError
        {
            get
            {
                return _isInError;
            }
        }
        #endregion
        #endregion
    }
}

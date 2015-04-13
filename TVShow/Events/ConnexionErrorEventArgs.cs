using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShow.Events
{
    /// <summary>
    /// ConnexionErrorEventArgs
    /// </summary>
    public class ConnexionErrorEventArgs : EventArgs
    {
        private readonly bool _isInError;

        #region Constructor
        public ConnexionErrorEventArgs(bool isInError)
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

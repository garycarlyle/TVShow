using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShow.Events
{
    public class ConnexionErrorEventArgs : EventArgs
    {
        private readonly bool _isInError;
        public ConnexionErrorEventArgs(bool isInError)
        {
            _isInError = isInError;
        }

        public bool IsInError
        {
            get
            {
                return _isInError;
            }
        }
    }
}

using System;

namespace TVShow.Events
{
    /// <summary>
    /// ConnectionErrorEventArgs
    /// </summary>
    public class NumberOfLoadedMoviesEventArgs : EventArgs
    {
        private readonly int _numberOfMovies;
        private readonly bool _isExceptionThrown;

        #region Constructor
        public NumberOfLoadedMoviesEventArgs(int numberOfMovies, bool isExceptionThrown)
        {
            _numberOfMovies = numberOfMovies;
            _isExceptionThrown = isExceptionThrown;
        }
        #endregion

        #region Properties

        #region Property -> NumberOfMovies
        public int NumberOfMovies
        {
            get
            {
                return _numberOfMovies;
            }
        }
        #endregion

        #region Property -> IsExceptionThrown
        public bool IsExceptionThrown
        {
            get
            {
                return _isExceptionThrown;
            }
        }
        #endregion

        #endregion
    }
}

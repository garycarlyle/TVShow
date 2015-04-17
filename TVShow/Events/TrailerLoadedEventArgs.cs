using System;

namespace TVShow.Events
{
    /// <summary>
    /// MovieBufferedEventArgs
    /// </summary>
    public class TrailerLoadedEventArgs : EventArgs
    {
        private readonly string _trailerUrl;
        private readonly bool _inError;

        #region Constructor
        public TrailerLoadedEventArgs(string trailerUrl, bool inError)
        {
            _trailerUrl = trailerUrl;
            _inError = inError;
        }
        #endregion

        #region Properties

        #region Property -> TrailerUrl
        public string TrailerUrl
        {
            get
            {
                return _trailerUrl;
            }
        }
        #endregion

        #region Property -> InError
        public bool InError
        {
            get
            {
                return _inError;
            }
        }
        #endregion

        #endregion
    }
}

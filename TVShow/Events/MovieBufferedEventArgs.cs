using System;

namespace TVShow.Events
{
    /// <summary>
    /// MovieBufferedEventArgs
    /// </summary>
    public class MovieBufferedEventArgs : EventArgs
    {
        private readonly string _pathToFile;

        #region Constructor
        public MovieBufferedEventArgs(string pathToFile)
        {
            _pathToFile = pathToFile;
        }
        #endregion

        #region Properties

        #region Property -> PathToFile
        public string PathToFile
        {
            get
            {
                return _pathToFile;
            }
        }
        #endregion

        #endregion
    }
}

using System;

namespace TVShow.Events
{
    /// <summary>
    /// MovieLoadingProgressEventArgs
    /// </summary>
    public class MovieLoadingProgressEventArgs : EventArgs
    {
        private readonly double _progress;
        private readonly int _downloadRate;

        #region Constructor
        public MovieLoadingProgressEventArgs(double progress, int downloadRate)
        {
            _progress = progress;
            _downloadRate = downloadRate;
        }
        #endregion

        #region Properties

        #region Property -> Progress
        public double Progress
        {
            get
            {
                return _progress;
            }
        }
        #endregion

        #region Property -> DownloadRate
        public int DownloadRate
        {
            get
            {
                return _downloadRate;
            }
        }
        #endregion

        #endregion
    } 
}

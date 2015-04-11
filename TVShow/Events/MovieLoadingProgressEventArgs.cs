using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShow.Events
{
    public class MovieLoadingProgressEventArgs : EventArgs
    {
        private readonly double _progress;
        private readonly int _downloadRate;
        public MovieLoadingProgressEventArgs(double progress, int downloadRate)
        {
            _progress = progress;
            _downloadRate = downloadRate;
        }

        public double Progress
        {
            get
            {
                return _progress;
            }
        }

        public int DownloadRate
        {
            get
            {
                return _downloadRate;
            }
        }
    } 
}

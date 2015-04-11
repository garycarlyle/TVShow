using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShow.Events
{
    public class MovieBufferedEventArgs : EventArgs
    {
        private readonly string _pathToFile;
        public MovieBufferedEventArgs(string pathToFile)
        {
            _pathToFile = pathToFile;
        }

        public string PathToFile
        {
            get
            {
                return _pathToFile;
            }
        }
    }
}

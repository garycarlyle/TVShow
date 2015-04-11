using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TVShow.Helpers
{
    /// <summary>
    /// Inherits WebClient to override GetWebRequest
    /// </summary>
    public class NoKeepAliveWebClient : WebClient
    {
        #region Methods
        #region Method -> GetWebRequest
        /// <summary>
        /// Set KeepAlive to false (otherwise cause Server violation protocol Section=ResponseStatusLine with YTS Rest api)
        /// </summary>
        /// <param name="address">address</param>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);

            HttpWebRequest req = request as HttpWebRequest;
            if (req != null)
            {
                req.KeepAlive = false;
            }

            return request;
        }
        #endregion
        #endregion
    }
}

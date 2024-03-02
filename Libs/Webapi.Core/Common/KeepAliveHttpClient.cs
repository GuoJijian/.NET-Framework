using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core;

namespace Webapi.Core.Common
{

    public partial class KeepAliveHttpClient
    {

        #region Ctor

        public KeepAliveHttpClient(HttpClient httpClient) 
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Keep the current store site alive
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the asynchronous task whose result determines that request completed
        /// </returns>
        public virtual async Task KeepAliveAsync()
        {
            await HttpClient.GetStringAsync(NopCommonDefaults.KeepAlivePath);
        }

        #endregion
    }
}

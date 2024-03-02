using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Webapi.Core.Common
{
    public class InternalHttpClient
    {
        #region Ctor

        public InternalHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return HttpClient.SendAsync(request);
        }

        #endregion
    }
}

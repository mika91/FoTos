using System;
using System.Net.Http;

namespace GPhotosClientApi
{
    // see https://thomaslevesque.com/2018/02/25/better-timeout-handling-with-httpclient/
    public static class HttpRequestExtensions
    {
        private static readonly string TimeoutPropertyKey = "RequestTimeout";

        public static void SetTimeout(this HttpRequestMessage request,  TimeSpan? timeout)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.Properties[TimeoutPropertyKey] = timeout;
        }

        public static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Properties.TryGetValue(TimeoutPropertyKey, out var value) 
                && value is TimeSpan timeout)
                return timeout;

            return null;
        }
    }
}

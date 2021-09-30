using Ring.Web.Enums;
using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Ring.Web.Core.Extensions
{
    internal static class HttpListenerRequestExtension
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HttpVerb GetVerbType(this HttpListenerRequest request)
        {
            // don't use dictionary to increase performance, same for verb order 
            if (request == null) return HttpVerb.Other;
            var method = request.HttpMethod;
            if (string.Compare(Constants.HttpGet, method, StringComparison.OrdinalIgnoreCase) == 0) return HttpVerb.Get;
            if (string.Compare(Constants.HttpPut, method, StringComparison.OrdinalIgnoreCase) == 0) return HttpVerb.Put;
            if (string.Compare(Constants.HttpPost, method, StringComparison.OrdinalIgnoreCase) == 0) return HttpVerb.Post;
            if (string.Compare(Constants.HttpDelete, method, StringComparison.OrdinalIgnoreCase) == 0) return HttpVerb.Delete;
            if (string.Compare(Constants.HttpPatch, method, StringComparison.OrdinalIgnoreCase) == 0) return HttpVerb.Patch;
            if (string.Compare(Constants.HttpOptions, method, StringComparison.OrdinalIgnoreCase) == 0) return HttpVerb.Options;
            return HttpVerb.Other;
        }

    }
}

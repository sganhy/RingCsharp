using Ring.Web.Enums;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.Web.Core.Extensions
{
    internal static class HttpListenerContextExtension
    {

        /// <summary>
        /// Manage Rest APIs
        /// </summary>
        /// <param name="httpContext"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HandleRestApi(this HttpListenerContext httpContext)
        {
            if (httpContext == null) return;
            httpContext.Response.Headers.Set(HttpResponseHeader.Server, null); // hide server information
            var httpVerb = httpContext.Request.GetVerbType();

            switch (httpVerb)
            {
                case HttpVerb.Post:
                    Post(httpContext);
                    break;
                case HttpVerb.Get:
                    Get(httpContext);
                    break;
                case HttpVerb.Put:
                    Put(httpContext);
                    break;
                case HttpVerb.Patch:
                    Patch(httpContext);
                    break;
                case HttpVerb.Delete:
                    Delete(httpContext);
                    break;
                case HttpVerb.Options:
                    Options(httpContext);
                    break;
                case HttpVerb.Other:
                    Other(httpContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        /// <summary>
        /// Manage http response (GET)
        /// </summary>
        /// <param name="context"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Get(HttpListenerContext context)
        {
            var str = @"{""status"":200,""name"":""UTS web API(UWA)"",""thread_pool"":{""current_worker_count"":1,""min_worker_count"":10,""max_worker_count"":1024,""current_completion_port_count"":0,""min_completion_port_count"":10,""max_completion_port_count"":1024},""connection_pool"":{""min_number_connection"":1,""max_number_connection"":10,""connection_available"":3,""opened_connection"":2},""disk_infoéè"":{""capacity"":0,""free_space"":0,""used_space"":0},""version"":{""number"":""""}}";

            // manage mime type
            context.Response.ContentType = Constants.MediaTypeJson;
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentEncoding = Encoding.UTF8;

            // manage header
            var headers = context.Response.Headers;
            headers?.Set(HttpResponseHeader.CacheControl, Constants.NoCacheHeaderValue);
            //context.Response.KeepAlive = false;

            // response
            var buf = Encoding.UTF8.GetBytes(str);
            context.Response.ContentLength64 = buf.Length;
            context.Response.OutputStream.Write(buf, 0, buf.Length);
            //context.Response.OutputStream.Flush();
            //context.Response.OutputStream.Close();
            //context.Response.Close();
        }

        /// <summary>
        /// Http response (POST)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Post(HttpListenerContext context)
        {
        }

        /// <summary>
        /// Http response (PUT)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Put(HttpListenerContext context)
        {
        }

        /// <summary>
        /// Http response (PATCH)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Patch(HttpListenerContext context)
        {
        }

        /// <summary>
        /// Http response (DELETE)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Delete(HttpListenerContext context)
        {
        }

        /// <summary>
        /// Http response (OPTIONS)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Options(HttpListenerContext context)
        {
        }

        /// <summary>
        /// Http response (OTHER)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Other(HttpListenerContext context)
        {
            var response = context.Response;

            // manage mime type
            response.ContentType = Constants.MediaTypeHtml;
            response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            response.StatusDescription = Constants.StatusDescription405;
            response.ContentEncoding = Encoding.UTF8;

            // manage header
            var headers = context.Response.Headers;
            headers.Set(HttpResponseHeader.ContentEncoding, Constants.ContentTypeGzip);
            headers.Set(HttpResponseHeader.Date, null);
            context.Response.KeepAlive = true;

            // manage response
            response.ContentLength64 = Constants.HtmlMessageNotAllowed.Length;
            response.OutputStream.Write(Constants.HtmlMessageNotAllowed, 0, Constants.HtmlMessageNotAllowed.Length);
        }


    }
}

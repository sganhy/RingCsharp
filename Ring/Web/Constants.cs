﻿namespace Ring.Web
{
    internal static class Constants
    {

        // messages 
        internal static readonly string MsgServerRunning = @"Webserver running...";
        internal static readonly string MsgClassNotSupported = @"HttpListener not supported on this operating system";
        //****** internal static readonly string HttpListenerPrefix = @"http://127.0.0.1:{0}/";
        //****** is access denied error ==> netsh http add urlacl url="http://+:9300/" user=everyone
        internal static readonly string HttpListenerPrefix = @"http://+:{0}/";

        /*
        public enum HttpStatusCode
        {
            // Informational 1xx
            Continue = 100,
            SwitchingProtocols = 101,
            Processing = 102,
            EarlyHints = 103,
            // Successful 2xx
            OK = 200,
            Created = 201,
            Accepted = 202,
            NonAuthoritativeInformation = 203,
            NoContent = 204,
            ResetContent = 205,
            PartialContent = 206,
            MultiStatus = 207,
            AlreadyReported = 208,
            IMUsed = 226,

            // Redirection 3xx
            MultipleChoices = 300,
            Ambiguous = 300,
            MovedPermanently = 301,
            Moved = 301,
            Found = 302,
            Redirect = 302,
            SeeOther = 303,
            RedirectMethod = 303,
            NotModified = 304,
            UseProxy = 305,
            Unused = 306,
            TemporaryRedirect = 307,
            RedirectKeepVerb = 307,
            PermanentRedirect = 308,

            // Client Error 4xx
            BadRequest = 400,
            Unauthorized = 401,
            PaymentRequired = 402,
            Forbidden = 403,
            NotFound = 404,
            MethodNotAllowed = 405,
            NotAcceptable = 406,
            ProxyAuthenticationRequired = 407,
            RequestTimeout = 408,
            Conflict = 409,
            Gone = 410,
            LengthRequired = 411,
            PreconditionFailed = 412,
            RequestEntityTooLarge = 413,
            RequestUriTooLong = 414,
            UnsupportedMediaType = 415,
            RequestedRangeNotSatisfiable = 416,
            ExpectationFailed = 417,
+       // Removed status code: ImATeapot = 418,

           MisdirectedRequest = 421,
           UnprocessableEntity = 422,
           Locked = 423,
           FailedDependency = 424,
           UpgradeRequired = 426,
        
            PreconditionRequired = 428,
            TooManyRequests = 429,
        
            RequestHeaderFieldsTooLarge = 431,
        
            UnavailableForLegalReasons = 451,

            // Server Error 5xx
            InternalServerError = 500,
            NotImplemented = 501,
            BadGateway = 502,
            ServiceUnavailable = 503,
            GatewayTimeout = 504,
            HttpVersionNotSupported = 505,
            VariantAlsoNegotiates = 506,
            InsufficientStorage = 507,
            LoopDetected = 508,
        
            NotExtended = 510,
            NetworkAuthenticationRequired = 511,
        }
        */


    }
}

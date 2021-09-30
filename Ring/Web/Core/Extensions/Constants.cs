using Ring.Web.Enums;
using Ring.Web.Helpers;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Ring.Web.Core.Extensions
{
    internal static class Constants
    {
        // HttpListenerContextExtension
        // MediaType
        internal static readonly string MediaTypeJson = @"application/json; charset=utf-8";
        internal static readonly string MediaTypeHtml = @"text/html; charset=utf-8";
        internal static readonly string MediaTypePdf = @"application/pdf";

        // content type
        internal static readonly string ContentTypeGzip = @"gzip";

        // REST service http verbs - warning should be in uppercase 
        internal static readonly string HttpPost = @"POST";
        internal static readonly string HttpGet = @"GET";
        internal static readonly string HttpPut = @"PUT";
        internal static readonly string HttpPatch = @"PATCH";
        internal static readonly string HttpDelete = @"DELETE";
        internal static readonly string HttpOptions = @"OPTIONS";

        // message description 
        internal static readonly string StatusDescription405 = @"Not Allowed";

        // REST header content
        internal static readonly string NoCacheHeaderValue = @"no-cache";

        // Html error msg 
        //nternal static readonly string HttpMessage
        internal static readonly string HtmlMessage405 = @"405 " + StatusDescription405;
        internal static readonly byte[] HtmlMessageNotAllowed = GetHtmlWarning(HtmlMessage405, HtmlMessage405);
        internal static readonly char NewLine = (char)10;

        /* TEST --> remove restriction on connection header 
        var field = typeof(WebHeaderCollection).GetField("HInfo", BindingFlags.Static | BindingFlags.NonPublic);
        var field2 = field?.FieldType.GetField("HeaderHashTable", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
        var method = field2?.FieldType.GetMethod("Remove");
        var parametersArray = new object[] { "Connection" };
        method?.Invoke(field2.GetValue(field2.FieldType), parametersArray);
        */

        #region private methods

        private static byte[] GetHtmlWarning(string title, string description)
        {
            var sb = new StringBuilder();
            sb.AppendLine(HtmlHelper.GetOpenTag(HtmlTag.Html)); // begin #1 
            sb.Append(HtmlHelper.GetOpenTag(HtmlTag.Head)); // begin #2 
            ////////// Start Title
            sb.Append(HtmlHelper.GetOpenTag(HtmlTag.Title)); // begin #3 
            sb.Append(title);
            sb.Append(HtmlHelper.GetCloseTag(HtmlTag.Title)); // end #3
            ////////// End Title
            sb.AppendLine(HtmlHelper.GetCloseTag(HtmlTag.Head)); // end #2 
            sb.AppendLine(HtmlHelper.GetOpenTag(HtmlTag.Body, HtmlAttribute.Bgcolor, "white")); // begin #4
            ////////// Start Description
            sb.Append(HtmlHelper.GetOpenTag(HtmlTag.Center)); // begin #5
            sb.Append(HtmlHelper.GetOpenTag(HtmlTag.H1)); // begin #6
            sb.Append(description);
            sb.Append(HtmlHelper.GetCloseTag(HtmlTag.H1)); // end #6
            sb.AppendLine(HtmlHelper.GetCloseTag(HtmlTag.Center)); // end #5
            ////////// End Description
            sb.Append(HtmlHelper.GetOpenTag(HtmlTag.Hr)); // begin #7
            sb.Append(HtmlHelper.GetOpenTag(HtmlTag.Center)); // begin #8
            sb.Append("ring");
            sb.AppendLine(HtmlHelper.GetCloseTag(HtmlTag.Center)); // begin #8
            sb.AppendLine(HtmlHelper.GetCloseTag(HtmlTag.Body)); // end #4
            sb.Append(HtmlHelper.GetCloseTag(HtmlTag.Html)); // end #1 

            // zip buffer 
            var result = Encoding.UTF8.GetBytes(sb.ToString());
            var ms = new MemoryStream();
            var zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(result, 0, result.Length);
            zip.Dispose();
            result = ms.ToArray();
            // dispose 
            ms.Dispose();
            return result;
        }

        #endregion 

    }
}

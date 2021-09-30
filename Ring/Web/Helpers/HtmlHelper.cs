using Ring.Web.Enums;
using System;

namespace Ring.Web.Helpers
{
    internal static class HtmlHelper
    {

        internal static string GetOpenTag(HtmlTag tag) => string.Format(Constants.OpenTag, GetTag(tag));
        internal static string GetOpenTag(HtmlTag tag, HtmlAttribute attribute, string attributeValue) =>
            string.Format(Constants.OpenTagWithAttribute, GetTag(tag), GetAttribute(attribute, attributeValue));
        internal static string GetCloseTag(HtmlTag tag) => string.Format(Constants.CloseTag, GetTag(tag));

        #region private methods 

        private static string GetTag(HtmlTag tag)
        {
            switch (tag)
            {
                case HtmlTag.Html: return Constants.TagHtml;
                case HtmlTag.Head: return Constants.TagHead;
                case HtmlTag.Body: return Constants.TagBody;
                case HtmlTag.Center: return Constants.TagCenter;
                case HtmlTag.Title: return Constants.TagTitle;
                case HtmlTag.H1: return Constants.TagH1;
                case HtmlTag.Hr: return Constants.TagHr;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetAttribute(HtmlAttribute attribute, string value)
        {
            switch (attribute)
            {
                case HtmlAttribute.Bgcolor: return string.Format(Constants.AttributeElement, Constants.AttributeBgcolor, value);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        #endregion 

    }
}

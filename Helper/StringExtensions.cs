using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.Heidi.Helper
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a value indicating wether a specified string occurs within this string, using the specified comparison rules.
        /// </summary>
        /// <param name="source">Extension method for string</param>
        /// <param name="term">String to search for</param>
        /// <param name="comp">Sort rule to use, i.e. OrdinalIgnoreCase, etc</param>
        /// <returns></returns>
        public static bool Contains(this string source, string term, StringComparison comp)
        {
            // for use with NET 5.0
            if (source == null) return false;
            return source.IndexOf(term, comp) >= 0;

            // for use with NET 6.0 - switch when upgrade
            // return source?.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Decodes a URL characters with given encoding
        /// </summary>
        /// <param name="source">Extension method for string</param>
        /// <param name="encoding">Encoding to use i.e. ISO-8859-1</param>
        /// <returns></returns>
        public static string UrlDecode(this string source, string encoding)
        {
            Encoding code = Encoding.GetEncoding(encoding);
            return HttpUtility.UrlDecode(source, code);
        }

        /// <summary>
        /// Encode a string to URL characters with given encoding
        /// </summary>
        /// <param name="source">Extension method for string</param>
        /// <param name="encoding">Encoding to use i.e. ISO-8859-1</param>
        /// <returns></returns>
        public static string UrlEncode(this string source, string encoding)
        {
            Encoding code = Encoding.GetEncoding(encoding);
            return HttpUtility.UrlEncode(source, code);
        }
    }
}

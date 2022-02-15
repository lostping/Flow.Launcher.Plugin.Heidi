using System.Text.RegularExpressions;

namespace Flow.Launcher.Plugin.Heidi.Helper
{
    internal class HeidiRegex
    {
        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  Match expression but don't capture it. [.*]
        ///      Any character, any number of repetitions
        ///  Match expression but don't capture it. [\\]
        ///      Literal \
        ///  [Session]: A named capture group. [.*(?=\\Host)]
        ///      .*(?=\\Host)
        ///          Any character, any number of repetitions
        ///          Match a suffix but exclude it from the capture. [\\Host]
        ///              \\Host
        ///                  Literal \
        ///                  Host
        ///  
        ///
        /// </summary>
        public static Regex theSession = new Regex(
              "(?:.*)(?:Servers\\\\)(?<Session>.*(?=\\\\Host))",
            RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );
    }
}

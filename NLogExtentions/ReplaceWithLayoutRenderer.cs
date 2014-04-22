using System.Text;

namespace Newtopian.NLogExtentions
{
    using System.Text.RegularExpressions;
    using NLog;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using NLog.LayoutRenderers.Wrappers;
    using NLog.Layouts;

    /// <summary>
    /// Find a string optionnaly based on a regex and replaces it with a rendered layout.
    /// 
    /// Inner: layout that will be rendered to form the input string for this wrapper
    /// SearchFor : pattern to search for in the rendered Inner
    /// ReplaceWith : Pattern that will be rendered to form the replacement string.
    /// </summary>
    [LayoutRenderer("replaceWithLayout")]
    [ThreadAgnostic]
    public class ReplaceLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        private Regex _regex;

        /// <summary>
        /// Pattern that will identify the string to be replaced in the input. Depending on the value in IsRegex 
        /// this pattern will be simple text of a regex.
        /// </summary>
        /// <value>The text (or pattern) search for.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public string SearchFor { get; set; }

        /// <summary>
        /// Identifies SearchFor as a regex or just simple text.
        /// </summary>
        /// <value>A value of <c>true</c> if regular expressions should be used otherwise, <c>false</c>.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public bool IsRegex { get; set; }

        /// <summary>
        /// The Layout to replace the pattern in SearchFor with
        /// </summary>
        /// <value>The replacement layout.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public Layout ReplaceWith { get; set; }

        /// <summary>
        /// Option for SearchFor pattern find to be case sensitive or not
        /// </summary>
        /// <value>A value of <c>true</c> if case should be ignored when searching; otherwise, <c>false</c>.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Option for SearchFor to denote if we replace Whole words or if we can replace mid word
        /// </summary>
        /// <value>A value of <c>true</c> if whole words should be searched for; otherwise, <c>false</c>.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public bool WholeWords { get; set; }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            string regexString = this.SearchFor;

            //Escape the text so the regex engine takes it as-is
            if (!this.IsRegex)
            {
                regexString = System.Text.RegularExpressions.Regex.Escape(regexString);
            }

#if SILVERLIGHT
            RegexOptions regexOptions = RegexOptions.None;
#else
            RegexOptions regexOptions = RegexOptions.Compiled;
#endif
            if (this.IgnoreCase)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }

            if (this.WholeWords)
            {
                regexString = "\\b" + regexString + "\\b";
            }

            this._regex = new Regex(regexString, regexOptions);
        }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string msg = this.RenderInner(logEvent);
            builder.Append(this.Transform(msg, logEvent));
        }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <param name="logEvent">log event that will be used to fill the format</param>
        /// <returns>Post-processed text.</returns>
        protected string Transform(string text, LogEventInfo logEvent)
        {
            return this._regex.Replace(text, this.ReplaceWith.Render(logEvent));
        }

        protected override string Transform(string text)
        {
            return "THIS IS THE WRONG TRANSFORM !!!";
        }
    }
}

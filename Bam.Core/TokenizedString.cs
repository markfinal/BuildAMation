#region License
// Copyright (c) 2010-2016, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Strings can contain macros and functions, which are tokenized and evaluated in this class.
    /// Tokens are identified by $( and ).
    /// Functions can run before or after token expansion.
    /// Pre-functions run before, and are identified by #name(...).
    /// Post-functions run after token expansion, and are identified by @(...).
    /// </summary>
    public sealed class TokenizedString
    {
        [System.Flags]
        private enum EFlags
        {
            None = 0,
            Inline = 0x1
        }

        /// <summary>
        /// Prefix of each token.
        /// </summary>
        public static readonly string TokenPrefix = @"$(";

        /// <summary>
        /// Suffix of each token.
        /// </summary>
        public static readonly string TokenSuffix = @")";

        private static readonly string TokenRegExPattern = @"(\$\([^)]+\))";
        private static readonly string ExtractTokenRegExPattern = @"\$\(([^)]+)\)";
        private static readonly string PositionalTokenRegExPattern = @"\$\(([0-9]+)\)";

        // pre-functions look like: #functionname(expression)
        // note: this is using balancing groups in order to handle nested function calls, or any other instances of parentheses in paths (e.g. Windows 'Program Files (x86)')
        private static readonly string PreFunctionRegExPattern = @"(#(?<func>[a-z]+)\((?<expression>[^\(\)]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\))";

        // post-functions look like: @functionname(expression)
        // note: this is using balancing groups in order to handle nested function calls, or any other instances of parentheses in paths (e.g. Windows 'Program Files (x86)')
        private static readonly string PostFunctionRegExPattern = @"(@(?<func>[a-z]+)\((?<expression>[^\(\)]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\))";

        private static System.Collections.Generic.List<TokenizedString> Cache = new System.Collections.Generic.List<TokenizedString>();

        private System.Collections.Generic.List<string> Tokens = null;
        private Module ModuleWithMacros = null;
        private string OriginalString = null;
        private string ParsedString = null;
        private bool Verbatim;
        private TokenizedStringArray PositionalTokens = new TokenizedStringArray();
        private string CreationStackTrace = null;
        private int RefCount = 1;
        private EFlags Flags = EFlags.None;
        private TokenizedString Alias = null;

        /// <summary>
        /// Alias to another string. Useful for placeholder strings.
        /// </summary>
        /// <param name="alias">Alias.</param>
        public void
        Aliased(
            TokenizedString alias)
        {
            if (null != this.Alias)
            {
                throw new Exception("TokenizedString is already aliased");
            }
            this.Alias = alias;
            alias.RefCount += 1;
        }

        static private System.Collections.Generic.IEnumerable<string>
        SplitIntoTokens(
            string original,
            string regExPattern)
        {
            var regExSplit = System.Text.RegularExpressions.Regex.Split(original, regExPattern);
            var filtered = regExSplit.Where(item => !System.String.IsNullOrEmpty(item));
            return filtered;
        }

        static private System.Collections.Generic.IEnumerable<string>
        GetMatches(
            string original,
            string regExPattern)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(original, regExPattern);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                foreach (System.Text.RegularExpressions.Group group in match.Groups)
                {
                    if (group.Value == original)
                    {
                        continue;
                    }
                    yield return group.Value;
                }
            }
        }

        private TokenizedString(
            string original,
            Module moduleWithMacros,
            bool verbatim,
            TokenizedStringArray positionalTokens,
            EFlags flags)
        {
            this.CreationStackTrace = System.Environment.StackTrace;
            this.ModuleWithMacros = moduleWithMacros;
            if (null != positionalTokens)
            {
                this.PositionalTokens.AddRange(positionalTokens);
            }
            this.Verbatim = verbatim;
            this.Flags |= flags;
            this.OriginalString = original;
            if (verbatim)
            {
                this.ParsedString = NormalizeDirectorySeparators(original);
                return;
            }
        }

        private static TokenizedString
        CreateInternal(
            string tokenizedString,
            Module macroSource,
            bool verbatim,
            TokenizedStringArray positionalTokens,
            EFlags flags)
        {
            if (null == tokenizedString)
            {
                return null;
            }

            // strings can be created during the multithreaded phase
            lock (Cache)
            {
                var search = Cache.Where((ts) =>
                {
                    // first check the simple states for equivalence
                    if (ts.OriginalString == tokenizedString && ts.ModuleWithMacros == macroSource && ts.Verbatim == verbatim)
                    {
                        // and then check the positional tokens, if they exist
                        var samePosTokenCount = ((null != positionalTokens) && (positionalTokens.Count() == ts.PositionalTokens.Count())) ||
                                                ((null == positionalTokens) && (0 == ts.PositionalTokens.Count()));
                        if (!samePosTokenCount)
                        {
                            return false;
                        }
                        for (int i = 0; i < ts.PositionalTokens.Count(); ++i)
                        {
                            // because positional tokens are TokenizedStrings, they will refer to the same object
                            if (ts.PositionalTokens[i] != positionalTokens[i])
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
                var foundTS = search.FirstOrDefault();
                if (null != foundTS)
                {
                    ++foundTS.RefCount;
                    return foundTS;
                }
                var newTS = new TokenizedString(tokenizedString, macroSource, verbatim, positionalTokens, flags);
                Cache.Add(newTS);
                return newTS;
            }
        }

        /// <summary>
        /// Utility method to create a TokenizedString associated with a module, or return a cached version.
        /// </summary>
        /// <param name="tokenizedString">Tokenized string.</param>
        /// <param name="macroSource">Macro source.</param>
        /// <param name="positionalTokens">Positional tokens.</param>
        public static TokenizedString
        Create(
            string tokenizedString,
            Module macroSource,
            TokenizedStringArray positionalTokens = null)
        {
            return CreateInternal(tokenizedString, macroSource, false, positionalTokens, EFlags.None);
        }

        /// <summary>
        /// Utility method to create a TokenizedString with no macro replacement, or return a cached version.
        /// </summary>
        /// <returns>The verbatim.</returns>
        /// <param name="verboseString">Verbose string.</param>
        public static TokenizedString
        CreateVerbatim(
            string verboseString)
        {
            return CreateInternal(verboseString, null, true, null, EFlags.None);
        }

        /// <summary>
        /// Utility method to create a TokenizedString that can be inlined into other TokenizedStrings
        /// , or return a cached version.
        /// </summary>
        /// <returns>The inline.</returns>
        /// <param name="inlineString">Inline string.</param>
        public static TokenizedString
        CreateInline(
            string inlineString)
        {
            return CreateInternal(inlineString, null, false, null, EFlags.Inline);
        }

        private bool IsExpanded
        {
            get
            {
                if (null != this.Alias)
                {
                    return this.Alias.IsExpanded;
                }
                if (this.Verbatim)
                {
                    return true;
                }
                if (this.IsInline)
                {
                    return false;
                }
                foreach (var positionalToken in this.PositionalTokens)
                {
                    if (!positionalToken.IsExpanded)
                    {
                        return false;
                    }
                }
                return (null != this.ParsedString);
            }
        }

        private static string
        NormalizeDirectorySeparators(
            string path)
        {
            var normalized = OSUtilities.IsWindowsHosting ? path.Replace('/', '\\') : path.Replace('\\', '/');
            return normalized;
        }

        /// <summary>
        /// Display the parsed string, assuming it has been parsed.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Bam.Core.TokenizedString"/>.</returns>
        public override string
        ToString()
        {
            if (this.Alias != null)
            {
                return this.Alias.ToString();
            }
            if (!this.Verbatim && !this.IsExpanded)
            {
                throw new Exception("TokenizedString {0} was not expanded", this.OriginalString);
            }
            return this.ParsedString;
        }

        /// <summary>
        /// Determine if the string is empty.
        /// </summary>
        /// <value><c>true</c> if empty; otherwise, <c>false</c>.</value>
        public bool Empty
        {
            get
            {
                if (this.Alias != null)
                {
                    return this.Alias.Empty;
                }
                return (null == this.Tokens) || (0 == this.Tokens.Count());
            }
        }

        private bool IsInline
        {
            get
            {
                return (EFlags.Inline == (this.Flags & EFlags.Inline));
            }
        }

        /// <summary>
        /// Parse every TokenizedString.
        /// </summary>
        public static void
        ParseAll()
        {
            Log.Detail("Parsing strings");
            foreach (var t in Cache)
            {
                if (t.IsInline)
                {
                    Log.DebugMessage("Not parsing inline string: {0}", t.OriginalString);
                    continue;
                }
                t.Parse();
            }
        }

        /// <summary>
        /// Parse a TokenizedString with no macro overrides.
        /// </summary>
        public string
        Parse()
        {
            return this.Parse(null);
        }

        /// <summary>
        /// Parsed a TokenizedString with another source of macro overrides.
        /// Pre-functions are evaluated first.
        /// The order of source of tokens are checked in the follow order:
        /// - Positional tokens.
        /// - Any custom macros
        /// - Global macros (from the Graph)
        /// - Macros in the associated Module
        /// - Macros in the Tool associated with the Module
        /// - Environment variables
        /// After token expansion, post-functions are then evaluated.
        /// </summary>
        /// <param name="customMacros">Custom macros.</param>
        public string
        Parse(
            MacroList customMacros)
        {
            if (this.IsInline)
            {
                throw new Exception("Inline TokenizedString cannot be parsed, {0}", this.OriginalString);
            }
            if (this.Alias != null)
            {
                return this.Alias.Parse(customMacros);
            }
            if (this.IsExpanded && (null == customMacros))
            {
                return this.ParsedString;
            }

            this.Tokens = SplitIntoTokens(this.EvaluatePreFunctions(this.OriginalString, customMacros), TokenRegExPattern).ToList<string>();

            System.Func<TokenizedString, TokenizedString, int, string> expandTokenizedString = (source, tokenString, currentIndex) =>
                {
                    if (!tokenString.IsExpanded)
                    {
                        if (source == tokenString)
                        {
                            throw new Exception("Infinite recursion for {0}. Created at {3}", source.OriginalString, source.CreationStackTrace);
                        }

                        if (tokenString.IsInline)
                        {
                            // current token expands to nothing, and the inline string's tokens are processed next
                            source.Tokens.InsertRange(currentIndex + 1, SplitIntoTokens(tokenString.EvaluatePreFunctions(tokenString.OriginalString, source.ModuleWithMacros.Macros), TokenRegExPattern).ToList<string>());
                            return string.Empty;
                        }

                        // recursive
                        tokenString.Parse(null);
                    }
                    return tokenString.ToString();
                };

            var graph = Graph.Instance;
            var parsedString = new System.Text.StringBuilder();
            for (int index = 0; index < this.Tokens.Count; ++index)
            {
                var token = this.Tokens[index];
                if (!(token.StartsWith(TokenPrefix) && token.EndsWith(TokenSuffix)))
                {
                    parsedString.Append(token);
                    continue;
                }

                // step 1 : is the token positional, i.e. was set up at creation time
                var positional = GetMatches(token, PositionalTokenRegExPattern).FirstOrDefault();
                if (!System.String.IsNullOrEmpty(positional))
                {
                    var positionalIndex = System.Convert.ToInt32(positional);
                    if (positionalIndex > this.PositionalTokens.Count)
                    {
                        throw new Exception("TokenizedString positional token at index {0} requested, but only {1} positional values given. Created at {2}.", positionalIndex, this.PositionalTokens.Count, this.CreationStackTrace);
                    }
                    parsedString.Append(expandTokenizedString(this, this.PositionalTokens[positionalIndex], index));
                    continue;
                }
                // step 2 : try to resolve with custom macros passed to the Parse function
                else if (null != customMacros && customMacros.Dict.ContainsKey(token))
                {
                    parsedString.Append(expandTokenizedString(this, customMacros.Dict[token], index));
                    continue;
                }
                // step 3 : try macros in the global Graph, common to all modules
                else if (graph.Macros.Dict.ContainsKey(token))
                {
                    parsedString.Append(expandTokenizedString(this, graph.Macros.Dict[token], index));
                    continue;
                }
                else if (this.ModuleWithMacros != null)
                {
                    var tool = this.ModuleWithMacros.Tool;
                    // step 4 : try macros in the specific module
                    if (this.ModuleWithMacros.Macros.Dict.ContainsKey(token))
                    {
                        parsedString.Append(expandTokenizedString(this, this.ModuleWithMacros.Macros.Dict[token], index));
                        continue;
                    }
                    // step 5 : try macros in the Tool attached to the specific module
                    else if (null != tool && tool.Macros.Dict.ContainsKey(token))
                    {
                        parsedString.Append(expandTokenizedString(this, tool.Macros.Dict[token], index));
                        continue;
                    }
                }

                // step 6 : try the immediate environment
                var strippedToken = SplitIntoTokens(token, ExtractTokenRegExPattern).First();
                var envVar = System.Environment.GetEnvironmentVariable(strippedToken);
                if (null != envVar)
                {
                    parsedString.Append(envVar);
                    continue;
                }
                // step 7 : fail
                else
                {
                    // TODO: this could be due to the user not having set a property, e.g. inputpath
                    // is there a better error message that could be returned, other than this in those
                    // circumstances?
                    var message = new System.Text.StringBuilder();
                    message.AppendFormat("Unrecognized token '{0}' from original string '{1}'", token, this.OriginalString);
                    message.AppendLine();
                    if (null != customMacros)
                    {
                        message.AppendLine("Searched in custom macros");
                    }
                    message.AppendLine("Searched in global macros");
                    if (null != this.ModuleWithMacros)
                    {
                        message.AppendFormat("Searched in module {0}", this.ModuleWithMacros.ToString());
                        message.AppendLine();
                        if (null != this.ModuleWithMacros.Tool)
                        {
                            message.AppendFormat("Searched in tool {0}", this.ModuleWithMacros.Tool.ToString());
                            message.AppendLine();
                        }
                    }
                    message.AppendLine("TokenizedString created with this stack trace:");
                    message.AppendLine(this.CreationStackTrace);
                    throw new Exception(message.ToString());
                }
            }
            var functionEvaluated = this.EvaluatePostFunctions(NormalizeDirectorySeparators(parsedString.ToString()));
            if (null == customMacros)
            {
                this.ParsedString = functionEvaluated;
                Log.DebugMessage("Converted '{0}' to '{1}'", this.OriginalString, this.ToString());
            }
            return functionEvaluated;
        }

        private string
        EvaluatePostFunctions(
            string sourceExpression)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(sourceExpression, PostFunctionRegExPattern);
            if (0 == matches.Count)
            {
                if (sourceExpression.Contains("@"))
                {
                    throw new Exception("Expression '{0}' did not match for post-functions, but does contain @ - are there mismatching brackets?. Tokenized string '{1}' created at{2}{3}", sourceExpression, this.OriginalString, System.Environment.NewLine, this.CreationStackTrace);
                }
                return sourceExpression;
            }
            var modifiedString = sourceExpression;
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var functionName = match.Groups["func"].Value;
                // this correctly obtains the expression when nested functions are present
                var expressionText = new System.Text.StringBuilder();
                foreach (System.Text.RegularExpressions.Capture capture in match.Groups["expression"].Captures)
                {
                    expressionText.Append(capture.Value);
                }
                var expression = this.EvaluatePostFunctions(expressionText.ToString());
                var expandedExpression = this.FunctionExpression(functionName, expression);
                modifiedString = modifiedString.Replace(match.Value, expandedExpression);
            }
            return modifiedString;
        }

        private string
        FunctionExpression(
            string functionName,
            string argument)
        {
            switch (functionName)
            {
                case "basename":
                    return System.IO.Path.GetFileNameWithoutExtension(argument);

                case "filename":
                    return System.IO.Path.GetFileName(argument);

                case "dir":
                    return System.IO.Path.GetDirectoryName(argument);

                case "normalize":
                    return System.IO.Path.GetFullPath(argument);

                case "changeextension":
                    {
                        var split = argument.Split(',');
                        var original = split[0];
                        var extension = split[1].Trim();
                        var changed = System.IO.Path.ChangeExtension(original, extension);
                        return changed;
                    }

                case "removetrailingseperator":
                    return argument.TrimEnd(System.IO.Path.DirectorySeparatorChar);

                case "relativeto":
                    {
                        var split = argument.Split(',');
                        var path = split[0];
                        var root = split[1] + System.IO.Path.DirectorySeparatorChar;
                        var relative = RelativePathUtilities.GetPath(path, root);
                        if (relative.Equals(path))
                        {
                            throw new Exception("Unable to determine relative path of {0} from {1}", path, root);
                        }
                        return relative;
                    }

                case "trimstart":
                    {
                        var split = argument.Split(',');
                        var original = split[0];
                        var totrim = split[1];
                        while (original.StartsWith(totrim))
                        {
                            original = original.Replace(totrim, string.Empty);
                        }
                        return original;
                    }

                case "escapedquotes":
                    // ensure back slashes are escaped too
                    return System.String.Format("\\\"{0}\\\"", argument.Replace("\\", "\\\\"));

                default:
                    throw new Exception("Unknown post-function '{0}' in TokenizedString '{1}'", functionName, this.OriginalString);
            }
        }

        /// <summary>
        /// Does the string contain a space?
        /// </summary>
        /// <value><c>true</c> if contains space; otherwise, <c>false</c>.</value>
        public bool ContainsSpace
        {
            get
            {
                if (!this.IsExpanded)
                {
                    throw new Exception("TokenizedString, '{0}', is not yet expanded", this.OriginalString);
                }
                if (null != this.ParsedString)
                {
                    return this.ParsedString.Contains(' ');
                }
                else
                {
                    if (this.Tokens.Count != 1)
                    {
                        throw new Exception("Tokenized string that is expanded, but has more than one token");
                    }
                    return this.Tokens[0].Contains(' ');
                }
            }
        }

        /// <summary>
        /// Are two strings equivalent?
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Bam.Core.TokenizedString"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Bam.Core.TokenizedString"/>; otherwise, <c>false</c>.</returns>
        public override bool
        Equals(
            object obj)
        {
            var other = obj as TokenizedString;
            var equal = (this.Parse() == other.Parse());
            return equal;
        }

        /// <summary>
        /// Required by the Equals override.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int
        GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Parse, and if the parsed string contains a space, surround with quotes.
        /// </summary>
        /// <returns>The and quote if necessary.</returns>
        /// <param name="customMacros">Custom macros.</param>
        public string ParseAndQuoteIfNecessary(
            MacroList customMacros = null)
        {
            if (null != this.Alias)
            {
                return this.Alias.ParseAndQuoteIfNecessary(customMacros);
            }
            var parsed = this.Parse(customMacros);
            if (!this.ContainsSpace)
            {
                return parsed;
            }
            return System.String.Format("\"{0}\"", parsed);
        }

        /// <summary>
        /// Static utility method to return the number of TokenizedStrings cached.
        /// </summary>
        /// <value>The count.</value>
        public static int
        Count
        {
            get
            {
                return Cache.Count();
            }
        }

        /// <summary>
        /// Static utility method to return the number of strings with a single refcount.
        /// </summary>
        /// <value>The unshared count.</value>
        public static int
        UnsharedCount
        {
            get
            {
                return Cache.Where(item => item.RefCount == 1).Count();
            }
        }

        /// <summary>
        /// In debug builds, dump data representing all of the tokenized strings.
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void
        DumpCache()
        {
            Log.DebugMessage("Tokenized string cache");
            foreach (var item in Cache.OrderBy(item => item.RefCount).ThenBy(item => !item.Verbatim).ThenBy(item => item.Alias != null))
            {
                Log.DebugMessage("#{0} {1}'{2}'{3} {4} {5}",
                    item.RefCount,
                    item.Verbatim ? "<verbatim>" : string.Empty,
                    item.OriginalString,
                    item.Verbatim ? "</verbatim>" : string.Empty,
                    item.Alias != null ? System.String.Format("Aliased to: '{0}'", item.Alias.OriginalString) : string.Empty,
                    item.ModuleWithMacros != null ? System.String.Format("(ref: {0})", item.ModuleWithMacros.GetType().ToString()) : string.Empty);
            }
        }

        private bool
        IsTokenValid(
            string token,
            MacroList customMacros)
        {
            // step 1 : is the token positional, i.e. was set up at creation time
            var positional = GetMatches(token, PositionalTokenRegExPattern).FirstOrDefault();
            if (!System.String.IsNullOrEmpty(positional))
            {
                var positionalIndex = System.Convert.ToInt32(positional);
                return (positionalIndex <= this.PositionalTokens.Count);
            }
            // step 2 : try to resolve with custom macros passed to the Parse function
            else if (null != customMacros && customMacros.Dict.ContainsKey(token))
            {
                return true;
            }
            // step 3 : try macros in the global Graph, common to all modules
            else if (Graph.Instance.Macros.Dict.ContainsKey(token))
            {
                return true;
            }
            else if (this.ModuleWithMacros != null)
            {
                var tool = this.ModuleWithMacros.Tool;
                // step 4 : try macros in the specific module
                if (this.ModuleWithMacros.Macros.Dict.ContainsKey(token))
                {
                    return true;
                }
                // step 5 : try macros in the Tool attached to the specific module
                else if (null != tool && tool.Macros.Dict.ContainsKey(token))
                {
                    return true;
                }
            }

            // step 6 : try the immediate environment
            var strippedToken = SplitIntoTokens(token, ExtractTokenRegExPattern).First();
            var envVar = System.Environment.GetEnvironmentVariable(strippedToken);
            if (null != envVar)
            {
                return true;
            }
            // step 7 : fail
            else
            {
                return false;
            }
        }

        private string
        EvaluatePreFunctions(
            string originalExpression,
            MacroList customMacros)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(originalExpression, PreFunctionRegExPattern);
            if (0 == matches.Count)
            {
                if (originalExpression.Contains("#"))
                {
                    throw new Exception("Expression '{0}' did not match for pre-functions, but does contain # - are there mismatching brackets?. Tokenized string '{1}' created at{2}{3}", originalExpression, this.OriginalString, System.Environment.NewLine, this.CreationStackTrace);
                }
                return originalExpression;
            }
            var modifiedString = originalExpression;
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var functionName = match.Groups["func"].Value;
                // this correctly obtains the expression when nested functions are present
                var expressionText = new System.Text.StringBuilder();
                foreach (System.Text.RegularExpressions.Capture capture in match.Groups["expression"].Captures)
                {
                    expressionText.Append(capture.Value);
                }
                var expression = this.EvaluatePreFunctions(expressionText.ToString(), customMacros);
                switch (functionName)
                {
                    case "valid":
                        {
                            var tokens = SplitIntoTokens(expression, TokenRegExPattern);
                            var allTokensValid = true;
                            foreach (var token in tokens)
                            {
                                if (!(token.StartsWith(TokenPrefix) && token.EndsWith(TokenSuffix)))
                                {
                                    continue;
                                }

                                // Note: with nested valid pre-functions, macros can be validated as many times as they are nested
                                if (this.IsTokenValid(token, customMacros))
                                {
                                    continue;
                                }

                                allTokensValid = false;
                                break;
                            }

                            if (allTokensValid)
                            {
                                modifiedString = modifiedString.Replace(match.Value, expression);
                            }
                            else
                            {
                                modifiedString = modifiedString.Replace(match.Value, string.Empty);
                            }
                        }
                        break;

                    default:
                        throw new Exception("Unknown pre-function '{0}' in TokenizedString '{1}'", functionName, this.OriginalString);
                }
            }
            return modifiedString;
        }
    }
}

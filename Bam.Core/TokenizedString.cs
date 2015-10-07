#region License
// Copyright (c) 2010-2015, Mark Final
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
    /// Extension functions
    /// </summary>
    static class Extensions
    {
        // ref: http://stackoverflow.com/questions/521687/c-sharp-foreach-with-index
        static public void
        Each<T>(
            this System.Collections.Generic.IEnumerable<T> ie,
            System.Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }
    }

    /// <summary>
    /// Strings can contain macros and functions, which are tokenized and evaluated in this class
    /// </summary>
    public sealed class TokenizedString
    {
        public static readonly string TokenPrefix = @"$(";
        public static readonly string TokenSuffix = @")";
        private static readonly string TokenRegExPattern = @"(\$\([^)]+\))";
        private static readonly string ExtractTokenRegExPattern = @"\$\(([^)]+)\)";
        private static readonly string PositionalTokenRegExPattern = @"\$\(([0-9]+)\)";
        private static readonly string FunctionRegExPattern = @"(@(?<func>[a-z]+)\((?<expression>[^()]*)\))";
        private static readonly string FunctionPrefix = @"@";
        private static readonly string FunctionSuffix = @")";

        private static System.Collections.Generic.List<TokenizedString> Cache = new System.Collections.Generic.List<TokenizedString>();

        private System.Collections.Generic.List<string> Tokens = null;
        private System.Collections.Generic.List<int> MacroIndices = null;
        private Module ModuleWithMacros = null;
        private string OriginalString = null;
        private string ParsedString = null;
        private bool Verbatim;
        private TokenizedStringArray PositionalTokens = new TokenizedStringArray();
        private string CreationStackTrace = null;

        public void
        Assign(
            TokenizedString other)
        {
            if (null == this.ModuleWithMacros)
            {
                throw new Exception("Can't switch out a TokenizedString without a module");
            }
            this.Tokens = other.Tokens;
            this.MacroIndices = other.MacroIndices;
            this.ModuleWithMacros = other.ModuleWithMacros;
            this.OriginalString = other.OriginalString;
            this.ParsedString = other.Verbatim ? other.ParsedString : null; // force reparsing
            this.Verbatim = other.Verbatim;
            this.PositionalTokens = other.PositionalTokens;
        }

        static private System.Collections.Generic.IEnumerable<string>
        SplitToParse(
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
            bool verbatim = false,
            TokenizedStringArray positionalTokens = null)
        {
            this.CreationStackTrace = System.Environment.StackTrace;
            if (null != positionalTokens)
            {
                this.PositionalTokens.AddRange(positionalTokens);
            }
            this.Verbatim = verbatim;
            this.OriginalString = original;
            if (verbatim)
            {
                this.ParsedString = original;
                return;
            }
            var tokenized = SplitToParse(original, TokenRegExPattern);
            tokenized.Each<string>((item, index) =>
            {
                if (item.StartsWith(TokenPrefix) && item.EndsWith(TokenSuffix))
                {
                    if (null == this.MacroIndices)
                    {
                        this.MacroIndices = new System.Collections.Generic.List<int>();
                    }
                    this.MacroIndices.Add(index);
                }
            });
            this.Tokens = tokenized.ToList<string>();
        }

        private TokenizedString(
            string original,
            Module moduleWithMacros,
            bool verbatim,
            TokenizedStringArray positionalTokens) :
            this(original, verbatim, positionalTokens)
        {
            if (null == moduleWithMacros)
            {
                if (null != this.MacroIndices)
                {
                    foreach (var tokenIndex in this.MacroIndices)
                    {
                        if (!Graph.Instance.Macros.Contains(this.Tokens[tokenIndex]))
                        {
                            throw new Exception("Cannot have a tokenized string without a module");
                        }
                    }
                }
                else
                {
                    // consider the string parsed, as there is no work to do
                    this.ParsedString = this.OriginalString;
                }
            }
            this.ModuleWithMacros = moduleWithMacros;
        }

        public static TokenizedString
        Create(
            string tokenizedString,
            Module macroSource,
            bool verbatim = false,
            TokenizedStringArray positionalTokens = null)
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
                        return ts.OriginalString == tokenizedString && ts.ModuleWithMacros == macroSource;
                    });
                if (search.Count() > 0)
                {
                    return search.ElementAt(0);
                }
                else
                {
                    var ts = new TokenizedString(tokenizedString, macroSource, verbatim, positionalTokens);
                    Cache.Add(ts);
                    return ts;
                }
            }
        }

        public static TokenizedString
        CreateVerbatim(
            string verboseString)
        {
            return Create(verboseString, null, verbatim: true);
        }

        private string this[int index]
        {
            get
            {
                return this.Tokens[index];
            }

            set
            {
                this.Tokens[index] = value;
                if (this.MacroIndices.Contains(index))
                {
                    this.MacroIndices.Remove(index);
                }
            }
        }

        private bool IsExpanded
        {
            get
            {
                if (this.Verbatim)
                {
                    return true;
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
        JoinTokens(
            System.Collections.Generic.List<string> tokens)
        {
            if (1 == tokens.Count)
            {
                return tokens[0];
            }
            var join = System.String.Join(string.Empty, tokens);
            if (OSUtilities.IsWindowsHosting)
            {
                join = join.Replace('/', '\\');
            }
            else
            {
                join = join.Replace('\\', '/');
            }
            return join;
        }

        public override string
        ToString()
        {
            if (this.Verbatim)
            {
                return this.OriginalString;
            }
            else
            {
                if (!this.IsExpanded)
                {
                    throw new Exception("TokenizedString {0} was not expanded", this.OriginalString);
                }
                return this.ParsedString;
            }
        }

        public bool Empty
        {
            get
            {
                return (null == this.Tokens) || (0 == this.Tokens.Count());
            }
        }

        public static void
        ParseAll()
        {
            Log.Detail("Parsing tokenized strings");
            foreach (var t in Cache)
            {
                t.ParsedString = t.Parse();
            }
        }

        public string
        Parse()
        {
            return this.Parse(null);
        }

        public string
        Parse(
            MacroList customMacros)
        {
            if (this.IsExpanded && (null == customMacros))
            {
                return this.ParsedString;
            }
            if (null == this.MacroIndices)
            {
                throw new Exception("Tokenized string '{0}', does not appear to contain any {1}...{2} tokens. Created at {3}", this.OriginalString, TokenPrefix, TokenSuffix, this.CreationStackTrace);
            }

            System.Func<TokenizedString, TokenizedString, string> expandTokenizedString = (source, tokenString) =>
                {
                    if (!tokenString.IsExpanded)
                    {
                        // recursive
                        if (source == tokenString)
                        {
                            throw new Exception("Infinite recursion for {0}. Created at {3}", source.OriginalString, source.CreationStackTrace);
                        }
                        tokenString.Parse();
                    }
                    return tokenString.ToString();
                };

            // take a copy of the macro indices
            var macroIndices = new System.Collections.Generic.List<int>(this.MacroIndices);
            var tokens = new System.Collections.Generic.List<string>(this.Tokens); // could just be a reserved list of strings
            foreach (int index in this.MacroIndices.Reverse<int>())
            {
                var token = this.Tokens[index];

                // step 0 : is the token positional, i.e. was set up at creation time
                var positional = GetMatches(token, PositionalTokenRegExPattern).FirstOrDefault();
                if (!System.String.IsNullOrEmpty(positional))
                {
                    var positionalIndex = System.Convert.ToInt32(positional);
                    if (positionalIndex > this.PositionalTokens.Count)
                    {
                        throw new Exception("TokenizedString positional token at index {0} requested, but only {1} positional values given. Created at {2}.", positionalIndex, this.PositionalTokens.Count, this.CreationStackTrace);
                    }
                    token = expandTokenizedString(this, this.PositionalTokens[positionalIndex]);
                }
                // step 1 : try to resolve with macros passed to the Parse function
                else if (null != customMacros && customMacros.Dict.ContainsKey(token))
                {
                    token = expandTokenizedString(this, customMacros.Dict[token]);
                }
                // step 2 : try macros in the global Graph, common to all modules
                else if (Graph.Instance.Macros.Dict.ContainsKey(token))
                {
                    token = expandTokenizedString(this, Graph.Instance.Macros.Dict[token]);
                }
                // step 3 : try macros in the specific module
                else if (this.ModuleWithMacros != null && this.ModuleWithMacros.Macros.Dict.ContainsKey(token))
                {
                    token = expandTokenizedString(this, this.ModuleWithMacros.Macros.Dict[token]);
                }
                // step 4 : try macros in the Tool attached to the specific module
                else if (this.ModuleWithMacros != null && null != this.ModuleWithMacros.Tool && this.ModuleWithMacros.Tool.Macros.Dict.ContainsKey(token))
                {
                    token = expandTokenizedString(this, this.ModuleWithMacros.Tool.Macros.Dict[token]);
                }
                else
                {
                    // step 5 : try the immediate environment
                    var strippedToken = SplitToParse(token, ExtractTokenRegExPattern).First();
                    var envVar = System.Environment.GetEnvironmentVariable(strippedToken);
                    if (null != envVar)
                    {
                        token = envVar;
                    }
                    // step 6 : fail
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
                        message.AppendLine("Created at");
                        message.AppendLine(this.CreationStackTrace);
                        throw new System.Exception(message.ToString());
                    }
                }
                if (null == token)
                {
                    throw new Exception("Token replacement for {0} was null - something went wrong during parsing of the string '{1}'. Created at {2}", tokens[index], this.OriginalString, this.CreationStackTrace);
                }
                tokens[index] = token;
                macroIndices.Remove(index);
            }
            if (macroIndices.Count > 0)
            {
                var message = new System.Text.StringBuilder();
                message.AppendFormat("Input string '{0}' could not be fully expanded. Could not identify tokens", this.OriginalString);
                message.AppendLine();
                foreach (var index in macroIndices)
                {
                    message.AppendFormat("\t{0}", this.Tokens[index]);
                    message.AppendLine();
                }
                message.AppendLine("Created at");
                message.AppendLine(this.CreationStackTrace);
                throw new System.Exception(message.ToString());
            }
            var joined = this.EvaluateFunctions(tokens);
            if (null == customMacros)
            {
                this.ParsedString = joined;
            }
            Log.DebugMessage("Converted '{0}' to '{1}'", this.OriginalString, this.ToString());
            return joined;
        }

        private string
        EvaluateFunctions(
            System.Collections.Generic.List<string> tokens)
        {
            var joined = JoinTokens(tokens);
            // function calls may be nested, so the reg ex gets the inner most calls
            // and so iterate until all functions have been found
            for (;;)
            {
                var tokenized = SplitToParse(joined, FunctionRegExPattern);
                var matchCount = tokenized.Count();
                if (1 == matchCount)
                {
                    break;
                }
                // triplets of matches (entire expression, function name, argument)
                int matchIndex = 0;
                while (matchIndex < matchCount)
                {
                    var matchedExpression = tokenized.ElementAt(matchIndex++);
                    // does the first match constitute a function call?
                    if (!matchedExpression.StartsWith(FunctionPrefix))
                    {
                        continue;
                    }
                    if (!matchedExpression.EndsWith(FunctionSuffix))
                    {
                        // nested function call - this is the outer call, ignore
                        continue;
                    }

                    // if it was a function call, the next match is the function name
                    var functionName = tokenized.ElementAt(matchIndex++);
                    // and after that is the argument expression
                    var expression = tokenized.ElementAt(matchIndex++);
                    var expandedExpression = this.FunctionExpression(functionName, expression);
                    joined = joined.Replace(matchedExpression, expandedExpression);
                }
            }
            return joined;
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

                default:
                    throw new Exception("Unknown TokenizedString function, {0}", functionName);
            }
        }

        public bool ContainsSpace
        {
            get
            {
                if (!this.IsExpanded)
                {
                    throw new Exception("String is not yet expanded");
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

        public override bool
        Equals(
            object obj)
        {
            var other = obj as TokenizedString;
            var equal = (this.Parse() == other.Parse());
            return equal;
        }

        public override int
        GetHashCode()
        {
            return base.GetHashCode();
        }

        public string ParseAndQuoteIfNecessary(
            MacroList customMacros = null)
        {
            var parsed = this.Parse(customMacros);
            if (!this.ContainsSpace)
            {
                return parsed;
            }
            return System.String.Format("\"{0}\"", parsed);
        }
    }

    public sealed class TokenizedStringArray :
        Array<TokenizedString>
    {
        public TokenizedStringArray()
        { }

        public TokenizedStringArray(
            TokenizedString input)
            :
            base(new [] {input})
        { }

        public TokenizedStringArray(
            System.Collections.Generic.IEnumerable<TokenizedString> input)
            :
            base(input)
        { }
    }
}

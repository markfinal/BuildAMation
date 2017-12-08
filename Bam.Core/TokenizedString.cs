#define NEW_PARSER
#region License
// Copyright (c) 2010-2017, Mark Final
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
    /// Strings are tokenized by macros and functions. Macros are themselves TokenizedStrings
    /// and so there is a recursive expansion to evaluate the resulting string (referred to as parsing).
    /// </summary>
    /// <remarks>
    /// Tokens are identified by $( and ).
    /// A numeric index within a token, e.g. $(0), represents the index into the list of macros passed into the TokenizedString creation function. Parsing is performed recursively, although macros are not shared between repeated parsing calls.
    /// <para />
    /// Functions can run before or after token expansion.
    /// <para />
    /// Pre-functions are run before token expansion, and are identified by #name(...):
    /// <list type="bullet">
    /// <item><description><code>#valid(expr[,default])</code></description> If the expression is a valid
    /// TokenizedString, expand it and use it, otherwise the entire function call is replaced with the 'default' expression, unless
    /// this is omitted, and an empty string is used.</item>
    /// <item><description><code>#inline(index)</code></description> Inline the original string.</item>
    /// </list>
    /// Post-functions are run after token expansion, and are identified by @(...):
    /// <list type="bullet">
    /// <item><description><code>@basename(path)</code></description> Return the filename excluding extension in the path.</item>
    /// <item><description><code>@filename(path)</code></description> Return the filename including extension in the path.</item>
    /// <item><description><code>@dir_(path)</code></description> Return the parent directory of path. (Remove the underscore from the name)</item>
    /// <item><description><code>@normalize(path)</code></description> Return the full path of path, without any special directories.</item>
    /// <item><description><code>@changeextension(path,ext)</code></description> Change the extension of the file in path, to ext.</item>
    /// <item><description><code>@removetrailingseparator(path)</code></description> Remove any directory separator characters from the end of path.</item>
    /// <item><description><code>@relativeto(path,baseDir)</code></description> Return the relative path from baseDir. If there is no common root between them, path is returned.</item>
    /// <item><description><code>@trimstart(path,totrim)</code></description> Trim string from the start of path.</item>
    /// <item><description><code>@escapedquotes(path)</code></description> Ensure that the path is double quoted, suitable for use with preprocessor definitions.</item>
    /// <item><description><code>@ifnotempty(path,whennotempty,whenempty)</code></description> If path is not empty, replace the expression with that in whennotempty, otherwise use whenempty.</item>
    /// </list>
    /// Custom unary post-functions can be registered using <code>registerPostUnaryFunction</code>.
    /// </remarks>
    public sealed class TokenizedString
    {
        [System.Flags]
        private enum EFlags
        {
            None = 0,
#if NEW_PARSER
#else
            Inline = 0x1,
#endif
            NoCache = 0x2
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
        private static readonly string NumericTokenRegExPattern = @"([0-9]+)";

        // pre-functions look like: #functionname(expression)
        // note: this is using balancing groups in order to handle nested function calls, or any other instances of parentheses in paths (e.g. Windows 'Program Files (x86)')
        private static readonly string PreFunctionRegExPattern = @"(#(?<func>[a-z]+)\((?<expression>[^\(\)]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\))";

        // post-functions look like: @functionname(expression)
        // note: this is using balancing groups in order to handle nested function calls, or any other instances of parentheses in paths (e.g. Windows 'Program Files (x86)')
        private static readonly string PostFunctionRegExPattern = @"(@(?<func>[a-z]+)\((?<expression>[^\(\)]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\))";

        private static readonly string[] BuiltInPostFunctionNames =
        {
            "basename",
            "filename",
            "dir",
            "normalize",
            "changeextension",
            "removetrailingseparator",
            "relativeto",
            "trimstart",
            "escapedquotes",
            "ifnotempty"
        };

        // static fields, initialised in reset()
        private static System.Collections.Generic.Dictionary<System.Int64, TokenizedString> VerbatimCacheMap;
        private static System.Collections.Generic.Dictionary<System.Int64, TokenizedString> NoModuleCacheMap;
        private static System.Collections.Generic.List<TokenizedString> AllStrings;
        private static System.Collections.Generic.List<TokenizedString> StringsForParsing = new System.Collections.Generic.List<TokenizedString>();
        private static bool AllStringsParsed;
        private static System.Collections.Generic.Dictionary<string, System.Func<string, string>> CustomPostUnaryFunctions;
        private static System.TimeSpan RegExTimeout;
        private static bool RecordStackTraces;

        // instance fields
        private System.Collections.Generic.List<string> Tokens = null;
        private Module ModuleWithMacros = null;
        private string OriginalString = null;
        private string ParsedString = null;
        private bool Verbatim;
        private TokenizedStringArray PositionalTokens = new TokenizedStringArray();
        private string CreationStackTrace = null;
        private int RefCount = 1;
        private EFlags Flags = EFlags.None;
#if NEW_PARSER
#else
        private TokenizedString Alias = null;
#endif
        private string parsingErrorMessage = null;
        private long hash = 0;
        private string parsedStackTrace = null;

        /// <summary>
        /// Register a custom unary post function to use in TokenizedString parsing.
        /// The name must not collide with any built-in functions, or any existing custom unary post functions.
        /// </summary>
        /// <param name="name">Name of the function that must be unique.</param>
        /// <param name="function">Function to apply to any usage of @name in TokenizedStrings.</param>
        public static void
        registerPostUnaryFunction(
            string name,
            System.Func<string, string> function)
        {
            if (BuiltInPostFunctionNames.Contains(name))
            {
                throw new Exception("Unable to register post unary function due to name collision with builtin functions, '{0}'", name);
            }
            if (CustomPostUnaryFunctions.ContainsKey(name))
            {
                throw new Exception("Unable to register post unary function because post function '{0}' already exists.", name);
            }
            CustomPostUnaryFunctions.Add(name, function);
        }

#if NEW_PARSER
#else
        /// <summary>
        /// Query if the TokenizedString has been aliased to another.
        /// </summary>
        /// <value><c>true</c> if this instance is aliased; otherwise, <c>false</c>.</value>
        public bool
        IsAliased
        {
            get
            {
                return (null != this.Alias);
            }
        }

        /// <summary>
        /// Alias to another string. Useful for placeholder strings.
        /// </summary>
        /// <param name="alias">Alias.</param>
        public void
        Aliased(
            TokenizedString alias)
        {
            if (this.IsAliased)
            {
                throw new Exception("TokenizedString is already aliased");
            }
            this.Alias = alias;
            ++alias.RefCount;
        }
#endif

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
                // was at least one substring captured by the regex?
                if (!match.Success)
                {
                    continue;
                }
                // there is >1 groups, as the first is the original expression, so skip it
                foreach (var group in match.Groups.Cast<System.Text.RegularExpressions.Group>().Skip(1))
                {
                    yield return group.Value;
                }
            }
        }

        private static string
        getStacktrace()
        {
            if (RecordStackTraces)
            {
                return System.Environment.StackTrace;
            }
            return string.Empty;
        }

        /// <summary>
        /// Reset all static state of the TokenizedString class.
        /// This function is only really useful in unit tests.
        /// </summary>
        public static void
        reset()
        {
            VerbatimCacheMap = new System.Collections.Generic.Dictionary<System.Int64, TokenizedString>();
            NoModuleCacheMap = new System.Collections.Generic.Dictionary<System.Int64, TokenizedString>();
            AllStrings = new System.Collections.Generic.List<TokenizedString>();
            StringsForParsing = new System.Collections.Generic.List<TokenizedString>();
            AllStringsParsed = false;
            CustomPostUnaryFunctions = new System.Collections.Generic.Dictionary<string, System.Func<string, string>>();
            RecordStackTraces = false;
            RegExTimeout = System.TimeSpan.FromSeconds(5);
        }

        static TokenizedString()
        {
            reset();
            RecordStackTraces = CommandLineProcessor.Evaluate(new Options.RecordStackTrace());
            if (RecordStackTraces)
            {
                Log.Info("WARNING: TokenizedString stack trace recording enabled. This will slow down your build.");
            }
        }

        private TokenizedString(
            string original,
            Module moduleWithMacros,
            bool verbatim,
            TokenizedStringArray positionalTokens,
            EFlags flags)
        {
            this.CreationStackTrace = getStacktrace();
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
                this.parsingErrorMessage = null; // as verbatim strings are already parsed
                this.parsedStackTrace = getStacktrace();
            }
            else
            {
#if NEW_PARSER
#else
                if (this.IsInline)
                {
                    this.parsingErrorMessage = null; // as inline strings do not get parsed
                }
                else
#endif
                {
                    this.parsingErrorMessage = "not been parsed";
                }
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

            // https://cs.stackexchange.com/questions/45287/why-does-this-particular-hashcode-function-help-decrease-collisions
            System.Int64 hash = 17;
            hash = hash * 31 + tokenizedString.GetHashCode();

            // strings can be created during the multithreaded phase, so synchronize on the cache used
            if (verbatim)
            {
                // covers all verbatim strings
                lock (VerbatimCacheMap)
                {
                    var useCache = (0 == (flags & EFlags.NoCache));
                    if (useCache)
                    {
                        TokenizedString foundTS;
                        if (VerbatimCacheMap.TryGetValue(hash, out foundTS))
                        {
                            ++foundTS.RefCount;
                            return foundTS;
                        }
                    }
                    var newTS = new TokenizedString(tokenizedString, macroSource, verbatim, positionalTokens, flags);
                    if (useCache)
                    {
                        VerbatimCacheMap.Add(hash, newTS);
                    }
                    newTS.hash = hash;
                    lock (AllStrings)
                    {
                        AllStrings.Add(newTS);
                    }
                    return newTS;
                }
            }
            else
            {
                // covers all strings associated with a module (for macros), or no module but with positional arguments
                var stringCache = (null != macroSource) ? macroSource.TokenizedStringCacheMap : NoModuleCacheMap;
                lock (stringCache)
                {
                    if (null != macroSource)
                    {
                        hash = hash * 31 + macroSource.GetHashCode();
                    }
                    if (null != positionalTokens)
                    {
                        foreach (var posToken in positionalTokens)
                        {
                            hash = hash * 31 + posToken.GetHashCode();
                        }
                    }
                    var useCache = (0 == (flags & EFlags.NoCache));
                    if (useCache)
                    {
                        TokenizedString foundTS;
                        if (stringCache.TryGetValue(hash, out foundTS))
                        {
                            ++foundTS.RefCount;
                            return foundTS;
                        }
                    }
                    var newTS = new TokenizedString(tokenizedString, macroSource, verbatim, positionalTokens, flags);
                    if (useCache)
                    {
                        stringCache.Add(hash, newTS);
                    }
                    newTS.hash = hash;
                    lock (AllStrings)
                    {
                        AllStrings.Add(newTS);
                    }
                    lock (StringsForParsing)
                    {
                        StringsForParsing.Add(newTS);
                    }
                    return newTS;
                }
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

#if NEW_PARSER
#else
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
#endif

        /// <summary>
        /// Utility method to create a TokenizedString which will not be cached with any other existing
        /// TokenizedStrings that share the same original string.
        /// Such TokenizedStrings are intended to be aliased at a future time.
        /// </summary>
        /// <param name="uncachedString">The string that will be uncached.</param>
        /// <param name="macroSource">The Module containing macros that will be eventually referenced.</param>
        /// <param name="positionalTokens">Positional tokens.</param>
        /// <returns>A unique TokenizedString.</returns>
        public static TokenizedString
        CreateUncached(
            string uncachedString,
            Module macroSource,
            TokenizedStringArray positionalTokens = null)
        {
            return CreateInternal(uncachedString, macroSource, false, positionalTokens, EFlags.NoCache);
        }

        /// <summary>
        /// Determine if the TokenizedString has been parsed already.
        /// Sometimes useful if a TokenizedString is created after the ParseAll step, but is repeated
        /// as a dependency.
        /// </summary>
        public bool IsParsed
        {
            get
            {
#if NEW_PARSER
#else
                if (this.IsAliased)
                {
                    return this.Alias.IsParsed;
                }
#endif
                if (this.Verbatim)
                {
                    return true;
                }
#if NEW_PARSER
                var hasTokens = (null != this.Tokens);
                var hasParsedString = (null != this.ParsedString);
                return !hasTokens && hasParsedString;
#else
                if (this.IsInline)
                {
                    return false;
                }
                foreach (var positionalToken in this.PositionalTokens)
                {
                    if (!positionalToken.IsParsed)
                    {
                        return false;
                    }
                }
                return (null != this.ParsedString);
#endif
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
        /// Return the parsed string.
        /// If the string has not been parsed, or unsuccessfully parsed, an exception is thrown.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Bam.Core.TokenizedString"/>.</returns>
        public override string
        ToString()
        {
#if NEW_PARSER
#else
            if (this.IsAliased)
            {
                return this.Alias.ToString();
            }
#endif
            if (null != this.parsingErrorMessage || null == this.ParsedString)
            {
                throw new Exception("TokenizedString '{0}' has {4}{3}{1}{1}Created at:{1}{2}{1}{1}",
                    this.OriginalString,
                    System.Environment.NewLine,
                    this.CreationStackTrace,
                    AllStringsParsed ? " after the string parsing phase" : string.Empty,
                    this.parsingErrorMessage);
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
#if NEW_PARSER
#else
                if (this.IsAliased)
                {
                    return this.Alias.Empty;
                }
#endif
                return (null == this.Tokens) || !this.Tokens.Any();
            }
        }

#if NEW_PARSER
#else
        private bool IsInline
        {
            get
            {
                return (EFlags.Inline == (this.Flags & EFlags.Inline));
            }
        }
#endif

        /// <summary>
        /// Parse every TokenizedString.
        /// </summary>
        public static void
        ParseAll()
        {
            Log.Detail("Parsing strings...");
            var scale = 100.0f / StringsForParsing.Count;
            var count = 0;
            foreach (var t in StringsForParsing)
            {
#if NEW_PARSER
#else
                if (t.IsInline)
                {
                    Log.DebugMessage("Not parsing inline string: {0}", t.OriginalString);
                    continue;
                }
#endif

                t.ParseInternalWithAlreadyParsedCheck(null);
                Log.DetailProgress("{0,3}%", (int)(++count * scale));
            }
            AllStringsParsed = true;
        }

        /// <summary>
        /// Parsed a TokenizedString.
        /// Pre-functions are evaluated first.
        /// The order of source of tokens are checked in the follow order:
        /// - Positional tokens.
        /// - Any custom macros (will be none in this context)
        /// - Global macros (from the Graph)
        /// - Macros in the associated Module
        /// - Macros in the Tool associated with the Module
        /// - Environment variables
        /// After token expansion, post-functions are then evaluated.
        /// No string is returned, use ToString().
        /// Failure to parse are stored in the TokenizedString and will be displayed as an exception
        /// message when used.
        /// </summary>
        public void
        Parse()
        {
            if (this.ParsedString != null)
            {
                throw new Exception("TokenizedString '{0}' is already parsed{4}.{1}{1}Created at:{1}{2}{1}{1}Parsed at:{1}{3}",
                    this.OriginalString,
                    System.Environment.NewLine,
                    this.CreationStackTrace,
                    this.parsedStackTrace,
                    AllStringsParsed ? " after the string parsing phase" : string.Empty);
            }
            this.ParseInternalWithAlreadyParsedCheck(null);
            lock (StringsForParsing)
            {
                StringsForParsing.Remove(this);
            }
        }

        /// <summary>
        /// Parsed a TokenizedString with a custom source of macro overrides.
        /// This performs a similar operation to Parse(), except that the parsed string is not saved, but is returned
        /// from the function.
        /// This allows TokenizedStrings to be re-parsed with different semantics to their tokens, but will not affect
        /// the existing parse result.
        /// The array of MacroLists is evaluated from front to back, so if there are duplicate macros in several MacroLists
        /// the first encountered will be the chosen value.
        /// No errors or exceptions are reported or saved from using this function, so use it sparingly and with care.
        /// </summary>
        /// <param name="customMacroArray">Array of custom macros.</param>
        public string
        UncachedParse(
            Array<MacroList> customMacroArray)
        {
            return this.ParseInternal(customMacroArray);
        }

        private string
        ParseInternalWithAlreadyParsedCheck(
            Array<MacroList> customMacroArray)
        {
            if (this.ParsedString != null)
            {
                return this.ParsedString;
            }
            return this.ParseInternal(customMacroArray);
        }

#if NEW_PARSER
        private string
        GetParsedString(
            Array<MacroList> customMacroArray)
        {
            if (null == this.ParsedString)
            {
                this.ParseInternal(customMacroArray);
                //throw new Exception("String '{0}' has yet to be parsed", this.OriginalString);
            }
            return this.ParsedString;
        }
#endif

        private string
        ParseInternal(
            Array<MacroList> customMacroArray)
        {
#if NEW_PARSER
#if false
            if (this.IsAliased)
            {
                // TODO: aliasing messes up the order of parsing
                return this.Alias.ParseInternal(customMacroArray);
            }
#endif
            var graph = Graph.Instance;
            var parsedString = new System.Text.StringBuilder();
            var tokens = SplitIntoTokens(this.EvaluatePreFunctions(this.OriginalString, customMacroArray), TokenRegExPattern).ToList<string>();
            for (int index = 0; index < tokens.Count;)
            {
                var token = tokens[index];

                // if not identified as a token, just add the string, and move along
                if (!(token.StartsWith(TokenPrefix) && token.EndsWith(TokenSuffix)))
                {
                    parsedString.Append(token);
                    tokens.Remove(token);
                    continue;
                }

                // step 1: if the token is a positional token, inline, and add outstanding tokens
                var positional = GetMatches(token, PositionalTokenRegExPattern).FirstOrDefault();
                if (!System.String.IsNullOrEmpty(positional))
                {
                    var positionalIndex = System.Convert.ToInt32(positional);
                    if (positionalIndex > this.PositionalTokens.Count)
                    {
                        throw new Exception("TokenizedString positional token at index {0} requested, but only {1} positional values given. Created at {2}.", positionalIndex, this.PositionalTokens.Count, this.CreationStackTrace);
                    }
                    try
                    {
                        var posTokenStr = this.PositionalTokens[positionalIndex];
                        tokens.Remove(token);
                        if (null == posTokenStr.Tokens)
                        {
                            parsedString.Append(posTokenStr.GetParsedString(customMacroArray));
                        }
                        else
                        {
                            tokens.InsertRange(index, posTokenStr.Tokens);
                        }
                    }
                    catch (System.ArgumentOutOfRangeException ex)
                    {
                        throw new Exception(ex, "Positional token index {0} exceeded number of tokens available", positionalIndex, this.PositionalTokens.Count);
                    }
                    continue;
                }

                // step 2 : try to resolve with custom macros passed to the Parse function
                if (null != customMacroArray &&
                    (null != customMacroArray.FirstOrDefault(item => item.Dict.ContainsKey(token))))
                {
                    var containingMacroList = customMacroArray.First(item => item.Dict.ContainsKey(token));
                    var customTokenStr = containingMacroList.Dict[token];
                    tokens.Remove(token);
                    if (null == customTokenStr.Tokens)
                    {
                        parsedString.Append(customTokenStr.GetParsedString(customMacroArray));
                    }
                    else
                    {
                        tokens.InsertRange(index, customTokenStr.Tokens);
                    }
                    continue;
                }

                // step 3 : try macros in the global Graph, common to all modules
                if (graph.Macros.Dict.ContainsKey(token))
                {
                    var graphTokenStr = graph.Macros.Dict[token];
                    tokens.Remove(token);
                    if (null == graphTokenStr.Tokens)
                    {
                        parsedString.Append(graphTokenStr.GetParsedString(customMacroArray));
                    }
                    else
                    {
                        tokens.InsertRange(index, graphTokenStr.Tokens);
                    }
                    continue;
                }

                if (this.ModuleWithMacros != null)
                {
                    var tool = this.ModuleWithMacros.Tool;
                    // step 4 : try macros in the specific module
                    if (this.ModuleWithMacros.Macros.Dict.ContainsKey(token))
                    {
                        var moduleMacroStr = this.ModuleWithMacros.Macros.Dict[token];
                        tokens.Remove(token);
                        if (null == moduleMacroStr.Tokens)
                        {
                            parsedString.Append(moduleMacroStr.GetParsedString(customMacroArray));
                        }
                        else
                        {
                            tokens.InsertRange(index, moduleMacroStr.Tokens);
                        }
                        continue;
                    }
                    // step 5 : try macros in the Tool attached to the specific module
                    else if (null != tool && tool.Macros.Dict.ContainsKey(token))
                    {
                        var moduleToolMacroStr = tool.Macros.Dict[token];
                        tokens.Remove(token);
                        if (null == moduleToolMacroStr.Tokens)
                        {
                            parsedString.Append(moduleToolMacroStr.GetParsedString(customMacroArray));
                        }
                        else
                        {
                            tokens.InsertRange(index, moduleToolMacroStr.Tokens);
                        }
                        continue;
                    }
                }

                // step 6 : try the immediate environment
                var strippedToken = SplitIntoTokens(token, ExtractTokenRegExPattern).First();
                var envVar = System.Environment.GetEnvironmentVariable(strippedToken);
                if (null != envVar)
                {
                    parsedString.Append(envVar);
                    tokens.Remove(token);
                    continue;
                }

                // step 7 : original token must be honoured, as it might be resolved in a later inlining step
                parsedString.Append(token);
                ++index;
            }

            if (tokens.Any())
            {
                if (null != customMacroArray)
                {
                    throw new Exception("String cannot be fully parsed with the custom macros provided");
                }
                this.Tokens = tokens;
                this.ParsedString = parsedString.ToString();
                return this.ParsedString;
            }
            else
            {
                this.Tokens = null;
                var functionEvaluated = this.EvaluatePostFunctions(NormalizeDirectorySeparators(parsedString.ToString()));
                // when using a custom array of MacroLists, do not store the parsed string
                // instead just return it
                // this allows a TokenizedString to be re-parsed with different semantics, but does not
                // permanently change it
                if (null == customMacroArray)
                {
                    this.ParsedString = functionEvaluated;
                    this.parsingErrorMessage = null;
                    this.parsedStackTrace = getStacktrace();
                    Log.DebugMessage(" '{0}' --> '{1}'", this.OriginalString, this.ToString());
                }
                else
                {
                    Log.DebugMessage(" '{0}' --> '{1}' (using custom macros)", this.OriginalString, functionEvaluated);
                }
                return functionEvaluated;
            }
#else
            if (this.IsInline)
            {
                throw new Exception("Inline TokenizedString cannot be parsed, {0}", this.OriginalString);
            }
            if (this.IsAliased)
            {
                this.Alias.ParseInternalWithAlreadyParsedCheck(customMacroArray);
            }

            this.Tokens = SplitIntoTokens(this.EvaluatePreFunctions(this.OriginalString, customMacroArray), TokenRegExPattern).ToList<string>();

            System.Func<TokenizedString, TokenizedString, int, string> expandTokenizedString = (source, tokenString, currentIndex) =>
                {
                    if (!tokenString.IsParsed)
                    {
                        if (source == tokenString)
                        {
                            throw new Exception("Infinite recursion for {0}. Created at {3}", source.OriginalString, source.CreationStackTrace);
                        }

                        if (tokenString.IsInline)
                        {
                            // current token expands to nothing, and the inline string's tokens are processed next
                            source.Tokens.InsertRange(currentIndex + 1, SplitIntoTokens(tokenString.EvaluatePreFunctions(tokenString.OriginalString, new Array<MacroList>{ source.ModuleWithMacros.Macros }), TokenRegExPattern).ToList<string>());
                            return string.Empty;
                        }

                        // recursive
                        tokenString.ParseInternalWithAlreadyParsedCheck(null);
                        if (!tokenString.IsParsed)
                        {
                            // create a list of MacroLists, starting with the original Module's macro
                            // and then appending the Module in the parent TokenizedString
                            // it's a single level addition
                            // priority is thus taken for original macros, but any missing macros
                            // from that first macrolist can now be looked for in the second
                            var macroList = new Array<MacroList>();
                            if (null != tokenString.ModuleWithMacros)
                            {
                                macroList.Add(tokenString.ModuleWithMacros.Macros);
                            }
                            macroList.Add(source.ModuleWithMacros.Macros);
                            var customResult = tokenString.ParseInternalWithAlreadyParsedCheck(macroList);
                            if (null != customResult)
                            {
                                return customResult;
                            }
                        }
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
                    try
                    {
                        var posTokenStr = this.PositionalTokens[positionalIndex];
                        parsedString.Append(expandTokenizedString(this, posTokenStr, index));
                    }
                    catch (System.ArgumentOutOfRangeException ex)
                    {
                        throw new Exception(ex, "Positional token index {0} exceeded number of tokens available", positionalIndex, this.PositionalTokens.Count);
                    }
                    continue;
                }
                // step 2 : try to resolve with custom macros passed to the Parse function
                else if (null != customMacroArray && (null != customMacroArray.FirstOrDefault(item => item.Dict.ContainsKey(token))))
                {
                    var containingMacroList = customMacroArray.First(item => item.Dict.ContainsKey(token));
                    parsedString.Append(expandTokenizedString(this, containingMacroList.Dict[token], index));
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
                    message.AppendFormat("unrecognized token '{0}'", token);
                    message.AppendLine();
                    if (null != customMacroArray)
                    {
                        message.AppendFormat("Searched in {0} custom macro lists", customMacroArray.Count);
                        message.AppendLine();
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
                    this.parsingErrorMessage = message.ToString();
                    return string.Empty;
                }
            }
            var functionEvaluated = this.EvaluatePostFunctions(NormalizeDirectorySeparators(parsedString.ToString()));
            // when using a custom array of MacroLists, do not store the parsed string
            // instead just return it
            // this allows a TokenizedString to be re-parsed with different semantics, but does not
            // permanently change it
            if (null == customMacroArray)
            {
                this.ParsedString = functionEvaluated;
                this.parsingErrorMessage = null;
                this.parsedStackTrace = getStacktrace();
                Log.DebugMessage("Converted '{0}' to '{1}'", this.OriginalString, this.ToString());
            }
            else
            {
                Log.DebugMessage("Converted (with custom macros) '{0}' to '{1}'", this.OriginalString, functionEvaluated);
            }
            return functionEvaluated;
#endif
        }

        private string
        EvaluatePostFunctions(
            string sourceExpression)
        {
            System.Text.RegularExpressions.MatchCollection matches = null;
            try
            {
                matches = System.Text.RegularExpressions.Regex.Matches(
                    sourceExpression,
                    PostFunctionRegExPattern,
                    System.Text.RegularExpressions.RegexOptions.None,
                    RegExTimeout);
                if (0 == matches.Count)
                {
                    return sourceExpression;
                }
            }
            catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
            {
                var message = new System.Text.StringBuilder();
                message.AppendFormat("TokenizedString post-function regular expression matching timed out after {0} seconds. Check details below for errors.", RegExTimeout.Seconds);
                message.AppendLine();
                message.AppendFormat("String being parsed: {0}", sourceExpression);
                message.AppendLine();
                message.AppendFormat("Regex              : {0}", PostFunctionRegExPattern);
                message.AppendLine();
                message.AppendFormat("Tokenized string {0} created at", this.OriginalString);
                message.AppendLine();
                message.AppendLine(this.CreationStackTrace);
                throw new Exception(message.ToString());
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
                        if (split.Length != 2)
                        {
                            throw new Exception("Expected 2, not {0}, arguments in the function call {1}({2}) in {3}",
                                split.Length,
                                functionName,
                                argument,
                                this.OriginalString);
                        }
                        var original = split[0];
                        var extension = split[1].Trim();
                        var changed = System.IO.Path.ChangeExtension(original, extension);
                        return changed;
                    }

                case "removetrailingseparator":
                    return argument.TrimEnd(System.IO.Path.DirectorySeparatorChar);

                case "relativeto":
                    {
                        var split = argument.Split(',');
                        if (split.Length != 2)
                        {
                            throw new Exception("Expected 2, not {0}, arguments in the function call {1}({2}) in {3}",
                                split.Length,
                                functionName,
                                argument,
                                this.OriginalString);
                        }
                        var path = split[0];
                        var root = split[1] + System.IO.Path.DirectorySeparatorChar;
                        var relative = RelativePathUtilities.GetPath(path, root);
                        return relative;
                    }

                case "trimstart":
                    {
                        var split = argument.Split(',');
                        if (split.Length != 2)
                        {
                            throw new Exception("Expected 2, not {0}, arguments in the function call {1}({2}) in {3}",
                                split.Length,
                                functionName,
                                argument,
                                this.OriginalString);
                        }
                        var original = split[0];
                        var totrim = split[1];
                        while (original.StartsWith(totrim))
                        {
                            original = original.Replace(totrim, string.Empty);
                        }
                        return original;
                    }

                case "escapedquotes":
                    {
                        if (OSUtilities.IsWindowsHosting)
                        {
                            // on Windows, escape any backslashes, as these are normal Windows paths
                            // so don't interpret them as control characters
                            argument = argument.Replace("\\", "\\\\");
                        }
                        return System.String.Format("\"{0}\"", argument);
                    }

                case "ifnotempty":
                    {
                        var split = argument.Split(',');
                        if (split.Length != 3)
                        {
                            throw new Exception("Expected 3, not {0}, arguments in the function call {1}({2}) in {3}",
                                split.Length,
                                functionName,
                                argument,
                                this.OriginalString);
                        }
                        var predicateString = split[0];
                        if (!System.String.IsNullOrEmpty(predicateString))
                        {
                            var positiveString = split[1];
                            return positiveString;
                        }
                        else
                        {
                            var negativeString = split[2];
                            return negativeString;
                        }
                    }

                default:
                    {
                        // search through custom functions
                        if (CustomPostUnaryFunctions.ContainsKey(functionName))
                        {
                            return CustomPostUnaryFunctions[functionName](argument);
                        }
                        throw new Exception("Unknown post-function '{0}' in TokenizedString '{1}'", functionName, this.OriginalString);
                    }
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
                if (!this.IsParsed)
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
        /// Are two strings identical? This includes comparing how the string was constructed.
        /// It does not necessarily mean that the parsed strings are identical. Use ToString().Equals() to achieve that test.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Bam.Core.TokenizedString"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Bam.Core.TokenizedString"/>; otherwise, <c>false</c>.</returns>
        public override bool
        Equals(
            object obj)
        {
            var other = obj as TokenizedString;
            var equals = this.hash == other.hash;
            return equals;
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
        /// Quote the string if it contains a space
        /// </summary>
        /// <returns>The string and quote if necessary.</returns>
        public string ToStringQuoteIfNecessary()
        {
#if NEW_PARSER
#else
            if (this.IsAliased)
            {
                return this.Alias.ToStringQuoteIfNecessary();
            }
#endif
            var contents = this.ToString();
            if (!this.ContainsSpace)
            {
                return contents;
            }
            return System.String.Format("\"{0}\"", contents);
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
                return AllStrings.Count();
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
                return AllStrings.Where(item => item.RefCount == 1).Count();
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
#if NEW_PARSER
            foreach (var item in AllStrings.OrderBy(item => item.RefCount).ThenBy(item => !item.Verbatim))
            {
                Log.DebugMessage("#{0} {1}'{2}'{3} {4}",
                    item.RefCount,
                    item.Verbatim ? "<verbatim>" : string.Empty,
                    item.OriginalString,
                    item.Verbatim ? "</verbatim>" : string.Empty,
                    item.ModuleWithMacros != null ? System.String.Format("(ref: {0})", item.ModuleWithMacros.GetType().ToString()) : string.Empty);
            }
#else
            foreach (var item in AllStrings.OrderBy(item => item.RefCount).ThenBy(item => !item.Verbatim).ThenBy(item => item.IsAliased))
            {
                Log.DebugMessage("#{0} {1}'{2}'{3} {4} {5}",
                    item.RefCount,
                    item.Verbatim ? "<verbatim>" : string.Empty,
                    item.OriginalString,
                    item.Verbatim ? "</verbatim>" : string.Empty,
                    item.IsAliased ? System.String.Format("Aliased to: '{0}'", item.Alias.OriginalString) : string.Empty,
                    item.ModuleWithMacros != null ? System.String.Format("(ref: {0})", item.ModuleWithMacros.GetType().ToString()) : string.Empty);
            }
#endif
        }

        private bool
        IsTokenValid(
            string token,
            Array<MacroList> customMacroArray)
        {
            // step 1 : is the token positional, i.e. was set up at creation time
            var positional = GetMatches(token, PositionalTokenRegExPattern).FirstOrDefault();
            if (!System.String.IsNullOrEmpty(positional))
            {
                var positionalIndex = System.Convert.ToInt32(positional);
                return (positionalIndex <= this.PositionalTokens.Count);
            }
            // step 2 : try to resolve with custom macros passed to the Parse function
            else if (null != customMacroArray && (null != customMacroArray.FirstOrDefault(item => item.Dict.ContainsKey(token))))
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
            Array<MacroList> customMacroArray)
        {
            System.Text.RegularExpressions.MatchCollection matches = null;
            try
            {
                matches = System.Text.RegularExpressions.Regex.Matches(
                    originalExpression,
                    PreFunctionRegExPattern,
                    System.Text.RegularExpressions.RegexOptions.None,
                    RegExTimeout);
                if (0 == matches.Count)
                {
                    return originalExpression;
                }
            }
            catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
            {
                var message = new System.Text.StringBuilder();
                message.AppendFormat("TokenizedString pre-function regular expression matching timed out after {0} seconds. Check details below for errors.", RegExTimeout.Seconds);
                message.AppendLine();
                message.AppendFormat("String being parsed: {0}", originalExpression);
                message.AppendLine();
                message.AppendFormat("Regex              : {0}", PreFunctionRegExPattern);
                message.AppendLine();
                message.AppendFormat("Tokenized string {0} created at", this.OriginalString);
                message.AppendLine();
                message.AppendLine(this.CreationStackTrace);
                throw new Exception(message.ToString());
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
                var expression = this.EvaluatePreFunctions(expressionText.ToString(), customMacroArray);
                switch (functionName)
                {
                    case "valid":
                        {
                            var split = expression.Split(',');
                            var replacement = (1 == split.Length) ? string.Empty : split[1];
                            var tokens = SplitIntoTokens(split[0], TokenRegExPattern);
                            var allTokensValid = true;
                            foreach (var token in tokens)
                            {
                                if (!(token.StartsWith(TokenPrefix) && token.EndsWith(TokenSuffix)))
                                {
                                    continue;
                                }

                                // Note: with nested valid pre-functions, macros can be validated as many times as they are nested
                                if (this.IsTokenValid(token, customMacroArray))
                                {
                                    continue;
                                }

                                allTokensValid = false;
                                break;
                            }

                            if (allTokensValid)
                            {
                                modifiedString = modifiedString.Replace(match.Value, split[0]);
                            }
                            else
                            {
                                modifiedString = modifiedString.Replace(match.Value, replacement);
                            }
                        }
                        break;

                    case "inline":
                        {
                            var positional = GetMatches(expression, NumericTokenRegExPattern).FirstOrDefault();
                            if (!System.String.IsNullOrEmpty(positional))
                            {
                                var positionalIndex = System.Convert.ToInt32(positional);
                                if (positionalIndex > this.PositionalTokens.Count)
                                {
                                    throw new Exception("TokenizedString positional token at index {0} requested, but only {1} positional values given. Created at {2}.", positionalIndex, this.PositionalTokens.Count, this.CreationStackTrace);
                                }
                                modifiedString = modifiedString.Replace(match.Value, this.PositionalTokens[positionalIndex].OriginalString);
                            }
                            else
                            {
                                throw new Exception("Unrecognized positional token '{0}' in pre-function '{1}' in TokenizedString '{2}'",
                                    expression, functionName, this.OriginalString);
                            }
                        }
                        break;

                    default:
                        throw new Exception("Unknown pre-function '{0}' in TokenizedString '{1}'", functionName, this.OriginalString);
                }
            }
            return modifiedString;
        }

        /// <summary>
        /// Remove all strings referencing a module type, including those that are not yet parsed.
        /// </summary>
        /// <param name="moduleType"></param>
        static public void
        RemoveEncapsulatedStrings(
            System.Type moduleType)
        {
            lock (AllStrings)
            {
                var toRemove = AllStrings.Where(
                    item => item.ModuleWithMacros != null && item.ModuleWithMacros.GetType() == moduleType);
                foreach (var i in toRemove.ToList())
                {
                    i.RefCount--;

                    if (0 == i.RefCount)
                    {
                        Log.DebugMessage("Removing string {0} from {1}", i.OriginalString, moduleType.ToString());
                        AllStrings.Remove(i);
                        // Don't believe a separate lock is needed for StringsForParsing
                        if (StringsForParsing.Contains(i))
                        {
                            StringsForParsing.Remove(i);
                        }
                    }
                }
            }
        }
    }
}

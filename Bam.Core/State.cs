#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace Bam.Core
{
namespace V2
{
    using System.Linq;

    /// <summary>
    /// Extension functions
    /// </summary>
    static class Extensions
    {
        // ref: http://stackoverflow.com/questions/521687/c-sharp-foreach-with-index
        static public void Each<T>(this System.Collections.Generic.IEnumerable<T> ie, System.Action<T, int> action)
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
        private static readonly string FunctionRegExPattern = @"(@([a-z]+)\((.+)\))";
        private static readonly string FunctionPrefix = @"@";

        private static System.Collections.Generic.List<TokenizedString> Cache = new System.Collections.Generic.List<TokenizedString>();

        private System.Collections.Generic.List<string> Tokens = null;
        private System.Collections.Generic.List<int> MacroIndices = null;
        private Module ModuleWithMacros = null;
        private string OriginalString = null;
        private string ParsedString = null;
        private bool Verbatim;

        static private System.Collections.Generic.IEnumerable<string> SplitToParse(string original, string regExPattern)
        {
            var matches = System.Text.RegularExpressions.Regex.Split(original, regExPattern);
            var filtered = matches.Where(item => !System.String.IsNullOrEmpty(item));
            return filtered;
        }

        private TokenizedString(
            string original,
            bool verbatim = false)
        {
            this.Verbatim = verbatim;
            this.OriginalString = original;
            if (verbatim)
            {
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
            bool verbatim) :
            this(original, verbatim)
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
            bool verbatim = false)
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
                var ts = new TokenizedString(tokenizedString, macroSource, verbatim);
                Cache.Add(ts);
                return ts;
            }
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
                return this.Verbatim || (null != this.ParsedString);
            }
        }

        private static string JoinTokens(System.Collections.Generic.List<string> tokens)
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

        public override string ToString()
        {
            return this.IsExpanded ? this.ParsedString : this.OriginalString;
        }

        public bool Empty
        {
            get
            {
                return (null == this.Tokens) || (0 == this.Tokens.Count());
            }
        }

        public static void ParseAll()
        {
            foreach (var t in Cache)
            {
                t.ParsedString = t.Parse();
            }
        }

        public string Parse()
        {
            return this.Parse(null);
        }

        public string Parse(MacroList customMacros)
        {
            if (this.IsExpanded && (null == customMacros))
            {
                return this.ParsedString;
            }
            // take a copy of the macro indices
            var macroIndices = new System.Collections.Generic.List<int>(this.MacroIndices);
            var tokens = new System.Collections.Generic.List<string>(this.Tokens); // could just be a reserved list of strings
            foreach (int index in this.MacroIndices.Reverse<int>())
            {
                var token = this.Tokens[index];
                if (null != customMacros && customMacros.Dict.ContainsKey(token))
                {
                    var value = customMacros.Dict[token];
                    if (!value.IsExpanded)
                    {
                        // recursive
                        value.Parse();
                    }
                    token = value.ToString();
                }
                else if (Graph.Instance.Macros.Dict.ContainsKey(token))
                {
                    var value = Graph.Instance.Macros.Dict[token];
                    if (!value.IsExpanded)
                    {
                        // recursive
                        value.Parse();
                    }
                    token = value.ToString();
                }
                else if (this.ModuleWithMacros != null && this.ModuleWithMacros.Macros.Dict.ContainsKey(token))
                {
                    var value = this.ModuleWithMacros.Macros.Dict[token];
                    if (!value.IsExpanded)
                    {
                        // recursive
                        value.Parse();
                    }
                    token = value.ToString();
                }
                else if (this.ModuleWithMacros != null && null != this.ModuleWithMacros.Tool && this.ModuleWithMacros.Tool.Macros.Dict.ContainsKey(token))
                {
                    var value = this.ModuleWithMacros.Tool.Macros.Dict[token];
                    if (!value.IsExpanded)
                    {
                        // recursive
                        value.Parse();
                    }
                    token = value.ToString();
                }
                else
                {
                    // TODO: this could be due to the user not having set a property, e.g. inputpath
                    // is there a better error message that could be returned, other than this in those
                    // circumstances?
                    throw new System.Exception(System.String.Format("Unrecognized token '{0}", token));
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
                throw new System.Exception(message.ToString());
            }
            var joined = this.EvaluateFunctions(tokens);
            if (null == customMacros)
            {
                this.ParsedString = joined;
            }
            Core.Log.DebugMessage("Converted '{0}' to '{1}'", this.OriginalString, this.ToString());
            return joined;
        }

        private string
        EvaluateFunctions(
            System.Collections.Generic.List<string> tokens)
        {
            var joined = JoinTokens(tokens);
            var tokenized = SplitToParse(joined, FunctionRegExPattern);
            var matchCount = tokenized.Count();
            if (1 == matchCount)
            {
                return joined;
            }
            // triplets of matches
            int matchIndex = 0;
            while (matchIndex < matchCount)
            {
                var index = matchIndex++;
                var expr = tokenized.ElementAt(index);
                if (!expr.StartsWith(FunctionPrefix))
                {
                    continue;
                }

                var functionName = tokenized.ElementAt(matchIndex++);
                var argument = tokenized.ElementAt(matchIndex++);
                var result = this.FunctionExpression(functionName, argument);
                joined = joined.Replace(expr, result);
            }
            return joined;
        }

        private string FunctionExpression(string functionName, string argument)
        {
            switch (functionName)
            {
                case "basename":
                    return System.IO.Path.GetFileNameWithoutExtension(argument);

                default:
                    throw new System.Exception("Unknown function");
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

        public override bool Equals(object obj)
        {
            var other = obj as TokenizedString;
            if (this.OriginalString != other.OriginalString)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Container of key-values pairs representing macro replacement in strings (usually paths)
    /// </summary>
    public sealed class MacroList
    {
        public MacroList()
        {
            this.DictInternal = new System.Collections.Generic.Dictionary<string, TokenizedString>();
        }

        private static string FormattedKey(string key)
        {
            return System.String.Format("{0}{1}{2}", TokenizedString.TokenPrefix, key, TokenizedString.TokenSuffix);
        }

        public TokenizedString this[string key]
        {
            get
            {
                return this.Dict[FormattedKey(key)];
            }
            set
            {
                this.DictInternal[FormattedKey(key)] = value;
            }
        }

        public void Add(string key, TokenizedString value)
        {
            if (key.StartsWith(TokenizedString.TokenPrefix) || key.EndsWith(TokenizedString.TokenSuffix))
            {
                throw new System.Exception(System.String.Format("Invalid macro key: {0}", key));
            }
            if (null == value)
            {
                throw new System.Exception("Macro value cannot be null");
            }
            this.DictInternal[FormattedKey(key)] = value;
        }

        public void Add(string key, string value)
        {
            this.Add(key, TokenizedString.Create(value, null));
        }

        private System.Collections.Generic.Dictionary<string, TokenizedString> DictInternal
        {
            get;
            set;
        }

        public System.Collections.ObjectModel.ReadOnlyDictionary<string, TokenizedString> Dict
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyDictionary<string, TokenizedString>(this.DictInternal);
            }
        }

        public bool Contains(string token)
        {
            return this.Dict.ContainsKey(token);
        }
    }

    /// <summary>
    /// Encapsulation of all things needed to configure a build
    /// </summary>
    sealed public class Environment
    {
        public Environment()
        {
            this.Configuration = EConfiguration.Invalid;
            this.Platform = State.Platform;
        }

        public EConfiguration Configuration
        {
            get;
            set;
        }

        public EPlatform Platform
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Utility functions for dealing with execution policies
    /// </summary>
    public static class ExecutionPolicyUtilities<T> where T: class
    {
        static System.Collections.Generic.Dictionary<string, T> Policies = new System.Collections.Generic.Dictionary<string, T>();

        private static T InternalCreate(string classname)
        {
            var type = System.Type.GetType(classname,
                (typename) =>
                {
                    // TODO: this does not seem to be used
                    return Core.State.ScriptAssembly;
                },
                (assembly, name, checkcase) =>
                {
                    return Core.State.ScriptAssembly.GetType(name);
                });
            if (null == type)
            {
                throw new Bam.Core.Exception("Unable to locate class '{0}'", classname);
            }
            var policy = System.Activator.CreateInstance(type) as T;
            if (null == policy)
            {
                throw new Bam.Core.Exception("Unable to create instance of class '{0}'", classname);
            }
            return policy;
        }

        // there is no where T: interface clause
        public static T Create(string classname)
        {
            if (!Policies.ContainsKey(classname))
            {
                Policies.Add(classname, InternalCreate(classname));
            }
            return Policies[classname];
        }
    }

    public sealed class ToolType
    {
        private static System.Collections.Generic.List<ToolType> List = new System.Collections.Generic.List<ToolType>();

        private ToolType(string id)
        {
            this.Id = id;
        }

        public string Id
        {
            get;
            private set;
        }

        public static ToolType Get(string id)
        {
            var matches = List.Where((item) => { return item.Id == id; });
            if (matches.Count() > 0)
            {
                return matches.ElementAt(0);
            }
            var tooltype = new ToolType(id);
            List.Add(tooltype);
            return tooltype;
        }
    }

    /// <summary>
    /// A tool is a module in the usual sense, so that it can be added into the dependency tree
    /// </summary>
    public abstract class Tool :
        Module
    {
        protected Tool()
            : base()
        {
            this.EnvironmentVariables = new System.Collections.Generic.Dictionary<string, TokenizedStringArray>();
            this.InheritedEnvironmentVariables = new System.Collections.Generic.List<string>();
        }

        // TODO: Might move the Name into the Module?
        public string Name
        {
            get;
            protected set;
        }

        public abstract Settings CreateDefaultSettings<T>(T module) where T : Bam.Core.V2.Module;

        protected override void
        ExecuteInternal(
            ExecutionContext context)
        {
            // by default, a Tool's execution does nothing as it's on disk
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // by default, the execution policy of a tool is to do nothing as it's on disk
        }

        public System.Collections.Generic.Dictionary<string, TokenizedStringArray> EnvironmentVariables
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<string> InheritedEnvironmentVariables
        {
            get;
            private set;
        }

        // TODO: is this on an interface? not all tools will be based on running an executable
        public abstract TokenizedString Executable
        {
            get;
        }

        public override void Evaluate()
        {
            var exists = System.IO.File.Exists(this.Executable.ToString());
            this.IsUpToDate = exists;
        }
    }

    public sealed class TokenizedStringArray :
        Bam.Core.Array<TokenizedString>
    {
        public TokenizedStringArray(TokenizedString input)
            :
            base(new[] { input })
        {
        }
    }

    public class ExecutionContext
    {
        public ExecutionContext()
        {
            this.OutputStringBuilder = new System.Text.StringBuilder();
            this.ErrorStringBuilder = new System.Text.StringBuilder();
        }

        public System.Text.StringBuilder OutputStringBuilder
        {
            get;
            private set;
        }

        public System.Text.StringBuilder ErrorStringBuilder
        {
            get;
            private set;
        }

        public void
        OutputDataReceived(
            object sender,
            System.Diagnostics.DataReceivedEventArgs e)
        {
            if (System.String.IsNullOrEmpty(e.Data))
            {
                return;
            }
            //System.Diagnostics.Process process = sender as System.Diagnostics.Process;
            this.OutputStringBuilder.Append(e.Data + '\n');
        }

        public void
        ErrorDataReceived(
            object sender,
            System.Diagnostics.DataReceivedEventArgs e)
        {
            if (System.String.IsNullOrEmpty(e.Data))
            {
                return;
            }
            //System.Diagnostics.Process process = sender as System.Diagnostics.Process;
            this.ErrorStringBuilder.Append(e.Data + '\n');
        }
    }
}

    public static class State
    {
        public static readonly LocationKey BuildRootLocationKey = new LocationKey("BuildRoot", ScaffoldLocation.ETypeHint.Directory);
        public static readonly LocationKey ModuleBuildDirLocationKey = new LocationKey("ModuleBuildDirectory", ScaffoldLocation.ETypeHint.Directory);

        public class Category :
            System.Collections.Generic.Dictionary<string, object>
        {}

        private static System.Collections.Generic.Dictionary<string, Category> s = new System.Collections.Generic.Dictionary<string, Category>();

        private static void
        GetAssemblyVersionData(
            out System.Version assemblyVersion,
            out string productVersion)
        {
            var coreAssembly = System.Reflection.Assembly.GetAssembly(typeof(State));
            assemblyVersion = coreAssembly.GetName().Version;
            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(coreAssembly.Location);
            var pv = versionInfo.ProductVersion.Trim();
            if (string.IsNullOrEmpty(pv))
            {
                // some Mono implementations only gather the product major/minor/build strings from the assembly
                productVersion = System.String.Format("{0}.{1}", versionInfo.ProductMajorPart, versionInfo.ProductMinorPart);
            }
            else
            {
                productVersion = pv;
            }
        }

        static
        State()
        {
            ReadOnly = false;

            System.Version assemblyVersion;
            string productVersion;
            GetAssemblyVersionData(out assemblyVersion, out productVersion);

            AddCategory("BuildAMation");
            var bamAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var assemblyDirectory = System.IO.Path.GetDirectoryName(bamAssembly.Location);
            Add<string>("BuildAMation", "Directory", assemblyDirectory);
            Add<System.Version>("BuildAMation", "Version", assemblyVersion);
            Add<string>("BuildAMation", "VersionString", productVersion);
            Add<bool>("BuildAMation", "RunningMono", System.Type.GetType("Mono.Runtime") != null);

            // TODO: commented out as the TargetFrameworkAttribute was only introduced in CLR 4
#if false
            var targetFramework = bamAssembly.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false);
            var targetFrameworkName = (targetFramework[0] as System.Runtime.Versioning.TargetFrameworkAttribute).FrameworkName;
            Add<string>("BuildAMation", "TargetFramework", targetFrameworkName);
            var targetFrameworkNameSplit = targetFrameworkName.Split('=');
            Add<string>("BuildAMation", "CSharpCompilerVersion", targetFrameworkNameSplit[1]);
#endif

            var schemaDirectory = System.IO.Path.Combine(State.ExecutableDirectory, "Schema");
            {
                var schemaPath = System.IO.Path.Combine(schemaDirectory, "OpusPackageDependency.xsd");
                if (!System.IO.File.Exists(schemaPath))
                {
                    // TODO: this is quite dangerous, as the exception will try to log a message
                    // but the logger depends on the State object, so an unhandled exception occurs
                    // might be better to just exit here
                    throw new Exception("Schema '{0}' does not exist. Expected it to be in '{1}'", schemaPath, schemaDirectory);
                }
                Add<string>("BuildAMation", "PackageDependencySchemaPathName", schemaPath);
            }
            {
                var schemaV2Path = System.IO.Path.Combine(schemaDirectory, "OpusPackageDependencyV2.xsd");
                if (!System.IO.File.Exists(schemaV2Path))
                {
                    // TODO: this is quite dangerous, as the exception will try to log a message
                    // but the logger depends on the State object, so an unhandled exception occurs
                    // might be better to just exit here
                    throw new Exception("Schema '{0}' does not exist. Expected it to be in '{1}'", schemaV2Path, schemaDirectory);
                }
                Add<string>("BuildAMation", "PackageDependencySchemaPathNameV2", schemaV2Path);

                // relative path for definition files
                Add<string>("BuildAMation", "PackageDependencySchemaRelativePathNameV2", "./Schema/OpusPackageDependencyV2.xsd");
            }
            {
                // relative path for definition files
                Add<string>("BuildAMation", "PackageDefinitionSchemaRelativePathNameV3", "./Schema/BamPackageDefinitionV1.xsd");
            }

            AddCategory("System");
            OSUtilities.SetupPlatform();
            Add<TimeProfile[]>("System", "Profiling", new TimeProfile[System.Enum.GetValues(typeof(ETimingProfiles)).Length]);
            Add<EVerboseLevel>("System", "Verbosity", EVerboseLevel.Info);
            Add<string>("System", "WorkingDirectory", System.IO.Directory.GetCurrentDirectory());
            Add<bool>("System", "Pedantic", false);

            var primaryPackageRoot = System.IO.Path.Combine(System.IO.Directory.GetParent(System.IO.Directory.GetParent(assemblyDirectory).FullName).FullName, "packages");
            var packageRoots = new Array<DirectoryLocation>();
            packageRoots.Add(DirectoryLocation.Get(primaryPackageRoot));
            Add<Array<DirectoryLocation>>("System", "PackageRoots", packageRoots);

            var packageInfoCollection = new PackageInformationCollection();
            Add<PackageInformationCollection>("System", "Packages", packageInfoCollection);

            var dependentPackageList = new UniqueList<PackageIdentifier>();
            Add<UniqueList<PackageIdentifier>>("System", "DependentPackageList", dependentPackageList);

            Add<string>("System", "ScriptAssemblyPathname", null);
            Add<System.Reflection.Assembly>("System", "ScriptAssembly", null);
            Add<string>("System", "BuilderName", null);
            Add<string>("System", "BuildRoot", null);
            Add<DirectoryLocation>("System", "BuildRootLocation", null);
            Add<DependencyGraph>("System", "Graph", null);
            Add<BuildManager>("System", "BuildManager", null);
            Add<System.Threading.ManualResetEvent>("System", "BuildStartedEvent", new System.Threading.ManualResetEvent(false));
            Add<bool>("System", "ShowTimingStatistics", false);
            Add<StringArray>("System", "CompilerDefines", new StringArray());
            Add<StringArray>("System", "CompilerUndefines", new StringArray());
            Add<bool>("System", "CacheAssembly", true);
            Add<string>("System", "SchedulerType", "Bam.Core.DefaultScheduler");
            Add<Array<BuildSchedulerProgressUpdatedDelegate>>("System", "SchedulerProgressDelegates", new Array<BuildSchedulerProgressUpdatedDelegate>());

            AddCategory("PackageCreation");
            Add<StringArray>("PackageCreation", "DependentPackages", null);
            Add<StringArray>("PackageCreation", "Builders", null);

            AddCategory("Build");
            Add<IBuilder>("Build", "BuilderInstance", null);
            Add<PackageInformation>("Build", "BuilderPackage", null);
            Add<bool>("Build", "IncludeDebugSymbols", false);
            Add("Build", "JobCount", 1);
            Add<System.Collections.Generic.Dictionary<string, string>>("Build", "LazyArguments", new System.Collections.Generic.Dictionary<string, string>());
            Add<System.Collections.Generic.List<IAction>>("Build", "InvokedActions", new System.Collections.Generic.List<IAction>());
            Add<StringArray>("Build", "Platforms", null);
            Add<Array<EConfiguration>>("Build", "Configurations", null);
            Add<StringArray>("Build", "Modules", null);
            Add<bool>("Build", "ForceDefinitionFileUpdate", false);

            AddCategory("Toolset");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void
        Print()
        {
            foreach (var category in s)
            {
                Log.DebugMessage("Category '{0}'", category.Key);
                foreach (var item in category.Value)
                {
                    Log.DebugMessage("\t'{0}' = '{1}'", item.Key, (null != item.Value) ? item.Value.ToString() : "null");
                }
            }
        }

        public static bool ReadOnly
        {
            get;
            set;
        }

        public static void
        AddCategory(
            string category)
        {
            if (ReadOnly)
            {
                throw new Exception("State is marked readonly");
            }
            s.Add(category, new Category());
        }

        public static bool
        HasCategory(
            string category)
        {
            var hasCategory = s.ContainsKey(category);
            return hasCategory;
        }

        // TODO: is this the same as set?
        // Not quite. This will throw an exception if the entry already exists
        public static void
        Add<Type>(
            string category,
            string key,
            Type value)
        {
            if (ReadOnly)
            {
                throw new Exception("State is marked readonly");
            }
            s[category].Add(key, value);
        }

        public static bool
        Has(
            string category,
            string key)
        {
            if (!HasCategory(category))
            {
                return false;
            }

            if (!s[category].ContainsKey(key))
            {
                return false;
            }

            return true;
        }

        // TODO: how can I cast this correctly?
        public static object
        Get(
            string category,
            string key)
        {
            object value = null;
            try
            {
                value = s[category][key];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new Exception("Category '{0}' with key '{1}' not found", category, key);
            }
            return value;
        }

        public static T
        Get<T>(
            string category,
            string key,
            T defaultValue) where T:struct
        {
            object value = null;
            try
            {
                value = s[category][key];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return defaultValue;
            }
            return (T)value;
        }

        public static void
        Set(
            string category,
            string key,
            object value)
        {
            if (ReadOnly)
            {
                throw new Exception("State is marked readonly");
            }
            s[category][key] = value;
        }

        public static string ExecutableDirectory
        {
            get
            {
                return Get("BuildAMation", "Directory") as string;
            }
        }

        public static System.Version Version
        {
            get
            {
                return Get("BuildAMation", "Version") as System.Version;
            }
        }

        public static string VersionString
        {
            get
            {
                return Get("BuildAMation", "VersionString") as string;
            }
        }

        public static bool RunningMono
        {
            get
            {
                return (bool)Get("BuildAMation", "RunningMono");
            }
        }

#if false
        public static string TargetFramework
        {
            get
            {
                return Get("BuildAMation", "TargetFramework") as string;
            }
        }

        public static string CSharpCompilerVersion
        {
            get
            {
                return Get("BuildAMation", "CSharpCompilerVersion") as string;
            }
        }
#endif

        public static string PackageDefinitionSchemaPath
        {
            get
            {
                return Get("BuildAMation", "PackageDependencySchemaPathName") as string;
            }
        }

        public static string PackageDefinitionSchemaPathV2
        {
            get
            {
                return Get("BuildAMation", "PackageDependencySchemaPathNameV2") as string;
            }
        }

        public static string PackageDefinitionSchemaRelativePathNameV2
        {
            get
            {
                return Get("BuildAMation", "PackageDependencySchemaRelativePathNameV2") as string;
            }
        }

        public static string PackageDefinitionSchemaRelativePathNameV3
        {
            get
            {
                return Get("BuildAMation", "PackageDefinitionSchemaRelativePathNameV3") as string;
            }
        }

        public static EPlatform Platform
        {
            get
            {
                return (EPlatform)Get("System", "Platform");
            }
        }

        public static bool IsLittleEndian
        {
            get
            {
                return (bool)Get("System", "IsLittleEndian");
            }
        }

        public static string WorkingDirectory
        {
            get
            {
                return Get("System", "WorkingDirectory") as string;
            }
        }

        public static bool Pedantic
        {
            set
            {
                Set("System", "Pedantic", value);
            }
            get
            {
                return (bool)Get("System", "Pedantic");
            }
        }

        public static Array<DirectoryLocation> PackageRoots
        {
            set
            {
                Set("System", "PackageRoots", value);
            }
            get
            {
                return Get("System", "PackageRoots") as Array<DirectoryLocation>;
            }
        }

        public static PackageInformationCollection PackageInfo
        {
           set
           {
               Set("System", "Packages", value);
           }
           get
           {
               return Get("System", "Packages") as PackageInformationCollection;
           }
        }

        public static UniqueList<PackageIdentifier> DependentPackageList
        {
            set
            {
                Set("System", "DependentPackageList", value);
            }
            get
            {
                return Get("System", "DependentPackageList") as UniqueList<PackageIdentifier>;
            }
        }

        public static string ScriptAssemblyPathname
        {
            set
            {
                Set("System", "ScriptAssemblyPathname", value);
            }
            get
            {
                return Get("System", "ScriptAssemblyPathname") as string;
            }
        }

        public static System.Reflection.Assembly ScriptAssembly
        {
            set
            {
                Set("System", "ScriptAssembly", value);
            }
            get
            {
                return Get("System", "ScriptAssembly") as System.Reflection.Assembly;
            }
        }

        public static string BuilderName
        {
            set
            {
                Set("System", "BuilderName", value);
            }
            get
            {
                return Get("System", "BuilderName") as string;
            }
        }

        public static string BuildRoot
        {
            set
            {
                var absoluteBuildRootPath = RelativePathUtilities.MakeRelativePathAbsoluteToWorkingDir(value);

                Set("System", "BuildRoot", absoluteBuildRootPath);
                Set("System", "BuildRootLocation", DirectoryLocation.Get(absoluteBuildRootPath, Location.EExists.WillExist));
            }
            get
            {
                return Get("System", "BuildRoot") as string;
            }
        }

        public static DirectoryLocation BuildRootLocation
        {
            get
            {
                return Get("System", "BuildRootLocation") as DirectoryLocation;
            }
        }

        public static StringArray PackageCreationDependents
        {
            set
            {
                Set("PackageCreation", "DependentPackages", value);
            }
            get
            {
                return Get("PackageCreation", "DependentPackages") as StringArray;
            }
        }

        public static IBuilder BuilderInstance
        {
            set
            {
                Set("Build", "BuilderInstance", value);
            }

            get
            {
                return Get("Build", "BuilderInstance") as IBuilder;
            }
        }

        public static PackageInformation BuilderPackage
        {
            set
            {
                Set("Build", "BuilderPackage", value);
            }

            get
            {
                return Get("Build", "BuilderPackage") as PackageInformation;
            }
        }

        public static int JobCount
        {
            set
            {
                Set("Build", "JobCount", value);
            }

            get
            {
                return (int)Get("Build", "JobCount");
            }
        }

        public static bool CompileWithDebugSymbols
        {
            set
            {
                Set("Build", "IncludeDebugSymbols", value);
            }

            get
            {
                return (bool)Get("Build", "IncludeDebugSymbols");
            }
        }

        public static TimeProfile[] TimingProfiles
        {
            get
            {
                return Get("System", "Profiling") as TimeProfile[];
            }
        }

        public static EVerboseLevel VerbosityLevel
        {
            set
            {
                Set("System", "Verbosity", value);
            }

            get
            {
                return (EVerboseLevel)Get("System", "Verbosity");
            }
        }

        public static System.Collections.Generic.Dictionary<string,string> LazyArguments
        {
            set
            {
                Set("Build", "LazyArguments", value);
            }

            get
            {
                return Get("Build", "LazyArguments") as System.Collections.Generic.Dictionary<string, string>;
            }
        }

        public static System.Collections.Generic.List<IAction> InvokedActions
        {
            set
            {
                Set("Build", "InvokedActions", value);
            }

            get
            {
                return Get("Build", "InvokedActions") as System.Collections.Generic.List<IAction>;
            }
        }

        public static Array<EPlatform> BuildPlatforms
        {
            set
            {
                Set("Build", "Platforms", value);
            }

            get
            {
                return Get("Build", "Platforms") as Array<EPlatform>;
            }
        }

        public static Array<EConfiguration> BuildConfigurations
        {
            set
            {
                Set("Build", "Configurations", value);
            }

            get
            {
                return Get("Build", "Configurations") as Array<EConfiguration>;
            }
        }

        public static StringArray BuildModules
        {
            set
            {
                Set("Build", "Modules", value);
            }

            get
            {
                return Get("Build", "Modules") as StringArray;
            }
        }

        public static BuildManager BuildManager
        {
            set
            {
                Set("System", "BuildManager", value);
            }

            get
            {
                return Get("System", "BuildManager") as BuildManager;
            }
        }

        public static System.Threading.ManualResetEvent BuildStartedEvent
        {
            set
            {
                Set("System", "BuildStartedEvent", value);
            }

            get
            {
                return Get("System", "BuildStartedEvent") as System.Threading.ManualResetEvent;
            }
        }

        public static bool ShowTimingStatistics
        {
            set
            {
                Set("System", "ShowTimingStatistics", value);
            }

            get
            {
                return (bool)Get("System", "ShowTimingStatistics");
            }
        }

        public static StringArray PackageCompilationDefines
        {
            set
            {
                Set("System", "CompilerDefines", value);
            }

            get
            {
                return Get("System", "CompilerDefines") as StringArray;
            }
        }

        public static StringArray PackageCompilationUndefines
        {
            set
            {
                Set("System", "CompilerUndefines", value);
            }

            get
            {
                return Get("System", "CompilerUndefines") as StringArray;
            }
        }

        public static bool CacheAssembly
        {
            set
            {
                Set("System", "CacheAssembly", value);
            }

            get
            {
                return (bool)Get("System", "CacheAssembly");
            }
        }

        public static string SchedulerType
        {
            set
            {
                Set("System", "SchedulerType", value);
            }

            get
            {
                return Get("System", "SchedulerType") as string;
            }
        }

        public static Array<BuildSchedulerProgressUpdatedDelegate> SchedulerProgressUpdates
        {
            set
            {
                Set("System", "SchedulerProgressDelegates", value);
            }

            get
            {
                return Get("System", "SchedulerProgressDelegates") as Array<BuildSchedulerProgressUpdatedDelegate>;
            }
        }

        public static bool ForceDefinitionFileUpdate
        {
            set
            {
                Set("Build", "ForceDefinitionFileUpdate", value);
            }

            get
            {
                return (bool)Get("Build", "ForceDefinitionFileUpdate");
            }
        }
    }
}

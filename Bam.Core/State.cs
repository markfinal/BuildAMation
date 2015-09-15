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
namespace Bam.Core
{
    using System.Linq;

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
                    return State.ScriptAssembly;
                },
                (assembly, name, checkcase) =>
                {
                    return State.ScriptAssembly.GetType(name);
                });
            if (null == type)
            {
                throw new Exception("Unable to locate class '{0}'", classname);
            }
            var policy = System.Activator.CreateInstance(type) as T;
            if (null == policy)
            {
                throw new Exception("Unable to create instance of class '{0}'", classname);
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

    public interface ITool
    {
        Settings
        CreateDefaultSettings<T>(
            T module) where T : Module;
    }

    public interface ICommandLineTool :
        ITool
    {
        System.Collections.Generic.Dictionary<string, TokenizedStringArray> EnvironmentVariables
        {
            get;
        }

        System.Collections.Generic.List<string> InheritedEnvironmentVariables
        {
            get;
        }

        TokenizedString Executable
        {
            get;
        }
    }

    /// <summary>
    /// A tool is a module in the usual sense, so that it can be added into the dependency tree
    /// </summary>
    public abstract class PreBuiltTool :
        Module,
        ICommandLineTool
    {
        protected PreBuiltTool()
            : base()
        {
            this.EnvironmentVariables = new System.Collections.Generic.Dictionary<string, TokenizedStringArray>();
            this.InheritedEnvironmentVariables = new System.Collections.Generic.List<string>();
        }

            #if false
        // TODO: Might move the Name into the Module?
        public string Name
        {
            get;
            protected set;
        }
            #endif

        public abstract Settings CreateDefaultSettings<T>(T module) where T : Module;

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

        protected override void
        ExecuteInternal(
            ExecutionContext context)
        {
            // by default, a PreBuiltTool's execution does nothing as it's on disk
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // by default, the execution policy of a PreBuiltTool is to do nothing as it's on disk
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var exists = System.IO.File.Exists(this.Executable.ToString());
            if (!exists)
            {
                this.ReasonToExecute = ExecuteReasoning.FileDoesNotExist(this.Executable);
            }
        }
    }

    public sealed class TokenizedStringArray :
        Array<TokenizedString>
    {
        public TokenizedStringArray(TokenizedString input)
            :
            base(new[] { input })
        {
        }
    }

    public class ExecutionContext
    {
        public ExecutionContext(
            bool useEvaluation,
            bool explainRebuild)
        {
            this.Evaluate = useEvaluation;
            this.ExplainLoggingLevel = explainRebuild ? State.VerbosityLevel : EVerboseLevel.Full;
            this.OutputStringBuilder = new System.Text.StringBuilder();
            this.ErrorStringBuilder = new System.Text.StringBuilder();
        }

        public bool Evaluate
        {
            get;
            private set;
        }

        public EVerboseLevel ExplainLoggingLevel
        {
            get;
            private set;
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

    public static class State
    {
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

        private static string
        GetBamDirectory()
        {
            var bamAssembly = System.Reflection.Assembly.GetEntryAssembly();
            var rm = new System.Resources.ResourceManager(System.String.Format("{0}.PackageInfoResources", bamAssembly.GetName().Name), bamAssembly);
            // TODO: would be nice to check in advance if any exist
            try
            {
                return rm.GetString("BamInstallDir");
            }
            catch (System.Resources.MissingManifestResourceException)
            {
                // this assumes running an executable from the BAM! installation folder
                return System.IO.Path.GetDirectoryName(bamAssembly.Location);
            }
        }

        private static string
        GetWorkingDirectory()
        {
            var bamAssembly = System.Reflection.Assembly.GetEntryAssembly();
            var rm = new System.Resources.ResourceManager(System.String.Format("{0}.PackageInfoResources", bamAssembly.GetName().Name), bamAssembly);
            // TODO: would be nice to check in advance if any exist
            try
            {
                return rm.GetString("WorkingDir");
            }
            catch (System.Resources.MissingManifestResourceException)
            {
                return System.IO.Directory.GetCurrentDirectory();
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
            Add<bool>("BuildAMation", "RunningMono", System.Type.GetType("Mono.Runtime") != null);

            string assemblyDirectory = GetBamDirectory();
            Add<string>("BuildAMation", "Directory", assemblyDirectory);

            Add<System.Version>("BuildAMation", "Version", assemblyVersion);
            Add<string>("BuildAMation", "VersionString", productVersion);

            // TODO: commented out as the TargetFrameworkAttribute was only introduced in CLR 4
#if false
            var targetFramework = bamAssembly.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false);
            var targetFrameworkName = (targetFramework[0] as System.Runtime.Versioning.TargetFrameworkAttribute).FrameworkName;
            Add<string>("BuildAMation", "TargetFramework", targetFrameworkName);
            var targetFrameworkNameSplit = targetFrameworkName.Split('=');
            Add<string>("BuildAMation", "CSharpCompilerVersion", targetFrameworkNameSplit[1]);
#endif

            Add<string>("BuildAMation", "PackageDefinitionSchemaRelativePath", "./Schema/BamPackageDefinitionV1.xsd");

            AddCategory("System");
            OSUtilities.SetupPlatform();
            Add<TimeProfile[]>("System", "Profiling", new TimeProfile[System.Enum.GetValues(typeof(ETimingProfiles)).Length]);
            Add<EVerboseLevel>("System", "Verbosity", EVerboseLevel.Info);

            Add<string>("System", "WorkingDirectory", GetWorkingDirectory());
            Add<bool>("System", "Pedantic", false);

            var primaryPackageRepo = System.IO.Path.Combine(System.IO.Directory.GetParent(System.IO.Directory.GetParent(assemblyDirectory).FullName).FullName, "packages");
            var packageRepos = new StringArray();
            packageRepos.Add(primaryPackageRepo);
            Add<StringArray>("System", "PackageRepositories", packageRepos);

            Add<string>("System", "ScriptAssemblyPathname", null);
            Add<System.Reflection.Assembly>("System", "ScriptAssembly", null);
            Add<string>("System", "BuilderName", null);
            Add<string>("System", "BuildRoot", null);
            Add<System.Threading.ManualResetEvent>("System", "BuildStartedEvent", new System.Threading.ManualResetEvent(false));
            Add<bool>("System", "ShowTimingStatistics", false);
            Add<StringArray>("System", "CompilerDefines", new StringArray());
            Add<StringArray>("System", "CompilerUndefines", new StringArray());
            Add<bool>("System", "CacheAssembly", true);
            Add<string>("System", "SchedulerType", "Bam.Core.DefaultScheduler");

            AddCategory("PackageCreation");
            Add<StringArray>("PackageCreation", "DependentPackages", null);
            Add<StringArray>("PackageCreation", "Builders", null);

            AddCategory("Build");
#if true
#else
            Add<PackageInformation>("Build", "BuilderPackage", null);
#endif
            Add<bool>("Build", "IncludeDebugSymbols", false);
            Add("Build", "JobCount", 1);
            Add<System.Collections.Generic.Dictionary<string, string>>("Build", "LazyArguments", new System.Collections.Generic.Dictionary<string, string>());
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

        public static string PackageDefinitionSchemaRelativePath
        {
            get
            {
                return Get("BuildAMation", "PackageDefinitionSchemaRelativePath") as string;
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

        public static StringArray PackageRepositories
        {
            set
            {
                Set("System", "PackageRepositories", value);
            }
            get
            {
                return Get("System", "PackageRepositories") as StringArray;
            }
        }

#if true
#else
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
#endif

#if true
#else
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
#endif

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

        public static string BuildMode
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
            }
            get
            {
                return Get("System", "BuildRoot") as string;
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

#if true
#else
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
#endif

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

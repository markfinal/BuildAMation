// <copyright file="State.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus state.</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class State
    {
        public static readonly LocationKey BuildRootLocationKey = new LocationKey("BuildRoot", ScaffoldLocation.ETypeHint.Directory);
        public static readonly LocationKey ModuleBuildDirLocationKey = new LocationKey("ModuleBuildDirectory", ScaffoldLocation.ETypeHint.Directory);

        public class Category :
            System.Collections.Generic.Dictionary<string, object>
        {}

        private static System.Collections.Generic.Dictionary<string, Category> s = new System.Collections.Generic.Dictionary<string, Category>();

        private static void
        GetOpusVersionData(
            out System.Version assemblyVersion,
            out string productVersion)
        {
            var coreAssembly = System.Reflection.Assembly.GetAssembly(typeof(Opus.Core.State));
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
            GetOpusVersionData(out assemblyVersion, out productVersion);

            AddCategory("Opus");
            var opusDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Add<string>("Opus", "Directory", opusDirectory);
            Add<System.Version>("Opus", "Version", assemblyVersion);
            Add<string>("Opus", "VersionString", productVersion);
            Add<bool>("Opus", "RunningMono", System.Type.GetType("Mono.Runtime") != null);

            var opusSchemaDirectory = System.IO.Path.Combine(State.OpusDirectory, "Schema");
            {
                var opusSchemaPathname = System.IO.Path.Combine(opusSchemaDirectory, "OpusPackageDependency.xsd");
                if (!System.IO.File.Exists(opusSchemaPathname))
                {
                    throw new Exception("Schema '{0}' does not exist. Expected it to be in '{1}'", opusSchemaPathname, opusSchemaDirectory);
                }
                Add<string>("Opus", "PackageDependencySchemaPathName", opusSchemaPathname);
            }
            {
                var v2SchemaPathName = System.IO.Path.Combine(opusSchemaDirectory, "OpusPackageDependencyV2.xsd");
                if (!System.IO.File.Exists(v2SchemaPathName))
                {
                    throw new Exception("Schema '{0}' does not exist. Expected it to be in '{1}'", v2SchemaPathName, opusSchemaDirectory);
                }
                Add<string>("Opus", "PackageDependencySchemaPathNameV2", v2SchemaPathName);

                // relative path for definition files
                Add<string>("Opus", "PackageDependencySchemaRelativePathNameV2", "./Schema/OpusPackageDependencyV2.xsd");
            }

            AddCategory("System");
            OSUtilities.SetupPlatform();
            Add<TimeProfile[]>("System", "Profiling", new TimeProfile[System.Enum.GetValues(typeof(ETimingProfiles)).Length]);
            Add<EVerboseLevel>("System", "Verbosity", EVerboseLevel.Info);
            Add<string>("System", "WorkingDirectory", System.IO.Directory.GetCurrentDirectory());

            var opusPackageRoot = System.IO.Path.Combine(System.IO.Directory.GetParent(System.IO.Directory.GetParent(opusDirectory).FullName).FullName, "packages");
            var packageRoots = new Array<DirectoryLocation>();
            packageRoots.Add(DirectoryLocation.Get(opusPackageRoot));
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
            Add<string>("System", "SchedulerType", "Opus.Core.DefaultScheduler");
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

        public static string OpusDirectory
        {
            get
            {
                return Get("Opus", "Directory") as string;
            }
        }

        public static System.Version OpusVersion
        {
            get
            {
                return Get("Opus", "Version") as System.Version;
            }
        }

        public static string OpusVersionString
        {
            get
            {
                return Get("Opus", "VersionString") as string;
            }
        }

        public static bool RunningMono
        {
            get
            {
                return (bool)Get("Opus", "RunningMono");
            }
        }

        public static string OpusPackageDependencySchemaPathName
        {
            get
            {
                return Get("Opus", "PackageDependencySchemaPathName") as string;
            }
        }

        public static string OpusPackageDependencySchemaPathNameV2
        {
            get
            {
                return Get("Opus", "PackageDependencySchemaPathNameV2") as string;
            }
        }

        public static string OpusPackageDependencySchemaRelativePathNameV2
        {
            get
            {
                return Get("Opus", "PackageDependencySchemaRelativePathNameV2") as string;
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

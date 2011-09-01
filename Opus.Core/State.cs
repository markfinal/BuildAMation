// <copyright file="State.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus state.</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class State
    {
        public class Category : System.Collections.Generic.Dictionary<string, object>
        {
        }

        private static System.Collections.Generic.Dictionary<string, Category> s = new System.Collections.Generic.Dictionary<string, Category>();
                
        static State()
        {
            ReadOnly = false;

            System.Reflection.Assembly coreAssembly = System.Reflection.Assembly.GetAssembly(typeof(Opus.Core.State));
            System.Version version = coreAssembly.GetName().Version;

            AddCategory("Opus");
            string opusDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Add<string>("Opus", "Directory", opusDirectory);
            Add<System.Version>("Opus", "Version", version);
            Add<string>("Opus", "VersionString", System.String.Format("{0}.{1}", version.Major, version.Minor));
            Add<bool>("Opus", "RunningMono", System.Type.GetType("Mono.Runtime") != null);

            string opusSchemaDirectory = System.IO.Path.Combine(State.OpusDirectory, "Schema");
            {
                string opusSchemaPathname = System.IO.Path.Combine(opusSchemaDirectory, "OpusPackageDependency.xsd");
                if (!System.IO.File.Exists(opusSchemaPathname))
                {
                    throw new Exception(System.String.Format("Schema '{0}' does not exist. Expected it to be in '{1}'", opusSchemaPathname, opusSchemaDirectory), false);
                }
                Add<string>("Opus", "PackageDependencySchemaPathName", opusSchemaPathname);
            }
            {
                string v2SchemaPathName = System.IO.Path.Combine(opusSchemaDirectory, "OpusPackageDependencyV2.xsd");
                if (!System.IO.File.Exists(v2SchemaPathName))
                {
                    throw new Exception(System.String.Format("Schema '{0}' does not exist. Expected it to be in '{1}'", v2SchemaPathName, opusSchemaDirectory), false);
                }
                Add<string>("Opus", "PackageDependencySchemaPathNameV2", v2SchemaPathName);
            }

            AddCategory("System");
            OSUtilities.SetupPlatform();
            Add<TimeProfile[]>("System", "Profiling", new TimeProfile[System.Enum.GetValues(typeof(ETimingProfiles)).Length]);
            Add<EVerboseLevel>("System", "Verbosity", EVerboseLevel.Info);
            Add<string>("System", "WorkingDirectory", System.IO.Directory.GetCurrentDirectory());

            string opusPackageRoot = System.IO.Path.Combine(System.IO.Directory.GetParent(System.IO.Directory.GetParent(opusDirectory).FullName).FullName, "packages");
            StringArray packageRoots = new StringArray();
            packageRoots.Add(opusPackageRoot);
            Add<StringArray>("System", "PackageRoots", packageRoots);

            PackageInformationCollection packageInfoCollection = new PackageInformationCollection();
            Add<PackageInformationCollection>("System", "Packages", packageInfoCollection);

            Array<PackageIdentifier> dependentPackageList = new Array<PackageIdentifier>();
            Add<Array<PackageIdentifier>>("System", "DependentPackageList", dependentPackageList);

            Add<string>("System", "ScriptAssemblyPathname", null);
            Add<System.Reflection.Assembly>("System", "ScriptAssembly", null);
            Add<string>("System", "BuilderName", null);
            Add<string>("System", "BuildRoot", null);
            Add<DependencyGraph>("System", "Graph", null);
            Add<bool>("System", "ShowTimingStatistics", false);

            AddCategory("PackageCreation");
            Add<StringArray>("PackageCreation", "DependentPackages", null);
            Add<StringArray>("PackageCreation", "Builders", null);

            AddCategory("Build");
            Add<IBuilder>("Build", "BuilderInstance", null);
            Add<PackageInformation>("Build", "BuilderPackage", null);
            Add<TargetCollection>("Build", "Targets", new TargetCollection());
            Add<bool>("Build", "IncludeDebugSymbols", false);
            Add("Build", "JobCount", 1);
            Add<System.Collections.Generic.Dictionary<string, string>>("Build", "LazyArguments", new System.Collections.Generic.Dictionary<string, string>());
            Add<StringArray>("Build", "Platforms", null);
            Add<Array<EConfiguration>>("Build", "Configurations", null);
            Add<StringArray>("Build", "Modules", null);
        }
        
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Print()
        {
            foreach (System.Collections.Generic.KeyValuePair<string, Category> category in s)
            {
                Log.DebugMessage("Category '{0}'", category.Key);
                foreach (System.Collections.Generic.KeyValuePair<string, object> item in category.Value)
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
        
        public static void AddCategory(string category)
        {
            if (ReadOnly)
            {
                throw new Exception("State is marked readonly");
            }
            s.Add(category, new Category());
        }
        
        public static bool HasCategory(string category)
        {
            bool hasCategory = s.ContainsKey(category);
            return hasCategory;
        }

        // TODO: is this the same as set?
        public static void Add<Type>(string category, string key, Type value)
        {
            if (ReadOnly)
            {
                throw new Exception("State is marked readonly");
            }
            s[category].Add(key, value);
        }

        public static bool Has(string category, string key)
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
        public static object Get(string category, string key)
        {
            object value = null;
            try
            {
                value = s[category][key];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new Exception(System.String.Format("Category '{0}' with key '{1}' not found", category, key));
            }
            return value;
        }
        
        public static void Set(string category, string key, object value)
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

        public static StringArray PackageRoots
        {
            set
            {
                Set("System", "PackageRoots", value);
            }
            get
            {
                return Get("System", "PackageRoots") as StringArray;
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

        public static Array<PackageIdentifier> DependentPackageList
        {
            set
            {
                Set("System", "DependentPackageList", value);
            }
            get
            {
                return Get("System", "DependentPackageList") as Array<PackageIdentifier>;
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
                System.Uri buildRootUri = new System.Uri(value, System.UriKind.RelativeOrAbsolute);
                if (!buildRootUri.IsAbsoluteUri)
                {
                    buildRootUri = new System.Uri(System.IO.Path.Combine(WorkingDirectory, value));
                }

                string buildRootPath = buildRootUri.AbsolutePath;
                buildRootPath = System.Uri.UnescapeDataString(buildRootPath);
                if (OSUtilities.IsWindowsHosting)
                {
                    buildRootPath = buildRootPath.Replace('/', '\\');
                }

                Set("System", "BuildRoot", buildRootPath);
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
        
        public static TargetCollection Targets
        {
            set
            {
                Set("Build", "Targets", value);
            }
            
            get
            {
                return Get("Build", "Targets") as TargetCollection;
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
    }
}
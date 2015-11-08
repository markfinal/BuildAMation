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
            Add<EVerboseLevel>("System", "Verbosity", EVerboseLevel.Info);

            Add<string>("System", "WorkingDirectory", GetWorkingDirectory());

            var primaryPackageRepo = System.IO.Path.Combine(System.IO.Directory.GetParent(System.IO.Directory.GetParent(assemblyDirectory).FullName).FullName, "packages");
            var packageRepos = new StringArray();
            packageRepos.Add(primaryPackageRepo);
            Add<StringArray>("System", "PackageRepositories", packageRepos);

            Add<string>("System", "ScriptAssemblyPathname", null);
            Add<System.Reflection.Assembly>("System", "ScriptAssembly", null);
            Add<string>("System", "BuildMode", null);
            Add<bool>("System", "ShowTimingStatistics", false);
            Add<StringArray>("System", "CompilerDefines", new StringArray());
            Add<StringArray>("System", "CompilerUndefines", new StringArray());
            Add<bool>("System", "CacheAssembly", true);

            AddCategory("Build");
            Add<bool>("Build", "IncludeDebugSymbols", false);
            Add("Build", "JobCount", 1);
            Add<bool>("Build", "ForceDefinitionFileUpdate", false);
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
                Set("System", "BuildMode", value);
            }
            get
            {
                return Get("System", "BuildMode") as string;
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

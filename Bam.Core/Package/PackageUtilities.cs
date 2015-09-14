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
    public static class PackageUtilities
    {
        public static readonly string BamSubFolder = "bam";
        public static readonly string ScriptsSubFolder = "Scripts";

        public static void
        MakePackage()
        {
            var packageDir = Core.State.WorkingDirectory;
            var bamDir = System.IO.Path.Combine(packageDir, BamSubFolder);
            if (System.IO.Directory.Exists(bamDir))
            {
                throw new Exception("A Bam package already exists at {0}", packageDir);
            }

            var packageNameArgument = new PackageName();
            var packageName = CommandLineProcessor.Evaluate(packageNameArgument);
            if (null == packageName)
            {
                throw new Exception("No name was defined. Use {0} on the command line to specify it.", (packageNameArgument as ICommandLineArgument).LongName);
            }

            var packageVersion = CommandLineProcessor.Evaluate(new PackageVersion());
            var definition = new PackageDefinitionFile(bamDir, packageName, packageVersion);

            System.IO.Directory.CreateDirectory(bamDir);
            definition.Write();

            var scriptsDir = System.IO.Path.Combine(bamDir, ScriptsSubFolder);
            System.IO.Directory.CreateDirectory(scriptsDir);

            var initialScriptFile = System.IO.Path.Combine(scriptsDir, packageName) + ".cs";
            using (System.IO.TextWriter writer = new System.IO.StreamWriter(initialScriptFile))
            {
                writer.WriteLine("using Bam.Core;");
                writer.WriteLine("namespace {0}", packageName);
                writer.WriteLine("{");
                writer.WriteLine("    // write modules here ...");
                writer.WriteLine("}");
            }
        }

        public static void
        AddDependentPackage()
        {
            var packageNameArgument = new PackageName();
            var packageName = CommandLineProcessor.Evaluate(packageNameArgument);
            if (null == packageName)
            {
                throw new Exception("No name was defined. Use {0} on the command line to specify it.", (packageNameArgument as ICommandLineArgument).LongName);
            }

            var packageVersion = CommandLineProcessor.Evaluate(new PackageVersion());

            var masterPackage = GetMasterPackage();
            // TODO: no checking if this package exists
            // TODO: is this dependent already present?
            if (null != masterPackage.Dependents.Where(item => item.Item1 == packageName && item.Item2 == packageVersion).FirstOrDefault())
            {
                if (null != packageVersion)
                {
                    throw new Exception("Dependency {0}, version {1}, already exists", packageName, packageVersion);
                }
                else
                {
                    throw new Exception("Dependency {0} already exists", packageName);
                }
            }

            masterPackage.Dependents.Add(new System.Tuple<string, string, bool?>(packageName, packageVersion, null));
            masterPackage.Write();
        }

        public static string VersionDefineForCompiler
        {
            get
            {
                var coreVersion = State.Version;
                var coreVersionDefine = System.String.Format("OPUS_CORE_VERSION_{0}_{1}", coreVersion.Major, coreVersion.Minor);
                return coreVersionDefine;
            }
        }

        public static string HostPlatformDefineForCompiler
        {
            get
            {
                var platform = State.Platform;
                var platformString = Platform.ToString(platform, '\0', "OPUS_HOST_", true);
                return platformString;
            }
        }

        private static void
        GetPackageDetailsFromPath(
            string path,
            out string name,
            out string version)
        {
            var directories = path.Split(new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });
            if (directories.Length < 2)
            {
                throw new Exception("Cannot determine package name and version from the path '{0}'. Expected format is 'root{1}packagename{1}version'", path, System.IO.Path.DirectorySeparatorChar);
            }

            name = directories[directories.Length - 2];
            version = directories[directories.Length - 1];
        }

        public static bool
        IsPackageDirectory(
            string packagePath)
        {
#if true
            var bamDir = System.IO.Path.Combine(packagePath, BamSubFolder);
            if (!System.IO.Directory.Exists(bamDir))
            {
                throw new Exception("Path {0} does not form a BAM! package: missing '{1}' subdirectory", packagePath, BamSubFolder);
            }

            return true;
#else
            string packageName;
            string packageVersion;
            GetPackageDetailsFromPath(path, out packageName, out packageVersion);

            isWellDefined = false;
            if (!System.IO.Directory.Exists(path))
            {
                Log.DebugMessage("Package path '{0}' does not exist", path);
                return null;
            }

            var parentDir = System.IO.Directory.GetParent(path);
            if (null == parentDir)
            {
                Log.DebugMessage("No parent directory");
                return null;
            }
            var parentParentDir = System.IO.Directory.GetParent(parentDir.FullName);
            if (null == parentParentDir)
            {
                Log.DebugMessage("No parent of parent directory");
                return null;
            }
            var packageRoot = parentParentDir.FullName;

            var basePackageFilename = System.IO.Path.Combine(path, packageName);
            var scriptFilename = basePackageFilename + ".cs";
            var xmlFilename = basePackageFilename + ".xml";
            if (System.IO.File.Exists(scriptFilename) &&
                System.IO.File.Exists(xmlFilename))
            {
                Core.Log.DebugMessage("Path '{0}' refers to a valid package; root is '{1}'", path, packageRoot);
                isWellDefined = true;
            }
            else
            {
                Core.Log.DebugMessage("Path '{0}' is not a package, but can be a package directory.", path);
            }

            var packageRootLocation = DirectoryLocation.Get(packageRoot);
            State.PackageRoots.AddUnique(packageRootLocation);

            var id = new PackageIdentifier(packageName, packageVersion);
            return id;
#endif
        }

        public static string
        GetPackageDefinitionPathname(
            string packagePath)
        {
            var bamDir = System.IO.Path.Combine(packagePath, BamSubFolder);
            var xmlFiles = System.IO.Directory.GetFiles(bamDir, "*.xml", System.IO.SearchOption.AllDirectories);
            if (0 == xmlFiles.Length)
            {
                throw new Exception("No package definition .xml files found under {0}", bamDir);
            }
            if (xmlFiles.Length > 1)
            {
                var message = new System.Text.StringBuilder();
                message.AppendFormat("Too many .xml files found under {0}", bamDir);
                message.AppendLine();
                foreach (var file in xmlFiles)
                {
                    message.AppendFormat("\t{0}", file);
                    message.AppendLine();
                }
                throw new Exception(message.ToString());
            }
            return xmlFiles[0];
        }

#if false
        public static string
        PackageDefinitionPathName(
            PackageIdentifier id)
        {
            if (id.Root != null)
            {
                var packageDirectory = id.Path;
                var definitionFileName = id.Name + ".xml";
                var definitionPathName = System.IO.Path.Combine(packageDirectory, definitionFileName);
                return definitionPathName;
            }
            else
            {
                return null;
            }
        }
#endif

#if true
        public static PackageDefinitionFile
        GetMasterPackage()
        {
            var workingDir = State.WorkingDirectory;
            var isWorkingPackageWellDefined = IsPackageDirectory(workingDir);
            if (!isWorkingPackageWellDefined)
            {
                throw new Exception("Working directory package is not well defined");
            }

            var masterDefinitionFile = new PackageDefinitionFile(GetPackageDefinitionPathname(workingDir), !State.ForceDefinitionFileUpdate);
            masterDefinitionFile.Read(true);
            return masterDefinitionFile;
        }
#else
        public static void
        IdentifyMainPackageOnly()
        {
            // find the working directory package
            bool isWorkingPackageWellDefined;
            var id = IsPackageDirectory(State.WorkingDirectory, out isWorkingPackageWellDefined);
            if (null == id)
            {
                throw new Exception("No valid package found in the working directory");
            }

            if (!isWorkingPackageWellDefined)
            {
                throw new Exception("Working directory package is not well defined");
            }

            var definitionPathName = PackageDefinitionPathName(id);
            var definitionFile = new PackageDefinitionFile(definitionPathName, !State.ForceDefinitionFileUpdate);
            definitionFile.Read(true);
            id.Definition = definitionFile;

            if (!OSUtilities.IsCurrentPlatformSupported(definitionFile.SupportedPlatforms))
            {
                Log.DebugMessage("Package '{0}' is supported on platforms '{1}' which does not include the current platform '{2}'.", id.Name, definitionFile.SupportedPlatforms, State.Platform);
                return;
            }

            if (!OSUtilities.IsCurrentPlatformSupported(id.PlatformFilter))
            {
                Log.DebugMessage("Package '{0}' is filtered on platforms '{1}' which does not include the current platform '{2}'.", id.Name, id.PlatformFilter, State.Platform);
                return;
            }

            var info = new PackageInformation(id);
            State.PackageInfo.Add(info);
        }
#endif

        public static void
        IdentifyMainAndDependentPackages(
            bool resolveToSinglePackageVersion,
            bool allowUndefinedPackages)
        {
#if true
            var packageRepos = new System.Collections.Generic.Queue<string>();
            foreach (var repo in State.PackageRepositories)
            {
                if (packageRepos.Contains(repo))
                {
                    continue;
                }
                packageRepos.Enqueue(repo);
            }
#else
            var buildList = new PackageBuildList();
#endif

#if true
#if true
            var masterDefinitionFile = GetMasterPackage();
#else
            var workingDir = State.WorkingDirectory;
            var isWorkingPackageWellDefined = IsPackageDirectory(workingDir);
            if (!isWorkingPackageWellDefined)
            {
                throw new Exception("Working directory package is not well defined");
            }

            var masterDefinitionFile = new PackageDefinitionFile(GetPackageDefinitionPathname(workingDir), !State.ForceDefinitionFileUpdate);
            masterDefinitionFile.Read(true);
#endif

            foreach (var repo in masterDefinitionFile.PackageRepositories)
            {
                if (packageRepos.Contains(repo))
                {
                    continue;
                }
                packageRepos.Enqueue(repo);
            }
#else
            // find the working directory package
            {
                var workingDir = State.WorkingDirectory;
                bool isWorkingPackageWellDefined;
                var id = IsPackageDirectory(workingDir, out isWorkingPackageWellDefined);
                if (null == id)
                {
                    throw new Exception("No valid package found in the working directory");
                }

                if (!isWorkingPackageWellDefined)
                {
                    throw new Exception("Working directory package is not well defined");
                }

                State.DependentPackageList.Add(id);
                buildList.Add(new PackageBuild(id));
            }
#endif

#if true
            // read the definition files of any package found in the package roots
            var candidatePackageDefinitions = new Array<PackageDefinitionFile>();
            candidatePackageDefinitions.Add(masterDefinitionFile);
            while (packageRepos.Count > 0)
            {
                var repo = packageRepos.Dequeue();
                var candidatePackageDirs = System.IO.Directory.GetDirectories(repo, BamSubFolder, System.IO.SearchOption.AllDirectories);

                State.PackageRepositories.Add(repo);

                foreach (var bamDir in candidatePackageDirs)
                {
                    var packageDir = System.IO.Path.GetDirectoryName(bamDir);
                    var packageDefinitionPath = GetPackageDefinitionPathname(packageDir);

                    // ignore any duplicates (can be found due to nested repositories)
                    if (null != candidatePackageDefinitions.Where(item => item.XMLFilename == packageDefinitionPath).FirstOrDefault())
                    {
                        continue;
                    }

                    var definitionFile = new PackageDefinitionFile(packageDefinitionPath, !State.ForceDefinitionFileUpdate);
                    definitionFile.Read(true);
                    candidatePackageDefinitions.Add(definitionFile);

                    foreach (var newRepo in definitionFile.PackageRepositories)
                    {
                        if (State.PackageRepositories.Contains(repo))
                        {
                            continue;
                        }
                        packageRepos.Enqueue(newRepo);
                    }
                }
            }

            // defaults come from
            // - the master definition file
            // - command line args (these trump the mdf)
            // and only requires resolving when referenced
            var packageDefinitions = new Array<PackageDefinitionFile>();
            PackageDefinitionFile.ResolveDependencies(masterDefinitionFile, packageDefinitions, candidatePackageDefinitions);

            // now resolve any duplicate names using defaults
            var duplicatePackageNames = packageDefinitions.GroupBy(item => item.Name).Where(item => item.Count() > 1).Select(item => item.Key);
            if (duplicatePackageNames.Count() > 0)
            {
                var versionSpeciferArgs = new PackageDefaultVersion();
                var packageVersionSpecifiers = CommandLineProcessor.Evaluate(versionSpeciferArgs);

                foreach (var dupName in duplicatePackageNames)
                {
                    var duplicates = packageDefinitions.Where(item => item.Name == dupName);
                    var masterDependency = masterDefinitionFile.Dependents.Where(item => item.Item1 == dupName);
                    var resolvedDuplicate = false;
                    var toRemove = new Array<PackageDefinitionFile>();
                    foreach (var masterDep in masterDependency)
                    {
                        // guaranteed that at most one instance of the dependency is marked as default
                        if (masterDep.Item3.HasValue && masterDep.Item3.Value)
                        {
                            resolvedDuplicate = true;
                        }
                        else
                        {
                            // TODO: check whether a command line argument of
                            // --<packagename>.Version=<default>
                            // has been supplied
                            foreach (var specifier in packageVersionSpecifiers)
                            {
                                if (!specifier.Contains(dupName))
                                {
                                    continue;
                                }

                                if (specifier[1] == masterDep.Item2)
                                {
                                    resolvedDuplicate = true;
                                }
                            }

                            if (!resolvedDuplicate)
                            {
                                toRemove.Add(packageDefinitions.Where(item => item.Name == masterDep.Item1 && item.Version == masterDep.Item2).ElementAt(0));
                            }
                        }
                    }

                    if (resolvedDuplicate)
                    {
                        packageDefinitions.RemoveAll(toRemove);
                    }
                    else
                    {
                        var message = new System.Text.StringBuilder();
                        message.AppendFormat("Unable resolve to a single version of package {0}", duplicates.ElementAt(0).Name);
                        message.AppendLine();
                        foreach (var dup in duplicates)
                        {
                            message.AppendLine(dup.FullName);
                        }
                        throw new Exception(message.ToString());
                    }
                }
            }

            Graph.Instance.SetPackageDefinitions(packageDefinitions);
#else
            // TODO: check for inconsistent circular dependencies
            // i.e. package A depends on B, and B depends on A, but a different version of A

            // process all the dependent packages
            int i = 0;
            while (i < State.DependentPackageList.Count)
            {
                var id = State.DependentPackageList[i++];
                var definitionPathName = PackageDefinitionPathName(id);
                if (null == definitionPathName)
                {
                    if (allowUndefinedPackages)
                    {
                        continue;
                    }
                    else
                    {
                        throw new Exception("Package '{0}' definition file not found. Are you missing a package root?", id.ToString("-"));
                    }
                }

                var definitionFile = new PackageDefinitionFile(definitionPathName, !State.ForceDefinitionFileUpdate);
                try
                {
                    definitionFile.Read(true);
                    id.Definition = definitionFile;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex, "Failed loading definition file {0}", definitionPathName);
                }

                if (!OSUtilities.IsCurrentPlatformSupported(definitionFile.SupportedPlatforms))
                {
                    Log.DebugMessage("Package '{0}' is supported on platforms '{1}' which does not include the current platform '{2}'.", id.Name, definitionFile.SupportedPlatforms, State.Platform);
                    continue;
                }

                if (!OSUtilities.IsCurrentPlatformSupported(id.PlatformFilter))
                {
                    Log.DebugMessage("Package '{0}' is filtered on platforms '{1}' which does not include the current platform '{2}'.", id.Name, id.PlatformFilter, State.Platform);
                    continue;
                }

                foreach (var id2 in definitionFile.PackageIdentifiers)
                {
                    if (!OSUtilities.IsCurrentPlatformSupported(id2.PlatformFilter))
                    {
                        Log.DebugMessage("Package '{0}-{1}' is not supported on the current platform", id2.Name, id2.Version);
                        continue;
                    }

                    var build = buildList.GetPackage(id2.Name);
                    if (null == build)
                    {
                        build = new PackageBuild(id2);
                        buildList.Add(build);
                    }
                    else
                    {
                        build.Versions.Add(id2);
                    }

                    State.DependentPackageList.Add(id2);
                }
            }

            // can we resolve down to a single package?
            if (resolveToSinglePackageVersion)
            {
                // any package identifiers that are dependents from package versions discarded
                // any of these can be removed from consideration to resolve down to a single package version
                var ignoreList = new UniqueList<PackageIdentifier>();

                foreach (var build in buildList)
                {
                    var filteredVersionList = new UniqueList<PackageIdentifier>();
                    if (build.Versions.Count > 1)
                    {
                        foreach (var id in build.Versions)
                        {
                            if (ignoreList.Contains(id))
                            {
                                continue;
                            }
                            filteredVersionList.Add(id);
                        }

                        if (filteredVersionList.Count == 1)
                        {
                            build.SelectedVersion = filteredVersionList[0];
                        }
                    }
                    else
                    {
                        filteredVersionList.AddRange(build.Versions);
                    }

                    var ignoreVersions = new UniqueList<PackageIdentifier>();
                    if (filteredVersionList.Count > 1)
                    {
                        string defaultVersion;
                        if (!State.Has("PackageDefaultVersions", build.Name.ToLower()))
                        {
                            var defaultVersions = new StringArray();
                            foreach (var id in filteredVersionList)
                            {
                                if (id.IsDefaultVersion)
                                {
                                    defaultVersions.Add(id.Version);
                                }
                            }

                            if (0 == defaultVersions.Count)
                            {
                                throw new Exception("Package '{0}' has multiple versions. Please specify which one to use:\n{1}", build.Name, build.Versions);
                            }
                            else if (defaultVersions.Count > 1)
                            {
                                throw new Exception("Package '{0}' has multiple versions and multiple default versions from definition files. Please reduce this to a single version by changing the definition files or using an override on the command line. Defaults set from the definition files are:\n{1}", build.Name, defaultVersions.ToString('\n'));
                            }

                            defaultVersion = defaultVersions[0];
                        }
                        else
                        {
                            defaultVersion = State.Get("PackageDefaultVersions", build.Name.ToLower()) as string;
                        }
                        bool found = false;
                        foreach (var version in filteredVersionList)
                        {
                            if (version.Version == defaultVersion)
                            {
                                build.SelectedVersion = version;
                                found = true;
                            }
                            else
                            {
                                ignoreVersions.Add(version);
                            }
                        }
                        if (!found)
                        {
                            throw new Exception("Specified version for package '{0}' is '{1}' but the available package versions are {2}", build.Name, defaultVersion, build.Versions.ToString());
                        }
                    }
                    else if (State.Has("PackageDefaultVersions", build.Name.ToLower()))
                    {
                        var defaultVersion = State.Get("PackageDefaultVersions", build.Name.ToLower()) as string;
                        if (build.SelectedVersion.Version != defaultVersion)
                        {
                            throw new Exception("{0} version selected on command line was {1}, but only {2} exists in the definition file", build.Name, defaultVersion, build.SelectedVersion.Version);
                        }
                    }

                    var info = new PackageInformation(build.SelectedVersion);
                    State.PackageInfo.Add(info);

                    foreach (var ignoreV in ignoreVersions)
                    {
                        var allDependentIdentifiers = ignoreV.Definition.RecursiveDependentIdentifiers();
                        foreach (var ignoredId in allDependentIdentifiers)
                        {
                            // TODO: can't use AddRange here, as it's not a unique check
                            ignoreList.Add(ignoredId);
                        }
                    }
                }
            }
            else
            {
                foreach (var build in buildList)
                {
                    foreach (var id in build.Versions)
                    {
                        PackageInformation info = new PackageInformation(id);
                        State.PackageInfo.Add(info, true);
                    }
                }
            }

            if (0 == State.PackageInfo.Count)
            {
                throw new Exception("No packages were identified to build");
            }

            Log.DebugMessage("Packages identified are:\n{0}", State.PackageInfo.ToString("\t", "\n"));
#endif
        }

        private static string
        GetPackageHash(
            StringArray sourceCode,
            StringArray definitions,
            StringArray bamAssemblies)
        {
            int hashCode = 0;
            foreach (var source in sourceCode)
            {
                hashCode ^= source.GetHashCode();
            }
            foreach (var define in definitions)
            {
                hashCode ^= define.GetHashCode();
            }
            foreach (var assemblyPath in bamAssemblies)
            {
                var assembly = System.Reflection.Assembly.Load(assemblyPath);
                var version = assembly.GetName().Version.ToString();
                hashCode ^= version.GetHashCode();
            }
            var hash = hashCode.ToString();
            return hash;
        }

        public static bool
        CompilePackageAssembly()
        {
            // validate build root
            if (null == State.BuildRoot)
            {
                throw new Exception("Build root has not been specified");
            }

            var gatherSourceProfile = new TimeProfile(ETimingProfiles.GatherSource);
            gatherSourceProfile.StartProfile();

            IdentifyMainAndDependentPackages(true, false);

            if (!System.IO.Directory.Exists(State.BuildRoot))
            {
                System.IO.Directory.CreateDirectory(State.BuildRoot);
            }

#if true
#else
            var mainPackage = State.PackageInfo.MainPackage;

            Log.DebugMessage("Package is '{0}' in '{1}", mainPackage.Identifier.ToString("-"), mainPackage.Identifier.Root.GetSingleRawPath());
#endif

            BuilderUtilities.SetBuilderPackage();

            // Create resource file containing package information
            var resourceFilePathName = PackageListResourceFile.WriteResourceFile();

            var definitions = new StringArray();

            // gather source files
            var sourceCode = new StringArray();
#if true
            int packageIndex = 0;
            foreach (var package in Graph.Instance.Packages)
            {
                Log.DebugMessage("{0}: '{1}' @ '{2}'", packageIndex, package.Version, package.PackageRepositories[0]);

                // to compile with debug information, you must compile the files
                // to compile without, we need to file contents to hash the source
                if (State.CompileWithDebugSymbols)
                {
                    var scripts = package.GetScriptFiles();
                    sourceCode.AddRange(scripts);
                    Log.DebugMessage(scripts.ToString("\n\t"));
                }
                else
                {
                    foreach (var scriptFile in package.GetScriptFiles())
                    {
                        using (var reader = new System.IO.StreamReader(scriptFile))
                        {
                            sourceCode.Add(reader.ReadToEnd());
                        }
                        Log.DebugMessage("\t'{0}'", scriptFile);
                    }
                }

                foreach (var define in package.Definitions)
                {
                    if (!definitions.Contains(define))
                    {
                        definitions.Add(define);
                    }
                }

                ++packageIndex;
            }
#else
            int packageIndex = 0;
            foreach (var package in State.PackageInfo)
            {
                var id = package.Identifier;
                Log.DebugMessage("{0}: '{1}' @ '{2}'", packageIndex, id.ToString("-"), id.Root.GetSingleRawPath());

                // to compile with debug information, you must compile the files
                // to compile without, we need to file contents to hash the source
                if (State.CompileWithDebugSymbols)
                {
                    sourceCode.Add(id.ScriptPathName);
                }
                else
                {
                    using (var reader = new System.IO.StreamReader(id.ScriptPathName))
                    {
                        sourceCode.Add(reader.ReadToEnd());
                    }
                }
                Log.DebugMessage("\t'{0}'", id.ScriptPathName);

                if (null != package.Scripts)
                {
                    if (State.CompileWithDebugSymbols)
                    {
                        foreach (var scriptFile in package.Scripts)
                        {
                            sourceCode.Add(scriptFile);
                            Log.DebugMessage("\t'{0}'", scriptFile);
                        }
                    }
                    else
                    {
                        foreach (var scriptFile in package.Scripts)
                        {
                            using (var reader = new System.IO.StreamReader(scriptFile))
                            {
                                sourceCode.Add(reader.ReadToEnd());
                            }
                            Log.DebugMessage("\t'{0}'", scriptFile);
                        }
                    }
                }
                if (null != package.BuilderScripts)
                {
                    if (State.CompileWithDebugSymbols)
                    {
                        foreach (var builderScriptFile in package.BuilderScripts)
                        {
                            sourceCode.Add(builderScriptFile);
                            Log.DebugMessage("\t'{0}'", builderScriptFile);
                        }
                    }
                    else
                    {
                        foreach (var builderScriptFile in package.BuilderScripts)
                        {
                            using (var reader = new System.IO.StreamReader(builderScriptFile))
                            {
                                sourceCode.Add(reader.ReadToEnd());
                            }
                            Log.DebugMessage("\t'{0}'", builderScriptFile);
                        }
                    }
                }

                foreach (var define in package.Identifier.Definition.Definitions)
                {
                    if (!definitions.Contains(define))
                    {
                        definitions.Add(define);
                    }
                }

                definitions.Add(package.Identifier.CompilationDefinition);
                Log.DebugMessage("Package define: {0}", package.Identifier.CompilationDefinition);

                ++packageIndex;
            }
#endif

            // add/remove other definitions
            definitions.Add(VersionDefineForCompiler);
            definitions.Add(HostPlatformDefineForCompiler);
            // command line definitions
            definitions.AddRange(State.PackageCompilationDefines);
            definitions.RemoveAll(State.PackageCompilationUndefines);
            definitions.Sort();

            gatherSourceProfile.StopProfile();

            var assemblyCompileProfile = new TimeProfile(ETimingProfiles.AssemblyCompilation);
            assemblyCompileProfile.StartProfile();

            // assembly is written to the build root
            var cachedAssemblyPathname = System.IO.Path.Combine(State.BuildRoot, "CachedPackageAssembly");
#if true
            cachedAssemblyPathname = System.IO.Path.Combine(cachedAssemblyPathname, Graph.Instance.MasterPackage.Name) + ".dll";
#else
            cachedAssemblyPathname = System.IO.Path.Combine(cachedAssemblyPathname, mainPackage.Name) + ".dll";
#endif
            var hashPathName = System.IO.Path.ChangeExtension(cachedAssemblyPathname, "hash");
            string thisHashCode = null;

            if (!State.CompileWithDebugSymbols)
            {
                // can an existing assembly be reused?
#if true
                thisHashCode = GetPackageHash(sourceCode, definitions, Graph.Instance.MasterPackage.BamAssemblies);
                if (State.CacheAssembly)
                {
                    if (System.IO.File.Exists(hashPathName))
                    {
                        using (var reader = new System.IO.StreamReader(hashPathName))
                        {
                            var diskHashCode = reader.ReadLine();
                            if (diskHashCode.Equals(thisHashCode))
                            {
                                Log.DebugMessage("Cached assembly used '{0}', with hash {1}", cachedAssemblyPathname, diskHashCode);
                                Log.Detail("Re-using existing package assembly");
                                State.ScriptAssemblyPathname = cachedAssemblyPathname;

                                assemblyCompileProfile.StopProfile();

                                return true;
                            }
                            else
                            {
                                Log.DebugMessage("Assembly hashes differ: '{0}' (disk) '{1}' now", diskHashCode, thisHashCode);
                            }
                        }
                    }
                }
#else
                thisHashCode = GetPackageHash(sourceCode, definitions, mainPackage.Identifier.Definition.BamAssemblies);
                if (State.CacheAssembly)
                {
                    if (System.IO.File.Exists(hashPathName))
                    {
                        using (var reader = new System.IO.StreamReader(hashPathName))
                        {
                            var diskHashCode = reader.ReadLine();
                            if (diskHashCode.Equals(thisHashCode))
                            {
                                Log.DebugMessage("Cached assembly used '{0}', with hash {1}", cachedAssemblyPathname, diskHashCode);
                                Log.Detail("Re-using existing package assembly");
                                State.ScriptAssemblyPathname = cachedAssemblyPathname;

                                assemblyCompileProfile.StopProfile();

                                return true;
                            }
                            else
                            {
                                Log.DebugMessage("Assembly hashes differ: '{0}' (disk) '{1}' now", diskHashCode, thisHashCode);
                            }
                        }
                    }
                }
#endif
            }

            // use the compiler in the current runtime version to build the assembly of packages
            var clrVersion = System.Environment.Version;
            var compilerVersion = System.String.Format("v{0}.{1}", clrVersion.Major, clrVersion.Minor);

            Log.Detail("Compiling package assembly, using C# compiler {0}", compilerVersion);

            var providerOptions = new System.Collections.Generic.Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", compilerVersion);

            if (State.RunningMono)
            {
                Log.DebugMessage("Compiling assembly for Mono");
            }

            using (var provider = new Microsoft.CSharp.CSharpCodeProvider(providerOptions))
            {
                var compilerParameters = new System.CodeDom.Compiler.CompilerParameters();
                compilerParameters.TreatWarningsAsErrors = true;
                if (!State.RunningMono)
                {
                    compilerParameters.WarningLevel = 4;
                }
                else
                {
                    // mono appears to be a lot fussier about warnings
                    compilerParameters.WarningLevel = 2;
                }
                compilerParameters.GenerateExecutable = false;
                compilerParameters.GenerateInMemory = false;

                if (State.CompileWithDebugSymbols)
                {
#if true
                    compilerParameters.OutputAssembly = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Graph.Instance.MasterPackage.Name) + ".dll";
#else
                    compilerParameters.OutputAssembly = System.IO.Path.Combine(System.IO.Path.GetTempPath(), mainPackage.Name) + ".dll";
#endif
                }
                else
                {
                    compilerParameters.OutputAssembly = cachedAssemblyPathname;
                }

                var compilerOptions = "/checked+ /unsafe-";
                if (State.CompileWithDebugSymbols)
                {
                    compilerParameters.IncludeDebugInformation = true;
                    compilerOptions += " /optimize-";
                    if (State.RunningMono)
                    {
                        compilerOptions += " /define:DEBUG";
                    }
                }
                else
                {
                    compilerOptions += " /optimize+";
                }
                if (!State.RunningMono)
                {
                    // apparently, some versions of Mono don't support the /platform option
                    compilerOptions += " /platform:anycpu";
                }

                // define strings
                compilerOptions += " /define:" + definitions.ToString(';');

                compilerParameters.CompilerOptions = compilerOptions;
                compilerParameters.EmbeddedResources.Add(resourceFilePathName);

                if (provider.Supports(System.CodeDom.Compiler.GeneratorSupport.Resources))
                {
                    // Bam assembly
#if true
                    // TODO: Q: why is it only for the master package? Why not all of them, which may have additional dependencies?
                    foreach (var assembly in Graph.Instance.MasterPackage.BamAssemblies)
                    {
                        var assemblyFileName = System.String.Format("{0}.dll", assembly);
                        var assemblyPathName = System.IO.Path.Combine(State.ExecutableDirectory, assemblyFileName);
                        compilerParameters.ReferencedAssemblies.Add(assemblyPathName);
                    }

                    // DotNet assembly
                    foreach (var desc in Graph.Instance.MasterPackage.DotNetAssemblies)
                    {
                        var assemblyFileName = System.String.Format("{0}.dll", desc.Name);
                        compilerParameters.ReferencedAssemblies.Add(assemblyFileName);
                    }
#else
                    foreach (var assembly in mainPackage.Identifier.Definition.BamAssemblies)
                    {
                        var assemblyFileName = System.String.Format("{0}.dll", assembly);
                        var assemblyPathName = System.IO.Path.Combine(State.ExecutableDirectory, assemblyFileName);
                        compilerParameters.ReferencedAssemblies.Add(assemblyPathName);
                    }

                    // DotNet assembly
                    foreach (var desc in mainPackage.Identifier.Definition.DotNetAssemblies)
                    {
                        var assemblyFileName = System.String.Format("{0}.dll", desc.Name);
                        compilerParameters.ReferencedAssemblies.Add(assemblyFileName);
                    }
#endif

                    if (State.RunningMono)
                    {
                        compilerParameters.ReferencedAssemblies.Add("Mono.Posix.dll");
                    }
                }
                else
                {
                    throw new Exception("C# compiler does not support Resources");
                }

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(compilerParameters.OutputAssembly));

                var results = State.CompileWithDebugSymbols ?
                    provider.CompileAssemblyFromFile(compilerParameters, sourceCode.ToArray()) :
                    provider.CompileAssemblyFromSource(compilerParameters, sourceCode.ToArray());

                if (results.Errors.HasErrors || results.Errors.HasWarnings)
                {
#if true
                    Log.ErrorMessage("Failed to compile package '{0}'. There are {1} errors.", Graph.Instance.MasterPackage.FullName, results.Errors.Count);
                    foreach (System.CodeDom.Compiler.CompilerError error in results.Errors)
                    {
                        Log.ErrorMessage("\t{0}({1}): {2} {3}", error.FileName, error.Line, error.ErrorNumber, error.ErrorText);
                    }
#else
                    Log.ErrorMessage("Failed to compile package '{0}'. There are {1} errors.", mainPackage.FullName, results.Errors.Count);
                    foreach (System.CodeDom.Compiler.CompilerError error in results.Errors)
                    {
                        Log.ErrorMessage("\t{0}({1}): {2} {3}", error.FileName, error.Line, error.ErrorNumber, error.ErrorText);
                    }
#endif
                    return false;
                }

                if (!State.CompileWithDebugSymbols)
                {
                    if (State.CacheAssembly)
                    {
                        using (var writer = new System.IO.StreamWriter(hashPathName))
                        {
                            writer.WriteLine(thisHashCode);
                        }
                    }
                    else
                    {
                        // will not throw if the file doesn't exist
                        System.IO.File.Delete(hashPathName);
                    }
                }

                Log.DebugMessage("Written assembly to '{0}'", compilerParameters.OutputAssembly);
                State.ScriptAssemblyPathname = compilerParameters.OutputAssembly;
            }

            assemblyCompileProfile.StopProfile();

            return true;
        }

        public static void
        LoadPackageAssembly()
        {
            var assemblyLoadProfile = new TimeProfile(ETimingProfiles.LoadAssembly);
            assemblyLoadProfile.StartProfile();

            System.Reflection.Assembly scriptAssembly = null;

            try
            {
                // this code works from an untrusted location, and debugging IS available when
                // the pdb (.NET)/mdb (Mono) resides beside the assembly
                byte[] asmBytes = System.IO.File.ReadAllBytes(State.ScriptAssemblyPathname);
                if (State.CompileWithDebugSymbols)
                {
                    var debugInfoFilename = State.RunningMono ?
                        State.ScriptAssemblyPathname + ".mdb" :
                        System.IO.Path.ChangeExtension(State.ScriptAssemblyPathname, ".pdb");
                    if (System.IO.File.Exists(debugInfoFilename))
                    {
                        byte[] pdbBytes = System.IO.File.ReadAllBytes(debugInfoFilename);
                        scriptAssembly = System.Reflection.Assembly.Load(asmBytes, pdbBytes);
                    }
                }

                if (null == scriptAssembly)
                {
                    scriptAssembly = System.Reflection.Assembly.Load(asmBytes);
                }
            }
            catch (System.IO.FileNotFoundException exception)
            {
                Log.ErrorMessage("Could not find assembly '{0}'", State.ScriptAssemblyPathname);
                throw exception;
            }
            catch (System.Exception exception)
            {
                throw exception;
            }

            State.ScriptAssembly = scriptAssembly;

            assemblyLoadProfile.StopProfile();
        }
    }
}

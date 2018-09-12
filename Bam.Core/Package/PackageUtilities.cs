#region License
// Copyright (c) 2010-2018, Mark Final
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
    /// Static utility class with useful package related methods.
    /// </summary>
    public static class PackageUtilities
    {
        /// <summary>
        /// Central definition of what the Bam sub-folder is called.
        /// </summary>
        public static readonly string BamSubFolder = "bam";

        /// <summary>
        /// Central definition of what the scripts sub-folder is called.
        /// </summary>
        public static readonly string ScriptsSubFolder = "Scripts";

        /// <summary>
        /// Utility method to create a new package.
        /// </summary>
        public static void
        MakePackage()
        {
            var packageDir = Graph.Instance.ProcessState.WorkingDirectory;
            var bamDir = System.IO.Path.Combine(packageDir, BamSubFolder);
            if (System.IO.Directory.Exists(bamDir))
            {
                throw new Exception("Cannot create new package: A Bam package already exists at {0}", packageDir);
            }

            var packageNameArgument = new Options.PackageName();
            var packageName = CommandLineProcessor.Evaluate(packageNameArgument);
            if (null == packageName)
            {
                throw new Exception("Cannot create new package: No name was defined. Use {0} on the command line to specify it.", (packageNameArgument as ICommandLineArgument).LongName);
            }

            var packageVersion = CommandLineProcessor.Evaluate(new Options.PackageVersion());
            var definition = new PackageDefinition(bamDir, packageName, packageVersion);

            IOWrapper.CreateDirectory(bamDir);
            definition.Write();

            var scriptsDir = System.IO.Path.Combine(bamDir, ScriptsSubFolder);
            IOWrapper.CreateDirectory(scriptsDir);

            var initialScriptFile = System.IO.Path.Combine(scriptsDir, packageName) + ".cs";
            using (System.IO.TextWriter writer = new System.IO.StreamWriter(initialScriptFile))
            {
                writer.NewLine = "\n";
                writer.WriteLine("using Bam.Core;");
                writer.WriteLine("namespace {0}", packageName);
                writer.WriteLine("{");
                writer.WriteLine("    // write modules here ...");
                writer.WriteLine("}");
            }

            Log.Info("Package {0} was successfully created at {1}", definition.FullName, packageDir);
        }

        /// <summary>
        /// Utility method for adding a dependent package.
        /// </summary>
        public static void
        AddDependentPackage()
        {
            var packageNameArgument = new Options.PackageName();
            var packageName = CommandLineProcessor.Evaluate(packageNameArgument);
            if (null == packageName)
            {
                throw new Exception("No name was defined. Use {0} on the command line to specify it.", (packageNameArgument as ICommandLineArgument).LongName);
            }

            var packageVersion = CommandLineProcessor.Evaluate(new Options.PackageVersion());

            var masterPackage = GetMasterPackage();
            if (null != masterPackage.Dependents.FirstOrDefault(item => item.Item1 == packageName && item.Item2 == packageVersion))
            {
                if (null != packageVersion)
                {
                    throw new Exception("Package dependency {0}, version {1}, is already present", packageName, packageVersion);
                }
                else
                {
                    throw new Exception("Package dependency {0} is already present", packageName);
                }
            }

            var newDepTuple = new System.Tuple<string, string, bool?>(packageName, packageVersion, null);
            masterPackage.Dependents.Add(newDepTuple);
            // TODO: this is unfortunate having to write the file in order to use it with IdentifyAllPackages
            masterPackage.Write();

            // validate that the addition is ok
            try
            {
                PackageUtilities.IdentifyAllPackages();
            }
            catch (Exception exception)
            {
                masterPackage.Dependents.Remove(newDepTuple);
                masterPackage.Write();
                throw new Exception(exception, "Failed to add dependent. Are all necessary package repositories specified?");
            }
        }

        /// <summary>
        /// Get the preprocessor define specifying the Bam Core version.
        /// </summary>
        /// <value>The version define for compiler.</value>
        public static string VersionDefineForCompiler
        {
            get
            {
                var coreVersion = Graph.Instance.ProcessState.Version;
                var coreVersionDefine = System.String.Format("BAM_CORE_VERSION_{0}_{1}_{2}",
                    coreVersion.Major,
                    coreVersion.Minor,
                    coreVersion.Revision);
                return coreVersionDefine;
            }
        }

        /// <summary>
        /// Get the preprocessor define specifying the host OS.
        /// </summary>
        /// <value>The host platform define for compiler.</value>
        public static string HostPlatformDefineForCompiler
        {
            get
            {
                var platformString = Platform.ToString(OSUtilities.CurrentPlatform, '\0', "BAM_HOST_", true);
                return platformString;
            }
        }

        /// <summary>
        /// Determine if a path is configured as a package.
        /// </summary>
        /// <returns><c>true</c> if is package directory the specified packagePath; otherwise, <c>false</c>.</returns>
        /// <param name="packagePath">Package path.</param>
        public static bool
        IsPackageDirectory(
            string packagePath)
        {
            var bamDir = System.IO.Path.Combine(packagePath, BamSubFolder);
            if (!System.IO.Directory.Exists(bamDir))
            {
                throw new Exception("Path {0} does not form a BAM! package: missing '{1}' subdirectory", packagePath, BamSubFolder);
            }

            return true;
        }

        /// <summary>
        /// Get the XML pathname for the package.
        /// </summary>
        /// <returns>The package definition pathname.</returns>
        /// <param name="packagePath">Package path.</param>
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

        /// <summary>
        /// Get the package in which Bam is executed.
        /// </summary>
        /// <returns>The master package.</returns>
        public static PackageDefinition
        GetMasterPackage()
        {
            var workingDir = Graph.Instance.ProcessState.WorkingDirectory;
            var isWorkingPackageWellDefined = IsPackageDirectory(workingDir);
            if (!isWorkingPackageWellDefined)
            {
                throw new Exception("Working directory package is not well defined");
            }

            var masterDefinitionFile = new PackageDefinition(GetPackageDefinitionPathname(workingDir));
            masterDefinitionFile.Read();

            // in case the master package is not in a formal package repository structure, add it's parent directory
            // as a repository, so that sibling packages can be found
            var parentDir = System.IO.Path.GetDirectoryName(workingDir);
            masterDefinitionFile.PackageRepositories.AddUnique(parentDir);

            return masterDefinitionFile;
        }

        private static PackageDefinition
        TryToResolveDuplicate(
            PackageDefinition masterDefinitionFile,
            string dupName,
            System.Collections.Generic.IEnumerable<PackageDefinition> duplicates,
            Array<PackageDefinition> packageDefinitions,
            Array<StringArray> packageVersionSpecifiers,
            Array<PackageDefinition> toRemove)
        {
            // command line specifications take precedence to resolve a duplicate
            foreach (var specifier in packageVersionSpecifiers)
            {
                if (!specifier.Contains(dupName))
                {
                    continue;
                }

                foreach (var dupPackage in duplicates)
                {
                    if (specifier[1] == dupPackage.Version)
                    {
                        toRemove.AddRange(packageDefinitions.Where(item => (item.Name == dupName) && (item != dupPackage)));
                        Log.DebugMessage("Duplicate of {0} resolved to {1} by command line", dupName, dupPackage.ToString());
                        return dupPackage;
                    }
                }

                var noMatchMessage = new System.Text.StringBuilder();
                noMatchMessage.AppendFormat("Command line version specified, {0}, could not resolve to one of the available versions of package {1}:", specifier[1], duplicates.First().Name);
                noMatchMessage.AppendLine();
                foreach (var dup in duplicates)
                {
                    noMatchMessage.AppendFormat("\t{0}", dup.Version);
                    noMatchMessage.AppendLine();
                }
                throw new Exception(noMatchMessage.ToString());
            }

            // now look at the master dependency file, for any 'default' specifications
            var masterDependency = masterDefinitionFile.Dependents.FirstOrDefault(item => item.Item1 == dupName && item.Item3.HasValue && item.Item3.Value);
            if (null != masterDependency)
            {
                toRemove.AddRange(packageDefinitions.Where(item => (item.Name == dupName) && (item.Version != masterDependency.Item2)));
                var found = packageDefinitions.First(item => (item.Name == dupName) && (item.Version == masterDependency.Item2));
                Log.DebugMessage("Duplicate of {0} resolved to {1} by master definition file", dupName, found.ToString());
                return found;
            }

            Log.DebugMessage("Duplicate of {0} unresolved", dupName);
            return null;
        }

        private static Array<PackageDefinition>
        FindPackagesToRemove(
            Array<PackageDefinition> initialToRemove,
            Array<PackageDefinition> packageDefinitions,
            PackageDefinition masterDefinitionFile)
        {
            var totalToRemove = new Array<PackageDefinition>(initialToRemove);
            var queuedForRemoval = new System.Collections.Generic.Queue<PackageDefinition>(initialToRemove);
            while (queuedForRemoval.Count > 0)
            {
                var current = queuedForRemoval.Dequeue();
                totalToRemove.AddUnique(current);
                Log.DebugMessage("Examining: {0}", current.ToString());

                foreach (var package in packageDefinitions)
                {
                    if (package.Parents.Contains(current))
                    {
                        Log.DebugMessage("Package {0} parents include {1}, so removing reference", package.ToString(), current.ToString());
                        package.Parents.Remove(current);
                    }

                    if (!package.Parents.Any() && package != masterDefinitionFile && !totalToRemove.Contains(package) && !queuedForRemoval.Contains(package))
                    {
                        Log.DebugMessage("*** Package {0} enqueued for removal since no-one refers to it", package.ToString());
                        queuedForRemoval.Enqueue(package);
                    }
                }
            }
            return totalToRemove;
        }

        private static void
        EnqueuePackageRepositoryToVisit(
            System.Collections.Generic.LinkedList<System.Tuple<string,PackageDefinition>> reposToVisit,
            ref int reposAdded,
            string repoPath,
            PackageDefinition sourcePackageDefinition)
        {
            // need to always pre-load the search paths (reposToVisit) with the repo that the master package resides in
            if (null != sourcePackageDefinition)
            {
                // visited already? ignore
                if (Graph.Instance.PackageRepositories.Contains(repoPath))
                {
                    return;
                }
            }
            // already planned to visit? ignore
            if (reposToVisit.Any(item => item.Item1 == repoPath))
            {
                return;
            }
            reposToVisit.AddLast(System.Tuple.Create<string, PackageDefinition>(repoPath, sourcePackageDefinition));
            ++reposAdded;
        }

        /// <summary>
        /// Scan though all package repositories for all package dependencies, and resolve any duplicate package names
        /// by either data in the package definition file, or on the command line, by specifying a particular version to
        /// use. The master package definition file is the source of disambiguation for package versions.
        /// </summary>
        /// <param name="allowDuplicates">If set to <c>true</c> allow duplicates.</param>
        /// <param name="enforceBamAssemblyVersions">If set to <c>true</c> enforce bam assembly versions.</param>
        public static void
        IdentifyAllPackages(
            bool allowDuplicates = false,
            bool enforceBamAssemblyVersions = true)
        {
            var packageRepos = new System.Collections.Generic.LinkedList<System.Tuple<string,PackageDefinition>>();
            int reposHWM = 0;
            foreach (var repo in Graph.Instance.PackageRepositories)
            {
                EnqueuePackageRepositoryToVisit(packageRepos, ref reposHWM, repo, null);
            }

            var masterDefinitionFile = GetMasterPackage();
            // inject any packages from the command line into the master definition file
            // and these will be defaults
            var injectPackages = CommandLineProcessor.Evaluate(new Options.InjectDefaultPackage());
            if (null != injectPackages)
            {
                foreach (var injected in injectPackages)
                {
                    var name = injected[0];
                    string version = null;
                    if (injected.Count > 1)
                    {
                        version = injected[1].TrimStart(new [] {'-'}); // see regex in InjectDefaultPackage
                    }
                    var is_default = true;
                    var dependent = new System.Tuple<string, string, bool?>(name, version, is_default);
                    masterDefinitionFile.Dependents.AddUnique(dependent);
                }
            }
            foreach (var repo in masterDefinitionFile.PackageRepositories)
            {
                EnqueuePackageRepositoryToVisit(packageRepos, ref reposHWM, repo, masterDefinitionFile);
            }

            // read the definition files of any package found in the package roots
            var candidatePackageDefinitions = new Array<PackageDefinition>();
            candidatePackageDefinitions.Add(masterDefinitionFile);
            var packageReposVisited = 0;
            Log.Detail("Querying package repositories...");
            while (packageRepos.Count > 0)
            {
                var repoTuple = packageRepos.First();
                packageRepos.RemoveFirst();
                var repo = repoTuple.Item1;
                if (!System.IO.Directory.Exists(repo))
                {
                    var message = new System.Text.StringBuilder();
                    message.AppendFormat("Package repository directory {0} does not exist.", repo);
                    message.AppendLine();
                    message.AppendFormat("Repository requested from {0}", repoTuple.Item2.XMLFilename);
                    message.AppendLine();
                    throw new Exception(message.ToString());
                }

                // faster than System.IO.Directory.GetDirectories(repo, BamSubFolder, System.IO.SearchOption.AllDirectories);
                // when there are deep directories
                StringArray candidatePackageDirs = new StringArray();
                var possiblePackages = System.IO.Directory.GetDirectories(repo, "*", System.IO.SearchOption.TopDirectoryOnly);
                foreach (var packageDir in possiblePackages)
                {
                    var possibleBamFolder = System.IO.Path.Combine(packageDir, BamSubFolder);
                    if (System.IO.Directory.Exists(possibleBamFolder))
                    {
                        candidatePackageDirs.Add(packageDir);
                    }
                }

                Graph.Instance.PackageRepositories.Add(repo);

                foreach (var packageDir in candidatePackageDirs)
                {
                    var packageDefinitionPath = GetPackageDefinitionPathname(packageDir);

                    // ignore any duplicates (can be found due to nested repositories)
                    if (null != candidatePackageDefinitions.FirstOrDefault(item => item.XMLFilename == packageDefinitionPath))
                    {
                        continue;
                    }

                    var definitionFile = new PackageDefinition(packageDefinitionPath);
                    definitionFile.Read();
                    candidatePackageDefinitions.Add(definitionFile);

                    foreach (var newRepo in definitionFile.PackageRepositories)
                    {
                        EnqueuePackageRepositoryToVisit(packageRepos, ref reposHWM, newRepo, definitionFile);
                    }
                }

                ++packageReposVisited;
                Log.DetailProgress("{0,3}%", (int)(100 * ((float)packageReposVisited / reposHWM)));
            }
#if DEBUG
            if (packageReposVisited != reposHWM)
            {
                throw new Exception("Inconsistent package repository count: {0} added, {1} visited", reposHWM, packageReposVisited);
            }
#endif

            // defaults come from
            // - the master definition file
            // - command line args (these trump the mdf)
            // and only requires resolving when referenced
            var packageDefinitions = new Array<PackageDefinition>();
            PackageDefinition.ResolveDependencies(masterDefinitionFile, packageDefinitions, candidatePackageDefinitions);

            // now resolve any duplicate names using defaults
            // unless duplicates are allowed
            var duplicatePackageNames = packageDefinitions.GroupBy(item => item.Name).Where(item => item.Count() > 1).Select(item => item.Key);
            var uniquePackageNames = packageDefinitions.GroupBy(item => item.Name).Where(item => item.Count() == 1).Select(item => item.Key);
            var versionSpeciferArgs = new Options.PackageDefaultVersion();
            var packageVersionSpecifiers = CommandLineProcessor.Evaluate(versionSpeciferArgs);
            if ((duplicatePackageNames.Count() > 0) && !allowDuplicates)
            {
                foreach (var dupName in duplicatePackageNames)
                {
                    Log.DebugMessage("Duplicate '{0}'; total packages {1}", dupName, packageDefinitions.Count);
                    var duplicates = packageDefinitions.Where(item => item.Name == dupName);
                    var toRemove = new Array<PackageDefinition>();
                    var resolvedDuplicate = TryToResolveDuplicate(masterDefinitionFile, dupName, duplicates, packageDefinitions, packageVersionSpecifiers, toRemove);

                    Log.DebugMessage("Attempting to remove:\n\t{0}", toRemove.ToString("\n\t"));
                    // try removing any packages that have already been resolved
                    // which, in turn, can remove additional packages that have become orphaned by other removals
                    packageDefinitions.RemoveAll(FindPackagesToRemove(toRemove, packageDefinitions, masterDefinitionFile));

                    if (null != resolvedDuplicate)
                    {
                        continue;
                    }

                    // and if that has reduced the duplicates for this package down to a single version, we're good to carry on
                    var numDuplicates = duplicates.Count(); // this is LINQ, so it's 'live'
                    if (1 == numDuplicates)
                    {
                        continue;
                    }

                    // otherwise, error
                    var resolveErrorMessage = new System.Text.StringBuilder();
                    if (numDuplicates > 0)
                    {
                        resolveErrorMessage.AppendFormat("Unable to resolve to a single version of package {0}. Use --{0}.version=<version> to resolve.", dupName);
                        resolveErrorMessage.AppendLine();
                        resolveErrorMessage.AppendLine("Available versions of the package are:");
                        foreach (var dup in duplicates)
                        {
                            resolveErrorMessage.AppendFormat("\t{0}", dup.Version);
                            resolveErrorMessage.AppendLine();
                        }
                    }
                    else
                    {
                        resolveErrorMessage.AppendFormat("No version of package {0} has been determined to be available.", dupName);
                        resolveErrorMessage.AppendLine();
                        if (toRemove.Count() > 0)
                        {
                            resolveErrorMessage.AppendFormat("If there were any references to {0}, they may have been removed from consideration by the following packages being discarded:", dupName);
                            resolveErrorMessage.AppendLine();
                            foreach (var removed in toRemove)
                            {
                                resolveErrorMessage.AppendFormat("\t{0}", removed.FullName);
                                resolveErrorMessage.AppendLine();
                            }
                        }
                        resolveErrorMessage.AppendFormat("Please add an explicit dependency to (a version of) the {0} package either in your master package or one of its dependencies.", dupName);
                        resolveErrorMessage.AppendLine();
                    }
                    throw new Exception(resolveErrorMessage.ToString());
                }
            }

            // ensure that all packages with a single version in the definition files, does not have a command line override
            // that refers to a completely different version
            foreach (var uniquePkgName in uniquePackageNames)
            {
                foreach (var versionSpecifier in packageVersionSpecifiers)
                {
                    if (!versionSpecifier.Contains(uniquePkgName))
                    {
                        continue;
                    }

                    var versionFromDefinition = packageDefinitions.First(item => item.Name == uniquePkgName).Version;
                    if (versionSpecifier[1] != versionFromDefinition)
                    {
                        var noMatchMessage = new System.Text.StringBuilder();
                        noMatchMessage.AppendFormat("Command line version specified, {0}, could not resolve to one of the available versions of package {1}:", versionSpecifier[1], uniquePkgName);
                        noMatchMessage.AppendLine();
                        noMatchMessage.AppendFormat("\t{0}", versionFromDefinition);
                        noMatchMessage.AppendLine();
                        throw new Exception(noMatchMessage.ToString());
                    }
                }
            }

            if (enforceBamAssemblyVersions)
            {
                // for all packages that make up this assembly, ensure that their requirements on the version of the Bam
                // assemblies are upheld, prior to compiling the code
                foreach (var pkgDefn in packageDefinitions)
                {
                    pkgDefn.ValidateBamAssemblyRequirements();
                }
            }

            Graph.Instance.SetPackageDefinitions(packageDefinitions);
        }

        /// <summary>
        /// Compile the package assembly, using all the source files from the dependent packages.
        /// Throws Bam.Core.Exceptions if package compilation fails.
        /// </summary>
        /// <param name="enforceBamAssemblyVersions">If set to <c>true</c> enforce bam assembly versions. Default is true.</param>
        /// <param name="enableClean">If set to <c>true</c> cleaning the build root is allowed. Default is true.</param>
        public static void
        CompilePackageAssembly(
            bool enforceBamAssemblyVersions = true,
            bool enableClean = true)
        {
            // validate build root
            if (null == Graph.Instance.BuildRoot)
            {
                throw new Exception("Build root has not been specified");
            }

            var gatherSourceProfile = new TimeProfile(ETimingProfiles.GatherSource);
            gatherSourceProfile.StartProfile();

            IdentifyAllPackages(enforceBamAssemblyVersions: enforceBamAssemblyVersions);

            var cleanFirst = CommandLineProcessor.Evaluate(new Options.CleanFirst());
            if (enableClean && cleanFirst && System.IO.Directory.Exists(Graph.Instance.BuildRoot))
            {
                Log.Info("Deleting build root '{0}'", Graph.Instance.BuildRoot);
                try
                {
                    // make sure no files are read-only, which may have happened as part of collation preserving file attributes
                    var dirInfo = new System.IO.DirectoryInfo(Graph.Instance.BuildRoot);
                    foreach (var file in dirInfo.EnumerateFiles("*", System.IO.SearchOption.AllDirectories))
                    {
                        file.Attributes &= ~System.IO.FileAttributes.ReadOnly;
                    }

                    System.IO.Directory.Delete(Graph.Instance.BuildRoot, true);
                }
                catch (System.IO.IOException ex)
                {
                    Log.Info("Failed to delete build root, because {0}. Continuing", ex.Message);
                }
            }

            BuildModeUtilities.ValidateBuildModePackage();

            gatherSourceProfile.StopProfile();

            var assemblyCompileProfile = new TimeProfile(ETimingProfiles.AssemblyCompilation);
            assemblyCompileProfile.StartProfile();

            // assembly is written to the build root
            var cachedAssemblyPathname = System.IO.Path.Combine(Graph.Instance.BuildRoot, ".CachedPackageAssembly");
            cachedAssemblyPathname = System.IO.Path.Combine(cachedAssemblyPathname, Graph.Instance.MasterPackage.Name) + ".dll";
            var hashPathName = System.IO.Path.ChangeExtension(cachedAssemblyPathname, "hash");

            var cacheAssembly = !CommandLineProcessor.Evaluate(new Options.DisableCacheAssembly());

            string compileReason = null;
            if (Graph.Instance.CompileWithDebugSymbols)
            {
                compileReason = "debug symbols were enabled";
            }
            else
            {
                if (cacheAssembly)
                {
                    // gather source files
                    var filenames = new StringArray();
                    var strings = new System.Collections.Generic.SortedSet<string>();
                    foreach (var package in Graph.Instance.Packages)
                    {
#if BAM_V2
                        foreach (var scriptFile in package.GetScriptFiles(true))
#else
                        foreach (var scriptFile in package.GetScriptFiles())
#endif
                        {
                            filenames.Add(scriptFile);
                        }

                        foreach (var define in package.Definitions)
                        {
                            strings.Add(define);
                        }
                    }

                    // add/remove other definitions
                    strings.Add(VersionDefineForCompiler);
                    strings.Add(HostPlatformDefineForCompiler);
                    foreach (var feature in Features.PreprocessorDefines)
                    {
                        strings.Add(feature);
                    }

                    // TODO: what if other packages need more assemblies?
                    foreach (var assembly in Graph.Instance.MasterPackage.BamAssemblies)
                    {
                        var assemblyPath = System.IO.Path.Combine(Graph.Instance.ProcessState.ExecutableDirectory, assembly.Name) + ".dll";
                        var lastModifiedDate = System.IO.File.GetLastWriteTime(assemblyPath);
                        strings.Add(lastModifiedDate.ToString());
                    }

                    var compareResult = Hash.CompareAndUpdateHashFile(
                        hashPathName,
                        filenames,
                        strings
                    );
                    switch (compareResult)
                    {
                        case Hash.EHashCompareResult.HashFileDoesNotExist:
                            compileReason = "no previously compiled package assembly exists";
                            break;

                        case Hash.EHashCompareResult.HashesAreDifferent:
                            compileReason = "package source has changed since the last compile";
                            break;

                        case Hash.EHashCompareResult.HashesAreIdentical:
                            Graph.Instance.ScriptAssemblyPathname = cachedAssemblyPathname;
                            assemblyCompileProfile.StopProfile();
                            return;
                    }
                }
                else
                {
                    compileReason = "user has disabled package assembly caching";
                    // will not throw if the file doesn't exist
                    System.IO.File.Delete(hashPathName);
                }
            }

            // use the compiler in the current runtime version to build the assembly of packages
            var clrVersion = System.Environment.Version;

            Log.Detail("Compiling package assembly, CLR {0}{1}, because {2}.",
                clrVersion.ToString(),
                Graph.Instance.ProcessState.TargetFrameworkVersion != null ? (", targetting " + Graph.Instance.ProcessState.TargetFrameworkVersion) : string.Empty,
                compileReason);

            var outputAssemblyPath = cachedAssemblyPathname;

            // this will create the build root directory as necessary
            IOWrapper.CreateDirectory(System.IO.Path.GetDirectoryName(outputAssemblyPath));

            var projectPath = System.IO.Path.ChangeExtension(outputAssemblyPath, ".csproj");
            var project = new ProjectFile(false, projectPath);
            project.Write();

            string portableRID = System.String.Empty;
            var architecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString().ToLower();
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                portableRID = "win-" + architecture;
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                portableRID = "linux-" + architecture;
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                portableRID = "osx-" + architecture;
            }
            else
            {
                throw new Exception(
                    "Running on an unsupported OS: {0}",
                    System.Runtime.InteropServices.RuntimeInformation.OSDescription
                );
            }

            try
            {
                var args = new System.Text.StringBuilder();
                // publish is currently required in order to copy dependencies beside the assembly
                // don't use --force, because package may have already been restored
                // specifying a runtime ensures that all non-managed dependencies for NuGet packages with
                // platform specific runtimes are copied beside the assembly, and can be found by the assembly resolver
                args.AppendFormat("publish {0} ", projectPath);
                args.Append(System.String.Format("--runtime {0} ", portableRID));
                if (Graph.Instance.CompileWithDebugSymbols)
                {
                    args.Append("-c Debug ");
                }
                else
                {
                    args.Append("-c Release ");
                }
                args.AppendFormat("-o {0} ", System.IO.Path.GetDirectoryName(outputAssemblyPath));
                var dotNetResult = OSUtilities.RunExecutable(
                    "dotnet",
                    args.ToString()
                );
                Log.Info(dotNetResult.StandardOutput);
            }
            catch (RunExecutableException exception)
            {
                throw new Exception(
                    exception,
                    "Failed to build the packages:{0}{1}",
                    System.Environment.NewLine,
                    exception.Result.StandardOutput
                );
            }

            Log.DebugMessage("Written assembly to '{0}'", outputAssemblyPath);
            Graph.Instance.ScriptAssemblyPathname = outputAssemblyPath;

            assemblyCompileProfile.StopProfile();
        }

        /// <summary>
        /// Load the compiled package assembly.
        /// </summary>
        public static void
        LoadPackageAssembly()
        {
            var assemblyLoadProfile = new TimeProfile(ETimingProfiles.LoadAssembly);
            assemblyLoadProfile.StartProfile();

            System.Reflection.Assembly scriptAssembly = null;

            // don't scope the resolver with using, or resolving will fail!
            var resolver = new AssemblyResolver(Graph.Instance.ScriptAssemblyPathname);
            scriptAssembly = resolver.Assembly;
            Graph.Instance.ScriptAssembly = scriptAssembly;

            assemblyLoadProfile.StopProfile();
        }
    }

    // https://samcragg.wordpress.com/2017/06/30/resolving-assemblies-in-net-core/
    internal sealed class AssemblyResolver :
        System.IDisposable
    {
        private readonly Microsoft.Extensions.DependencyModel.Resolution.ICompilationAssemblyResolver assemblyResolver;
        private readonly Microsoft.Extensions.DependencyModel.DependencyContext dependencyContext;
        private readonly System.Runtime.Loader.AssemblyLoadContext loadContext;

        public AssemblyResolver(string path)
        {
            this.Assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            this.dependencyContext = Microsoft.Extensions.DependencyModel.DependencyContext.Load(this.Assembly);

            this.assemblyResolver = new Microsoft.Extensions.DependencyModel.Resolution.CompositeCompilationAssemblyResolver
                                    (new Microsoft.Extensions.DependencyModel.Resolution.ICompilationAssemblyResolver[]
            {
                new Microsoft.Extensions.DependencyModel.Resolution.AppBaseCompilationAssemblyResolver(System.IO.Path.GetDirectoryName(path)),
                new Microsoft.Extensions.DependencyModel.Resolution.ReferenceAssemblyPathResolver(),
                new Microsoft.Extensions.DependencyModel.Resolution.PackageCompilationAssemblyResolver()
            });

            this.loadContext = System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(this.Assembly);
            this.loadContext.Resolving += OnResolving;
        }

        public System.Reflection.Assembly Assembly { get; }

        public void Dispose()
        {
            this.loadContext.Resolving -= this.OnResolving;
        }

        private System.Reflection.Assembly OnResolving(
            System.Runtime.Loader.AssemblyLoadContext context,
            System.Reflection.AssemblyName name)
        {
            Log.DebugMessage("Resolving: {0}", name.FullName);
            bool NamesMatch(Microsoft.Extensions.DependencyModel.RuntimeLibrary runtime)
            {
                return string.Equals(runtime.Name, name.Name, System.StringComparison.OrdinalIgnoreCase);
            }

            Microsoft.Extensions.DependencyModel.RuntimeLibrary library =
                this.dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);
            if (library != null)
            {
                var wrapper = new Microsoft.Extensions.DependencyModel.CompilationLibrary(
                    library.Type,
                    library.Name,
                    library.Version,
                    library.Hash,
                    library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                    library.Dependencies,
                    library.Serviceable);
                // note that for NuGet packages with multiple platform specific assemblies
                // there will be more than one library.RuntimeAssemblyGroups
                // if there are native dependencies on these, and the native dynamic libraries
                // are not beside the managed assembly (they won't be if read from the NuGet cache, but will
                // be if published and targeted for a runtime), then loading will fail

                var assemblies = new System.Collections.Generic.List<string>();
                var result = this.assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
                if (assemblies.Count > 0)
                {
                    return this.loadContext.LoadFromAssemblyPath(assemblies[0]);
                }
                // note that this can silently fail
            }

            return null;
        }
    }
}

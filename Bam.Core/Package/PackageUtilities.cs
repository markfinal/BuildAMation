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
                return packageDefinitions.First(item => (item.Name == dupName) && (item.Version == masterDependency.Item2));
            }

            return null;
        }

        private static Array<PackageDefinition>
        PackagesToRemove(
            Array<PackageDefinition> toRemove,
            Array<PackageDefinition> packageDefinitions,
            PackageDefinition masterDefinitionFile)
        {
            var additionalToRemove = new Array<PackageDefinition>();
            foreach (var r in toRemove)
            {
                foreach (var p in packageDefinitions)
                {
                    if (p.Parents.Contains(r))
                    {
                        p.Parents.Remove(r);
                    }
                    if (0 == p.Parents.Count() && p != masterDefinitionFile && !toRemove.Contains(p))
                    {
                        additionalToRemove.AddUnique(p);
                    }
                }
            }
            if (additionalToRemove.Count() > 0)
            {
                // recurse
                additionalToRemove.AddRangeUnique(PackagesToRemove(additionalToRemove, packageDefinitions, masterDefinitionFile));
            }
            return additionalToRemove;
        }

        private static void
        EnqueuePackageRepositoryToVisit(
            System.Collections.Generic.LinkedList<System.Tuple<string,PackageDefinition>> reposToVisit,
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
                // visited parent already? ignore
                if (Graph.Instance.PackageRepositories.Any(item => repoPath.StartsWith(item)))
                {
                    return;
                }
            }
            // already planned to visit? ignore
            if (reposToVisit.Any(item => item.Item1 == repoPath))
            {
                return;
            }
            // new path is a parent path of a repo waiting to be viewed? replace all children with the parent (as it's a recursive search)
            if (reposToVisit.Any(item => item.Item1.StartsWith(repoPath)))
            {
                foreach (var repo in reposToVisit.Where(item => item.Item1.StartsWith(repoPath)).ToList())
                {
                    reposToVisit.Remove(repo);
                }
            }
            reposToVisit.AddLast(System.Tuple.Create<string, PackageDefinition>(repoPath, sourcePackageDefinition));
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
            foreach (var repo in Graph.Instance.PackageRepositories)
            {
                EnqueuePackageRepositoryToVisit(packageRepos, repo, null);
            }

            var masterDefinitionFile = GetMasterPackage();
            foreach (var repo in masterDefinitionFile.PackageRepositories)
            {
                EnqueuePackageRepositoryToVisit(packageRepos, repo, masterDefinitionFile);
            }

            // read the definition files of any package found in the package roots
            var candidatePackageDefinitions = new Array<PackageDefinition>();
            candidatePackageDefinitions.Add(masterDefinitionFile);
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
                var candidatePackageDirs = System.IO.Directory.GetDirectories(repo, BamSubFolder, System.IO.SearchOption.AllDirectories);

                Graph.Instance.PackageRepositories.Add(repo);

                foreach (var bamDir in candidatePackageDirs)
                {
                    var packageDir = System.IO.Path.GetDirectoryName(bamDir);
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
                        EnqueuePackageRepositoryToVisit(packageRepos, newRepo, definitionFile);
                    }
                }
            }

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
                var toRemove = new Array<PackageDefinition>();

                foreach (var dupName in duplicatePackageNames)
                {
                    var duplicates = packageDefinitions.Where(item => item.Name == dupName);
                    var resolvedDuplicate = TryToResolveDuplicate(masterDefinitionFile, dupName, duplicates, packageDefinitions, packageVersionSpecifiers, toRemove);
                    if (null != resolvedDuplicate)
                    {
                        continue;
                    }

                    // try removing any packages that have already been resolved
                    // which can remove additional packages (recursive check) because they had been added solely by those we are just about to remove
                    packageDefinitions.RemoveAll(PackagesToRemove(toRemove, packageDefinitions, masterDefinitionFile));
                    packageDefinitions.RemoveAll(toRemove);

                    // and if that has reduced the duplicates for this package down to a single version, we're good to carry on
                    var numDuplicates = duplicates.Count();
                    if (1 == numDuplicates)
                    {
                        toRemove.Clear();
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

                // finally, clean up the package definition list to use, with all those that need to be deleted
                packageDefinitions.RemoveAll(toRemove);
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

        private static string
        GetPackageHash(
            StringArray sourceCode,
            StringArray definitions,
            Array<BamAssemblyDescription> bamAssemblies)
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
            foreach (var assembly in bamAssemblies)
            {
                var assemblyPath = System.IO.Path.Combine(Graph.Instance.ProcessState.ExecutableDirectory, assembly.Name) + ".dll";
                var lastModifiedDate = System.IO.File.GetLastWriteTime(assemblyPath);
                hashCode ^= lastModifiedDate.GetHashCode();
            }
            var hash = hashCode.ToString();
            return hash;
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

            var definitions = new StringArray();

            // gather source files
            var sourceCode = new StringArray();
            int packageIndex = 0;
            foreach (var package in Graph.Instance.Packages)
            {
                Log.DebugMessage("{0}: '{1}' @ '{2}'", packageIndex, package.Version, (package.PackageRepositories.Count > 0) ? package.PackageRepositories[0] : "Not in a repository");

                // to compile with debug information, you must compile the files
                // to compile without, we need to file contents to hash the source
                if (Graph.Instance.CompileWithDebugSymbols)
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

            // add/remove other definitions
            definitions.Add(VersionDefineForCompiler);
            definitions.Add(HostPlatformDefineForCompiler);
            definitions.AddRange(Features.PreprocessorDefines);
            definitions.Sort();

            gatherSourceProfile.StopProfile();

            var assemblyCompileProfile = new TimeProfile(ETimingProfiles.AssemblyCompilation);
            assemblyCompileProfile.StartProfile();

            // assembly is written to the build root
            var cachedAssemblyPathname = System.IO.Path.Combine(Graph.Instance.BuildRoot, ".CachedPackageAssembly");
            cachedAssemblyPathname = System.IO.Path.Combine(cachedAssemblyPathname, Graph.Instance.MasterPackage.Name) + ".dll";
            var hashPathName = System.IO.Path.ChangeExtension(cachedAssemblyPathname, "hash");
            string thisHashCode = null;

            var cacheAssembly = !CommandLineProcessor.Evaluate(new Options.DisableCacheAssembly());

            string compileReason = null;
            if (Graph.Instance.CompileWithDebugSymbols)
            {
                compileReason = "debug symbols were enabled";
            }
            else
            {
                // can an existing assembly be reused?
                thisHashCode = GetPackageHash(sourceCode, definitions, Graph.Instance.MasterPackage.BamAssemblies);
                if (cacheAssembly)
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
                                Graph.Instance.ScriptAssemblyPathname = cachedAssemblyPathname;

                                assemblyCompileProfile.StopProfile();
                                return;
                            }
                            else
                            {
                                compileReason = "package source has changed since the last compile";
                            }
                        }
                    }
                    else
                    {
                        compileReason = "no previously compiled package assembly exists";
                    }
                }
                else
                {
                    compileReason = "user has disabled package assembly caching";
                }
            }

            // use the compiler in the current runtime version to build the assembly of packages
            var clrVersion = System.Environment.Version;

            Log.Detail("Compiling package assembly, CLR {0}{1}, because {2}.",
                clrVersion.ToString(),
                Graph.Instance.ProcessState.TargetFrameworkVersion != null ? (", targetting " + Graph.Instance.ProcessState.TargetFrameworkVersion) : string.Empty,
                compileReason);

            var providerOptions = new System.Collections.Generic.Dictionary<string, string>();
            var compilerVersion = System.String.Format("v{0}.{1}", clrVersion.Major, clrVersion.Minor);
            providerOptions.Add("CompilerVersion", compilerVersion);

            if (Graph.Instance.ProcessState.RunningMono)
            {
                Log.DebugMessage("Compiling assembly for Mono");
            }

            using (var provider = new Microsoft.CSharp.CSharpCodeProvider(providerOptions))
            {
                var compilerParameters = new System.CodeDom.Compiler.CompilerParameters();
                compilerParameters.TreatWarningsAsErrors = true;
                compilerParameters.WarningLevel = 4;
                compilerParameters.GenerateExecutable = false;
                compilerParameters.GenerateInMemory = false;

                if (Graph.Instance.CompileWithDebugSymbols)
                {
                    compilerParameters.OutputAssembly = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Graph.Instance.MasterPackage.Name) + ".dll";
                }
                else
                {
                    compilerParameters.OutputAssembly = cachedAssemblyPathname;
                }

                var compilerOptions = "/checked+ /unsafe-";
                if (Graph.Instance.CompileWithDebugSymbols)
                {
                    compilerParameters.IncludeDebugInformation = true;
                    compilerOptions += " /optimize-";
                }
                else
                {
                    compilerOptions += " /optimize+";
                }
                compilerOptions += " /platform:anycpu";

                // define strings
                compilerOptions += " /define:" + definitions.ToString(';');

                compilerParameters.CompilerOptions = compilerOptions;

                if (provider.Supports(System.CodeDom.Compiler.GeneratorSupport.Resources))
                {
                    // Bam assembly
                    // TODO: Q: why is it only for the master package? Why not all of them, which may have additional dependencies?
                    foreach (var assembly in Graph.Instance.MasterPackage.BamAssemblies)
                    {
                        var assemblyFileName = System.String.Format("{0}.dll", assembly.Name);
                        var assemblyPathName = System.IO.Path.Combine(Graph.Instance.ProcessState.ExecutableDirectory, assemblyFileName);
                        compilerParameters.ReferencedAssemblies.Add(assemblyPathName);
                    }

                    // DotNet assembly
                    foreach (var desc in Graph.Instance.MasterPackage.DotNetAssemblies)
                    {
                        var assemblyFileName = System.String.Format("{0}.dll", desc.Name);
                        compilerParameters.ReferencedAssemblies.Add(assemblyFileName);
                    }

                    if (Graph.Instance.ProcessState.RunningMono)
                    {
                        compilerParameters.ReferencedAssemblies.Add("Mono.Posix.dll");
                    }
                }
                else
                {
                    throw new Exception("C# compiler does not support Resources");
                }

                // this will create the build root directory as necessary
                IOWrapper.CreateDirectory(System.IO.Path.GetDirectoryName(compilerParameters.OutputAssembly));

                var results = Graph.Instance.CompileWithDebugSymbols ?
                    provider.CompileAssemblyFromFile(compilerParameters, sourceCode.ToArray()) :
                    provider.CompileAssemblyFromSource(compilerParameters, sourceCode.ToArray());

                if (results.Errors.HasErrors || results.Errors.HasWarnings)
                {
                    var message = new System.Text.StringBuilder();
                    message.AppendFormat("Failed to compile package '{0}'. There are {1} errors.", Graph.Instance.MasterPackage.FullName, results.Errors.Count);
                    message.AppendLine();
                    foreach (System.CodeDom.Compiler.CompilerError error in results.Errors)
                    {
                        message.AppendFormat("\t{0}({1}): {2} {3}", error.FileName, error.Line, error.ErrorNumber, error.ErrorText);
                        message.AppendLine();
                    }
                    if (!Graph.Instance.CompileWithDebugSymbols)
                    {
                        message.AppendLine();
                        ICommandLineArgument debugOption = new Options.UseDebugSymbols();
                        message.AppendFormat("Use the {0}/{1} command line option with bam for more accurate error messages.", debugOption.LongName, debugOption.ShortName);
                        message.AppendLine();
                    }
                    message.AppendLine();
                    ICommandLineArgument createDebugProjectOption = new Options.CreateDebugProject();
                    message.AppendFormat("Use the {0}/{1} command line option with bam to create an editable IDE project containing the build scripts.", createDebugProjectOption.LongName, createDebugProjectOption.ShortName);
                    message.AppendLine();
                    throw new Exception(message.ToString());
                }

                if (!Graph.Instance.CompileWithDebugSymbols)
                {
                    if (cacheAssembly)
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
                Graph.Instance.ScriptAssemblyPathname = compilerParameters.OutputAssembly;
            }

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

            try
            {
                // this code works from an untrusted location, and debugging IS available when
                // the pdb (.NET)/mdb (Mono) resides beside the assembly
                byte[] asmBytes = System.IO.File.ReadAllBytes(Graph.Instance.ScriptAssemblyPathname);
                if (Graph.Instance.CompileWithDebugSymbols)
                {
                    var debugInfoFilename = Graph.Instance.ProcessState.RunningMono ?
                        Graph.Instance.ScriptAssemblyPathname + ".mdb" :
                        System.IO.Path.ChangeExtension(Graph.Instance.ScriptAssemblyPathname, ".pdb");
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
                Log.ErrorMessage("Could not find assembly '{0}'", Graph.Instance.ScriptAssemblyPathname);
                throw exception;
            }
            catch (System.Exception exception)
            {
                throw exception;
            }

            Graph.Instance.ScriptAssembly = scriptAssembly;

            assemblyLoadProfile.StopProfile();
        }
    }
}

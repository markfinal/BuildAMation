#region License
// Copyright (c) 2010-2019, Mark Final
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
            Graph.Instance.SkipPackageSourceDownloads = true;
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
        /// Utility method for adding a dependent package to the master package.
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

            Graph.Instance.SkipPackageSourceDownloads = true;
            var masterPackage = GetMasterPackage();
            if (!default((string name, string version, bool? isDefault)).Equals(masterPackage.Dependents.FirstOrDefault(item => item.name.Equals(packageName, System.StringComparison.Ordinal) && item.version.Equals(packageVersion, System.StringComparison.Ordinal))))
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

            (string name, string version, bool? isDefault) newDepTuple = (packageName, packageVersion, null);
            masterPackage.Dependents.Add(newDepTuple);
            // TODO: this is unfortunate having to write the file in order to use it with IdentifyAllPackages
            masterPackage.Write();

            // validate that the addition is ok
            try
            {
                PackageUtilities.IdentifyAllPackages(false);
            }
            catch (Exception exception)
            {
                masterPackage.Dependents.Remove(newDepTuple);
                masterPackage.Write();
                throw new Exception(exception, "Failed to add dependent. Are all necessary package repositories specified?");
            }
        }


        /// <summary>
        /// Utility method for setting the default version of a dependent package in the master package.
        /// </summary>
        public static void
        SetDependentDefaultVersion()
        {
            var packageNameArgument = new Options.PackageName();
            var packageName = CommandLineProcessor.Evaluate(packageNameArgument);
            if (null == packageName)
            {
                throw new Exception($"No name was defined. Use {(packageNameArgument as ICommandLineArgument).LongName} on the command line to specify it.");
            }

            Graph.Instance.SkipPackageSourceDownloads = true;
            var masterPackage = GetMasterPackage();
            if (!masterPackage.Dependents.Any(item => item.name.Equals(packageName, System.StringComparison.Ordinal)))
            {
                throw new Exception($"Package dependency {packageName} is not present");
            }

            var packageVersionArgument = new Options.PackageVersion();
            var packageVersion = CommandLineProcessor.Evaluate(packageVersionArgument);
            if (null == packageVersion)
            {
                throw new Exception($"No version was defined. Use {(packageVersionArgument as ICommandLineArgument).LongName} on the command line to specify it.");
            }

            // set the new default version
            var newDefaultVersion = masterPackage.Dependents.FirstOrDefault(item => item.name.Equals(packageName, System.StringComparison.Ordinal) && item.version.Equals(packageVersion, System.StringComparison.Ordinal));
            if (default((string name, string version, bool? isDefault)).Equals(newDefaultVersion))
            {
                throw new Exception($"Package dependency {packageName}-{packageVersion} is not present");
            }
            masterPackage.Dependents.Remove(newDefaultVersion);
            masterPackage.Dependents.Add((packageName, packageVersion, true));

            // all other versions are not default
            var nonDefaultVersions = masterPackage.Dependents.Where(item => item.name.Equals(packageName, System.StringComparison.Ordinal) && !item.version.Equals(packageVersion, System.StringComparison.Ordinal)).ToList();
            foreach (var dep in nonDefaultVersions)
            {
                masterPackage.Dependents.Remove(dep);
                masterPackage.Dependents.Add((dep.name, dep.version, null));
            }

            masterPackage.Write();
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
        public static string HostPlatformDefineForCompiler => Platform.ToString(OSUtilities.CurrentPlatform, '\0', "BAM_HOST_", true);

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

            var parentDir = System.IO.Path.GetDirectoryName(workingDir);
            var repository = Graph.Instance.AddPackageRepository(parentDir);
            var masterDefinitionFile = repository.FindPackage(GetPackageDefinitionPathname(workingDir));
            if (null == masterDefinitionFile)
            {
                // could not find the master package in either an unstructured, or structured (packages) repository
                // try in the tests folder if this is part of a structured repository
                if (repository.HasTests)
                {
                    repository.ScanTestPackages();
                    masterDefinitionFile = repository.FindPackage(GetPackageDefinitionPathname(workingDir));
                }
                if (null == masterDefinitionFile)
                {
                    throw new Exception("Unable to locate master package in any repository");
                }
            }
            // the package will have been read already, so re-read it in the context of a master package
            masterDefinitionFile.ReReadAsMaster();

            return masterDefinitionFile;
        }

        private static void
        ProcessPackagesIntoTree(
            System.Collections.Generic.Queue<(string name, string version, Array<PackageTreeNode> parents)> queue,
            System.Collections.Generic.Dictionary<(string name, string version), PackageTreeNode> packageMap)
        {
            while (queue.Any())
            {
                var defn = queue.Dequeue();
                var defnKey = (defn.name, defn.version);
                Log.DebugMessage($"Considering package {defn.name}-{defn.version} and its dependents");

                PackageDefinition defFile;
                PackageTreeNode packageNode;
                if (packageMap.ContainsKey(defnKey) && packageMap[defnKey].Definition != null)
                {
                    packageNode = packageMap[defnKey];
                    defFile = packageNode.Definition;
                }
                else
                {
                    PackageDefinition
                    findPackageInRepositories(
                        (string name, string version) packageDesc)
                    {
                        foreach (var repo in Graph.Instance.PackageRepositories)
                        {
                            var definition = repo.FindPackage(packageDesc);
                            if (null != definition)
                            {
                                Log.DebugMessage($"\tFound {packageDesc.name}-{packageDesc.version} in repo {repo.RootPath}");
                                return definition;
                            }
                        }
                        return null;
                    }

                    defFile = findPackageInRepositories(defnKey);
                    if (null != defFile)
                    {
                        packageNode = new PackageTreeNode(defFile);
                    }
                    else
                    {
                        packageNode = new PackageTreeNode(defnKey.name, defnKey.version);
                    }

                    if (packageMap.ContainsKey(defnKey))
                    {
                        // since a placeholder is being replaced
                        System.Diagnostics.Debug.Assert(null == packageMap[defnKey].Definition);
                        packageMap[defnKey].RemoveFromParents();
                        packageMap.Remove(defnKey);
                    }
                    packageMap.Add(defnKey, packageNode);
                    if (defn.parents != null)
                    {
                        foreach (var parent in defn.parents)
                        {
                            parent.AddChild(packageNode);
                        }
                    }
                }

                if (null == defFile)
                {
                    // package not found, defer this for later
                    continue;
                }

                foreach (var (name, version, isDefault) in defFile.Dependents)
                {
                    var key = (name, version);
                    if (!packageMap.ContainsKey(key))
                    {
                        var match = queue.FirstOrDefault(item => item.name == key.name && item.version == key.version);
                        if (default((string name, string version, Array<PackageTreeNode> parents)).Equals(match))
                        {
                            Log.DebugMessage($"\tQueuing up {name}-{version}...");
                            queue.Enqueue((key.name, key.version, new Array<PackageTreeNode>(packageNode)));
                        }
                        else
                        {
                            match.parents.Add(packageNode);
                        }
                        continue;
                    }
                    Log.DebugMessage($"\tPackage {name}-{version} already encountered");
                    var depNode = packageMap[key];
                    packageNode.AddChild(depNode);
                }
            }
        }

        // this is breadth-first traversal, so that the details of packages are explored
        // at the highest level, not on the first encounter in a depth-first search
        private static void
        DumpTreeInternal(
            PackageTreeNode node,
            int depth,
            System.Collections.Generic.Dictionary<PackageTreeNode, int> encountered,
            Array<PackageTreeNode> displayed)
        {
            if (!encountered.ContainsKey(node))
            {
                encountered.Add(node, depth);
            }
            foreach (var child in node.Children)
            {
                if (!encountered.ContainsKey(child))
                {
                    encountered.Add(child, depth + 1);
                }
            }

            var indent = new string('\t', depth);
            if (null != node.Definition)
            {
                Log.DebugMessage($"{indent}{node.Definition.FullName}");
            }
            else
            {
                Log.DebugMessage($"{indent}{node.Name}-{node.Version} ***** undiscovered *****");
            }
            if (encountered[node] < depth)
            {
                return;
            }
            if (displayed.Contains(node))
            {
                return;
            }
            else
            {
                displayed.Add(node);
            }
            foreach (var child in node.Children)
            {
                DumpTreeInternal(child, depth + 1, encountered, displayed);
            }
        }

        private static void
        DumpTree(
            PackageTreeNode node)
        {
            Log.DebugMessage("-- Dumping the package tree");
            var encountered = new System.Collections.Generic.Dictionary<PackageTreeNode, int>();
            var displayed = new Array<PackageTreeNode>();
            DumpTreeInternal(node, 0, encountered, displayed);
            Log.DebugMessage("-- Dumping the package tree - DONE");
        }

        private static void
        ResolveDuplicatePackages(
            PackageTreeNode rootNode,
            PackageDefinition masterDefinitionFile)
        {
            var duplicatePackageNames = rootNode.DuplicatePackageNames;
            if (duplicatePackageNames.Any())
            {
                var packageVersionSpecifiers = CommandLineProcessor.Evaluate(new Options.PackageDefaultVersion());

                Log.DebugMessage("Duplicate packages found");
                foreach (var name in duplicatePackageNames)
                {
                    Log.DebugMessage($"\tResolving duplicates for {name}...");
                    var duplicates = rootNode.DuplicatePackages(name);

                    // package version specifiers take precedence
                    var specifierMatch = packageVersionSpecifiers.FirstOrDefault(item => item.First().Equals(name));
                    System.Collections.Generic.IEnumerable<PackageTreeNode> duplicatesToRemove = null;
                    if (null != specifierMatch)
                    {
                        Log.DebugMessage($"\t\tCommand line package specifier wants version {specifierMatch.Last()}");
                        duplicatesToRemove = duplicates.Where(item =>
                            item.Version != specifierMatch.Last()
                        );
                        foreach (var toRemove in duplicatesToRemove)
                        {
                            toRemove.RemoveFromParents();
                        }
                    }
                    else
                    {
                        // does the master package specify a default for this package?
                        var masterPackageMatch = masterDefinitionFile.Dependents.FirstOrDefault(item => item.name == name && item.isDefault.HasValue && item.isDefault.Value);
                        if (!default((string name, string version, bool? isDefault)).Equals(masterPackageMatch))
                        {
                            Log.DebugMessage($"\t\tMaster package specifies version {masterPackageMatch.version} is default");
                            duplicatesToRemove = duplicates.Where(item =>
                                item.Version != masterPackageMatch.version
                            );
                            foreach (var toRemove in duplicatesToRemove.ToList())
                            {
                                toRemove.RemoveFromParents();
                            }
                        }
                    }

                    // and if that has reduced the duplicates for this package down to a single version, we're good to carry on
                    duplicates = rootNode.DuplicatePackages(name);
                    var numDuplicates = duplicates.Count();
                    if (1 == numDuplicates)
                    {
                        continue;
                    }

                    // otherwise, error
                    var resolveErrorMessage = new System.Text.StringBuilder();
                    if (numDuplicates > 0)
                    {
                        resolveErrorMessage.AppendFormat("Unable to resolve to a single version of package {0}. Use --{0}.version=<version> to resolve.", name);
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
                        resolveErrorMessage.AppendFormat("No version of package {0} has been determined to be available.", name);
                        resolveErrorMessage.AppendLine();
                        if (duplicatesToRemove != null && duplicatesToRemove.Any())
                        {
                            resolveErrorMessage.AppendFormat("If there were any references to {0}, they may have been removed from consideration by the following packages being discarded:", name);
                            resolveErrorMessage.AppendLine();
                            foreach (var removed in duplicatesToRemove)
                            {
                                resolveErrorMessage.AppendFormat("\t{0}", removed.Definition.FullName);
                                resolveErrorMessage.AppendLine();
                            }
                        }
                        resolveErrorMessage.AppendFormat("Please add an explicit dependency to (a version of) the {0} package either in your master package or one of its dependencies.", name);
                        resolveErrorMessage.AppendLine();
                    }
                    throw new Exception(resolveErrorMessage.ToString());
                }
            }
        }

        private static void
        InjectExtraModules(
            PackageDefinition intoPackage)
        {
            var injectPackages = CommandLineProcessor.Evaluate(new Options.InjectDefaultPackage());
            if (null != injectPackages)
            {
                foreach (var injected in injectPackages)
                {
                    var name = injected[0];
                    string version = null;
                    if (injected.Count > 1)
                    {
                        version = injected[1].TrimStart(new[] { '-' }); // see regex in InjectDefaultPackage
                    }
                    var is_default = true;
                    intoPackage.Dependents.AddUnique((name, version, is_default));
                }
            }
        }

        /// <summary>
        /// Scan though all package repositories for all package dependencies, and resolve any duplicate package names
        /// by either data in the package definition file, or on the command line, by specifying a particular version to
        /// use. The master package definition file is the source of disambiguation for package versions.
        /// </summary>
        /// <param name="allowDuplicates">If set to <c>true</c> allow duplicates. Used to show the full extent of the definition file.</param>
        /// <param name="enforceBamAssemblyVersions">If set to <c>true</c> enforce bam assembly versions.</param>
        public static PackageTreeNode
        IdentifyAllPackages(
            bool allowDuplicates = false,
            bool enforceBamAssemblyVersions = true)
        {
            var masterDefinitionFile = GetMasterPackage();

            // inject any packages from the command line into the master definition file
            // and these will be defaults
            InjectExtraModules(masterDefinitionFile);

            System.Collections.Generic.Dictionary<(string name, string version), PackageTreeNode> packageMap = new System.Collections.Generic.Dictionary<(string name, string version), PackageTreeNode>();

            (string name, string version) masterDefn = (masterDefinitionFile.Name, masterDefinitionFile.Version);
            System.Collections.Generic.Queue<(string name, string version, Array<PackageTreeNode> parents)> queue = new System.Collections.Generic.Queue<(string name, string version, Array<PackageTreeNode> parents)>();
            queue.Enqueue((masterDefn.name, masterDefn.version, null));

            Log.DebugMessage("-- Starting package dependency evaluation... --");

            var firstCount = 0;
            ProcessPackagesIntoTree(queue, packageMap);
            var secondCount = packageMap.Count;

            var rootNode = packageMap.First(item => item.Key == masterDefn).Value;
            DumpTree(rootNode);

            if (!allowDuplicates)
            {
                // resolve duplicates before trying to find packages that weren't found
                // otherwise you may use package roots for packages that will be discarded
                ResolveDuplicatePackages(rootNode, masterDefinitionFile);
            }

            DumpTree(rootNode);

            var undiscovered = rootNode.UndiscoveredPackages;
            while (undiscovered.Any() && (firstCount != secondCount))
            {
                Log.DebugMessage($"{undiscovered.Count()} packages not found:");
                foreach (var package in undiscovered)
                {
                    Log.DebugMessage($"\t{package.Name}-{package.Version}");
                }
                var repoPaths = rootNode.PackageRepositoryPaths;
                Log.DebugMessage($"Implicit repo paths to add:");
                foreach (var path in repoPaths)
                {
                    Log.DebugMessage($"\t{path}");
                    Graph.Instance.AddPackageRepository(path, masterDefinitionFile);
                }

                foreach (var package in undiscovered)
                {
                    queue.Enqueue((package.Name, package.Version, new Array<PackageTreeNode>(package.Parents)));
                }

                firstCount = secondCount;
                ProcessPackagesIntoTree(queue, packageMap);
                secondCount = packageMap.Count;
                if (!allowDuplicates)
                {
                    ResolveDuplicatePackages(rootNode, masterDefinitionFile);
                }
                DumpTree(rootNode);

                undiscovered = rootNode.UndiscoveredPackages;
            }

            if (undiscovered.Any())
            {
                var message = new System.Text.StringBuilder();
                message.AppendLine("Some packages were not found in any repository:");
                foreach (var package in undiscovered)
                {
                    if (null != package.Version)
                    {
                        message.AppendLine($"\t{package.Name}-{package.Version}");
                    }
                    else
                    {
                        message.AppendLine($"\t{package.Name}");
                    }
                }
                message.AppendLine("Searched for in the following repositories:");
                foreach (var repo in Graph.Instance.PackageRepositories)
                {
                    message.AppendLine($"\t{repo.ToString()}");
                }
                throw new Exception(message.ToString());
            }

            Log.DebugMessage("-- Completed package dependency evaluation --");

            var packageDefinitions = rootNode.UniquePackageDefinitions;
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

            return rootNode;
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

            IdentifyAllPackages(
                enforceBamAssemblyVersions: enforceBamAssemblyVersions
            );

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
                        foreach (var scriptFile in package.GetScriptFiles(true))
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

            try
            {
                var args = new System.Text.StringBuilder();
                args.AppendFormat("build {0} ", projectPath);
                if (Graph.Instance.CompileWithDebugSymbols)
                {
                    args.Append("-c Debug ");
                }
                else
                {
                    args.Append("-c Release ");
                }
                args.AppendFormat("-o {0} ", System.IO.Path.GetDirectoryName(outputAssemblyPath));
                args.Append("-v quiet ");
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
                return runtime.Name.Equals(name.Name, System.StringComparison.OrdinalIgnoreCase);
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
                if (assemblies.Any())
                {
                    return this.loadContext.LoadFromAssemblyPath(assemblies[0]);
                }
                // note that this can silently fail
            }

            return null;
        }
    }
}

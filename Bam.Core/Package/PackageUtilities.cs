#region License
// Copyright 2010-2014 Mark Final
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
#endregion
namespace Bam.Core
{
    public static class PackageUtilities
    {
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

        public static PackageIdentifier
        IsPackageDirectory(
            string path,
            out bool isWellDefined)
        {
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
        }

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

        public static void
        IdentifyMainAndDependentPackages(
            bool resolveToSinglePackageVersion,
            bool allowUndefinedPackages)
        {
            var buildList = new PackageBuildList();

            // find the working directory package
            {
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

                State.DependentPackageList.Add(id);
                buildList.Add(new PackageBuild(id));
            }

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
                definitionFile.Read(true);
                id.Definition = definitionFile;

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

            if (resolveToSinglePackageVersion)
            {
                // can we resolve down to a single package?
                foreach (var build in buildList)
                {
                    if (build.Versions.Count > 1)
                    {
                        string defaultVersion;
                        if (!State.Has("PackageDefaultVersions", build.Name.ToLower()))
                        {
                            var defaultVersions = new StringArray();
                            foreach (var id in build.Versions)
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
                        foreach (var version in build.Versions)
                        {
                            if (version.Version == defaultVersion)
                            {
                                build.SelectedVersion = version;
                                found = true;
                                break;
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
        }

        private static string
        GetPackageHash(
            StringArray sourceCode,
            StringArray definitions,
            StringArray opusAssemblies)
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
            foreach (var assemblyPath in opusAssemblies)
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

            if (!System.IO.Directory.Exists(State.BuildRoot))
            {
                System.IO.Directory.CreateDirectory(State.BuildRoot);
            }

            var gatherSourceProfile = new TimeProfile(ETimingProfiles.GatherSource);
            gatherSourceProfile.StartProfile();

            IdentifyMainAndDependentPackages(true, false);

            var mainPackage = State.PackageInfo.MainPackage;

            Log.DebugMessage("Package is '{0}' in '{1}", mainPackage.Identifier.ToString("-"), mainPackage.Identifier.Root);

            BuilderUtilities.SetBuilderPackage();

            // Create resource file containing package information
            var resourceFilePathName = PackageListResourceFile.WriteResourceFile();

            var definitions = new StringArray();

            // gather source files
            var sourceCode = new StringArray();
            int packageIndex = 0;
            foreach (var package in State.PackageInfo)
            {
                var id = package.Identifier;
                Log.DebugMessage("{0}: '{1}' @ '{2}'", packageIndex, id.ToString("-"), id.Root);

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
            cachedAssemblyPathname = System.IO.Path.Combine(cachedAssemblyPathname, mainPackage.Name) + ".dll";
            var hashPathName = System.IO.Path.ChangeExtension(cachedAssemblyPathname, "hash");
            string thisHashCode = null;

            if (!State.CompileWithDebugSymbols)
            {
                // can an existing assembly be reused?
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
            }

            Log.Detail("Compiling package assembly");

            var providerOptions = new System.Collections.Generic.Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v3.5");

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
                    compilerParameters.OutputAssembly = System.IO.Path.Combine(System.IO.Path.GetTempPath(), mainPackage.Name) + ".dll";
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
                    foreach (var opusAssembly in mainPackage.Identifier.Definition.BamAssemblies)
                    {
                        var assemblyFileName = System.String.Format("{0}.dll", opusAssembly);
                        var assemblyPathName = System.IO.Path.Combine(State.ExecutableDirectory, assemblyFileName);
                        compilerParameters.ReferencedAssemblies.Add(assemblyPathName);
                    }

                    // DotNet assembly
                    foreach (var desc in mainPackage.Identifier.Definition.DotNetAssemblies)
                    {
                        var assemblyFileName = System.String.Format("{0}.dll", desc.Name);
                        compilerParameters.ReferencedAssemblies.Add(assemblyFileName);
                    }

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
                    Log.ErrorMessage("Failed to compile package '{0}'. There are {1} errors.", mainPackage.FullName, results.Errors.Count);
                    foreach (System.CodeDom.Compiler.CompilerError error in results.Errors)
                    {
                        Log.ErrorMessage("\t{0}({1}): {2} {3}", error.FileName, error.Line, error.ErrorNumber, error.ErrorText);
                    }
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

        public static bool
        ExecutePackageAssembly()
        {
            // let's think about a new domain
            //            System.AppDomain domain = System.AppDomain.CreateDomain("tempDomain");

            //            System.Reflection.AssemblyName assemblyName = System.Reflection.AssemblyName.GetAssemblyName(State.DebugAssembly);
            //            System.Reflection.Assembly assembly = domain.Load(assemblyName);
            //            System.AppDomain.Unload(domain);

            var findBuildableModulesProfile = new TimeProfile(ETimingProfiles.IdentifyBuildableModules);
            findBuildableModulesProfile.StartProfile();

            var topLevelTypes = GetTopLevelModuleTypes();

            BuilderUtilities.CreateBuilderInstance();

            RegisterToolsetAttribute.RegisterAll();

            if (null == State.BuildPlatforms)
            {
                throw new Exception("No build platforms were specified");
            }
            if (null == State.BuildConfigurations)
            {
                throw new Exception("No build configurations were specified");
            }

            var dependencyGraph = new DependencyGraph();
            State.Set("System", "Graph", dependencyGraph);

            // add modules in for each target configured
            foreach (var platform in State.BuildPlatforms)
            {
                foreach (var configuration in State.BuildConfigurations)
                {
                    var baseTarget = BaseTarget.GetInstance(platform, configuration);
                    Log.DebugMessage("Added base target '{0}'", baseTarget);
                    foreach (var topLevelType in topLevelTypes)
                    {
                        dependencyGraph.AddTopLevelModule(topLevelType, baseTarget);
                    }
                }
            }

            findBuildableModulesProfile.StopProfile();

            dependencyGraph.PopulateGraph();

            var dependencyGraphExecutionProfile = new TimeProfile(ETimingProfiles.GraphExecution);
            dependencyGraphExecutionProfile.StartProfile();

            var buildManager = new BuildManager(dependencyGraph);
            State.BuildManager = buildManager;
            State.ReadOnly = true;
            var success = buildManager.Execute();

            dependencyGraphExecutionProfile.StopProfile();

            return success;
        }

        public static PackageInformation
        GetOwningPackage(
            object obj)
        {
            var objType = obj.GetType();
            TypeUtilities.CheckTypeDerivesFrom(objType, typeof(BaseModule));

            var packageName = objType.Namespace;
            var package = State.PackageInfo[packageName];
            return package;
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

        public static void
        ProcessLazyArguments(
            bool fatal)
        {
            if (State.LazyArguments.Count > 0)
            {
                var actions = ActionManager.ScriptActions;
                if ((null == actions) || (0 == actions.Count))
                {
                    var message = new System.Text.StringBuilder();
                    message.AppendLine("There are unprocessed arguments, but no actions to associate with them:");
                    foreach (var arg in State.LazyArguments.Keys)
                    {
                        message.AppendLine(arg);
                    }

                    if (fatal)
                    {
                        throw new Exception(message.ToString());
                    }
                    else
                    {
                        Log.Info(message.ToString());
                        return;
                    }
                }

                var lazyCommandsProcessed = new StringArray();
                foreach (var command in State.LazyArguments.Keys)
                {
                    foreach (var action in actions)
                    {
                        var iaction = action.Action;
                        var isThisCommand = false;
                        if (iaction is IActionCommandComparison)
                        {
                            isThisCommand = (iaction as IActionCommandComparison).Compare(iaction.CommandLineSwitch, command);
                        }
                        else
                        {
                            isThisCommand = (iaction.CommandLineSwitch == command);
                        }

                        if (isThisCommand)
                        {
                            if (iaction is IActionWithArguments)
                            {
                                (iaction as IActionWithArguments).AssignArguments(State.LazyArguments[command]);
                            }

                            if (!iaction.Execute())
                            {
                                throw new Exception("Action '{0}' failed", command);
                            }

                            lazyCommandsProcessed.Add(command);
                        }
                    }
                }

                // remove those that have been processed
                foreach (var command in lazyCommandsProcessed)
                {
                    State.LazyArguments.Remove(command);
                }

                lazyCommandsProcessed = null;
            }
        }

        public static void
        HandleUnprocessedArguments(
            bool fatal)
        {
            if (State.LazyArguments.Count > 0)
            {
                var message = new System.Text.StringBuilder();
                message.Append("Unrecognized command line arguments:\n");
                foreach (var command in State.LazyArguments.Keys)
                {
                    message.AppendFormat("\t'{0}'\n", command);
                }
                if (fatal)
                {
                    throw new Exception(message.ToString());
                }
                else
                {
                    Log.Info(message.ToString());
                }
            }
        }

        private static TypeArray
        GetTopLevelModuleTypes()
        {
            // TODO: not sure I like this; find the top level namespace another way
            // TODO: maybe add a Resource into the assembly to indicate the top level types?
            var topLevelNamespace = System.IO.Path.GetFileNameWithoutExtension(State.ScriptAssemblyPathname);
            var topLevelTypes = new TypeArray();
            var topLevelTypeNames = new StringArray();
            Log.DebugMessage("Searching for top level modules...");
            try
            {
                var assemblyTypes = State.ScriptAssembly.GetTypes();
                foreach (var assemblyType in assemblyTypes)
                {
                    if ((0 == System.String.Compare(assemblyType.Namespace, topLevelNamespace, false)) &&
                        !assemblyType.IsNested)
                    {
                        if (!typeof(IModule).IsAssignableFrom(assemblyType))
                        {
                            //Log.DebugMessage("\tIgnoring type '{0}' as it does not implement Bam.Core.IModule", assemblyType.ToString());
                            continue;
                        }

                        if (assemblyType.IsAbstract)
                        {
                            //Log.DebugMessage("\tIgnoring type '{0}' as it is abstract", assemblyType.ToString());
                            continue;
                        }

                        Log.DebugMessage("\tFound '{0}'", assemblyType.FullName);
                        topLevelTypes.Add(assemblyType);
                        topLevelTypeNames.Add(assemblyType.FullName);
                    }
                }

                // scan the top level types to see if they consist of any types of the fields in the others
                {
                    var tltToRemove = new TypeArray();
                    foreach (var topLevelType in topLevelTypes)
                    {
                        var fields = topLevelType.GetFields(System.Reflection.BindingFlags.Instance |
                                                            System.Reflection.BindingFlags.Public |
                                                            System.Reflection.BindingFlags.NonPublic);
                        foreach (var field in fields)
                        {
                            var t = field.FieldType;
                            if (topLevelTypes.Contains(t))
                            {
                                tltToRemove.Add(t);
                            }
                        }
                    }

                    foreach (var t in tltToRemove)
                    {
                        Log.DebugMessage("\tRemoving '{0}' as it is used as a field in another top level type", t.FullName);
                        topLevelTypeNames.Remove(t.FullName);
                        topLevelTypes.Remove(t);
                    }
                }
            }
            catch (System.Reflection.ReflectionTypeLoadException exception)
            {
                var loaderExceptions = exception.LoaderExceptions;
                Log.ErrorMessage("Loader exceptions");
                foreach (System.IO.FileLoadException loaderException in loaderExceptions)
                {
                    Log.ErrorMessage(loaderException.Message);
                    Log.ErrorMessage(loaderException.FusionLog);
                }
                throw exception;
            }
            catch (System.Exception exception)
            {
                throw exception;
            }

            if (0 == topLevelTypes.Count)
            {
                throw new Exception("Unable to locate any objects in namespace '{0}'", topLevelNamespace);
            }

            // now find the intersection of the specified build modules and the top-level modules found
            var buildModules = State.BuildModules;
            if (null != buildModules)
            {
                var filteredTopLevelTypes = new TypeArray();
                foreach (var buildModule in buildModules)
                {
                    var found = false;
                    foreach (var topLevelType in topLevelTypes)
                    {
                        if (buildModule == topLevelType.FullName)
                        {
                            Log.DebugMessage("Filtered '{0}'", buildModule);
                            filteredTopLevelTypes.Add(topLevelType);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        throw new Exception("Unable to locate specified module '{0}' in the list of module types for this package:\n{1}", buildModule, topLevelTypeNames.ToString('\n'));
                    }
                }

                topLevelTypes = filteredTopLevelTypes;
            }

            return topLevelTypes;
        }
    }
}

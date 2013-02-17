// <copyright file="PackageUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class PackageUtilities
    {
        public static string OpusVersionDefineForCompiler
        {
            get
            {
                System.Version coreVersion = State.OpusVersion;
                string coreVersionDefine = System.String.Format("OPUS_CORE_VERSION_{0}_{1}", coreVersion.Major, coreVersion.Minor);
                return coreVersionDefine;
            }
        }

        public static string OpusHostPlatformForCompiler
        {
            get
            {
                EPlatform platform = State.Platform;
                string platformString = Platform.ToString(platform, '\0', "OPUS_HOST_", true);
                return platformString;
            }
        }

        private static void GetPackageDetailsFromPath(string path, out string name, out string version)
        {
            string[] directories = path.Split(new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });
            if (directories.Length < 2)
            {
                throw new Exception("Cannot determine package name and version from the path '{0}'. Expected format is 'root{1}packagename{1}version'", path, System.IO.Path.DirectorySeparatorChar);
            }

            name = directories[directories.Length - 2];
            version = directories[directories.Length - 1];
        }

        public static PackageIdentifier IsPackageDirectory(string path,
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

            System.IO.DirectoryInfo parentDir = System.IO.Directory.GetParent(path);
            if (null == parentDir)
            {
                Log.DebugMessage("No parent directory");
                return null;
            }
            System.IO.DirectoryInfo parentParentDir = System.IO.Directory.GetParent(parentDir.FullName);
            if (null == parentParentDir)
            {
                Log.DebugMessage("No parent of parent directory");
                return null;
            }
            string packageRoot = parentParentDir.FullName;

            string basePackageFilename = System.IO.Path.Combine(path, packageName);
            string scriptFilename = basePackageFilename + ".cs";
            string xmlFilename = basePackageFilename + ".xml";
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

            if (!State.PackageRoots.Contains(packageRoot))
            {
                State.PackageRoots.Add(packageRoot);
            }

            PackageIdentifier id = new PackageIdentifier(packageName, packageVersion);
            return id;
        }

        public static string PackageDefinitionPathName(PackageIdentifier id)
        {
            if (id.Root != null)
            {
                string packageDirectory = id.Path;
                string definitionFileName = id.Name + ".xml";
                string definitionPathName = System.IO.Path.Combine(packageDirectory, definitionFileName);
                return definitionPathName;
            }
            else
            {
                return null;
            }
        }

        public static void IdentifyMainPackageOnly()
        {
            // find the working directory package
            bool isWorkingPackageWellDefined;
            PackageIdentifier id = IsPackageDirectory(State.WorkingDirectory, out isWorkingPackageWellDefined);
            if (null == id)
            {
                throw new Exception("No valid package found in the working directory");
            }

            if (!isWorkingPackageWellDefined)
            {
                throw new Exception("Working directory package is not well defined");
            }

            string definitionPathName = PackageDefinitionPathName(id);
            PackageDefinitionFile definitionFile = new PackageDefinitionFile(definitionPathName, !State.ForceDefinitionFileUpdate);
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

            PackageInformation info = new PackageInformation(id);
            State.PackageInfo.Add(info);
        }

        public static void IdentifyMainAndDependentPackages(bool resolveToSinglePackageVersion, bool allowUndefinedPackages)
        {
            PackageBuildList buildList = new PackageBuildList();

            // find the working directory package
            {
                bool isWorkingPackageWellDefined;
                PackageIdentifier id = IsPackageDirectory(State.WorkingDirectory, out isWorkingPackageWellDefined);
                if (null == id)
                {
                    throw new Exception("No valid package found in the working directory");
                }

                if (!isWorkingPackageWellDefined)
                {
                    throw new Exception("Working directory package is not well define");
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
                PackageIdentifier id = State.DependentPackageList[i++];
                string definitionPathName = PackageDefinitionPathName(id);
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

                PackageDefinitionFile definitionFile = new PackageDefinitionFile(definitionPathName, !State.ForceDefinitionFileUpdate);
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

                foreach (PackageIdentifier id2 in definitionFile.PackageIdentifiers)
                {
                    if (!OSUtilities.IsCurrentPlatformSupported(id2.PlatformFilter))
                    {
                        Log.DebugMessage("Package '{0}-{1}' is not supported on the current platform", id2.Name, id2.Version);
                        continue;
                    }

                    PackageBuild build = buildList.GetPackage(id2.Name);
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
                foreach (PackageBuild build in buildList)
                {
                    if (build.Versions.Count > 1)
                    {
                        string defaultVersion;
                        if (!State.Has("PackageDefaultVersions", build.Name.ToLower()))
                        {
                            StringArray defaultVersions = new StringArray();
                            foreach (PackageIdentifier id in build.Versions)
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
                        foreach (PackageIdentifier version in build.Versions)
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

                    PackageInformation info = new PackageInformation(build.SelectedVersion);
                    State.PackageInfo.Add(info);
                }
            }
            else
            {
                foreach (PackageBuild build in buildList)
                {
                    foreach (PackageIdentifier id in build.Versions)
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

        private static string GetPackageHash(StringArray sourceCode, StringArray definitions)
        {
            int hashCode = 0;
            foreach (string source in sourceCode)
            {
                hashCode ^= source.GetHashCode();
            }
            foreach (string define in definitions)
            {
                hashCode ^= define.GetHashCode();
            }
            string hash = hashCode.ToString();
            return hash;
        }

        public static bool CompilePackageIntoAssembly()
        {
            // validate build root
            if (null == State.BuildRoot)
            {
                throw new Exception("Build root has not been specified");
            }

            TimeProfile gatherSourceProfile = new TimeProfile(ETimingProfiles.GatherSource);
            gatherSourceProfile.StartProfile();

            IdentifyMainAndDependentPackages(true, false);

            PackageInformation mainPackage = State.PackageInfo.MainPackage;

            Log.DebugMessage("Package is '{0}' in '{1}", mainPackage.Identifier.ToString("-"), mainPackage.Identifier.Root);

            BuilderUtilities.SetBuilderPackage();

            // Create resource file containing package information
            string resourceFilePathName = PackageListResourceFile.WriteResourceFile();

            StringArray definitions = new StringArray();

            // gather source files
            StringArray sourceCode = new StringArray();

            int packageIndex = 0;
            foreach (PackageInformation package in State.PackageInfo)
            {
                PackageIdentifier id = package.Identifier;
                Log.DebugMessage("{0}: '{1}' @ '{2}'", packageIndex, id.ToString("-"), id.Root);

                using (System.IO.TextReader reader = new System.IO.StreamReader(id.ScriptPathName))
                {
                    sourceCode.Add(reader.ReadToEnd());
                }
                Log.DebugMessage("\t'{0}'", id.ScriptPathName);
                if (null != package.Scripts)
                {
                    foreach (string scriptFile in package.Scripts)
                    {
                        using (System.IO.TextReader reader = new System.IO.StreamReader(scriptFile))
                        {
                            sourceCode.Add(reader.ReadToEnd());
                        }
                        Log.DebugMessage("\t'{0}'", scriptFile);
                    }
                }
                if (null != package.BuilderScripts)
                {
                    foreach (string builderScriptFile in package.BuilderScripts)
                    {
                        using (System.IO.TextReader reader = new System.IO.StreamReader(builderScriptFile))
                        {
                            sourceCode.Add(reader.ReadToEnd());
                        }
                        Log.DebugMessage("\t'{0}'", builderScriptFile);
                    }
                }

                foreach (string define in package.Identifier.Definition.Definitions)
                {
                    if (!definitions.Contains(define))
                    {
                        definitions.Add(define);
                    }
                }

                ++packageIndex;
            }

            //sourceCode.Sort();

            // add/remove other definitions
            definitions.Add(OpusVersionDefineForCompiler);
            definitions.Add(OpusHostPlatformForCompiler);
            // command line definitions
            definitions.AddRange(State.PackageCompilationDefines);
            definitions.RemoveAll(State.PackageCompilationUndefines);
            definitions.Sort();

            gatherSourceProfile.StopProfile();

            TimeProfile assemblyCompileProfile = new TimeProfile(ETimingProfiles.AssemblyCompilation);
            assemblyCompileProfile.StartProfile();

            // assembly is written to the build root
            string assemblyPathname = System.IO.Path.Combine(State.BuildRoot, "OpusPackageAssembly");
            assemblyPathname = System.IO.Path.Combine(assemblyPathname, mainPackage.Name) + ".dll";

            // can an existing assembly be reused?
            string hashPathName = System.IO.Path.ChangeExtension(assemblyPathname, "hash");
            string thisHashCode = GetPackageHash(sourceCode, definitions);
            if (State.CacheAssembly)
            {
                if (System.IO.File.Exists(hashPathName))
                {
                    using (System.IO.TextReader reader = new System.IO.StreamReader(hashPathName))
                    {
                        string diskHashCode = reader.ReadLine();
                        if (diskHashCode.Equals(thisHashCode))
                        {
                            Log.DebugMessage("Cached assembly used '{0}', with hash {1}", assemblyPathname, diskHashCode);
                            Log.Detail("Re-using existing package assembly");
                            State.ScriptAssemblyPathname = assemblyPathname;

                            assemblyCompileProfile.StopProfile();

                            return true;
                        }
                        else
                        {
                            Log.DebugMessage("Hashes differ: '{0}' (disk) '{1}' now", diskHashCode, thisHashCode);
                        }
                    }
                }
            }

            Log.Detail("Compiling package assembly");

            System.Collections.Generic.Dictionary<string, string> providerOptions = new System.Collections.Generic.Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v3.5");

            if (State.RunningMono)
            {
                Log.DebugMessage("Compiling assembly for Mono");
            }

            using (Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider(providerOptions))
            {
                System.CodeDom.Compiler.CompilerParameters compilerParameters = new System.CodeDom.Compiler.CompilerParameters();
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

                compilerParameters.OutputAssembly = assemblyPathname;

                string compilerOptions = "/checked+ /unsafe-";
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
                    // Opus assembly
                    foreach (string opusAssembly in mainPackage.Identifier.Definition.OpusAssemblies)
                    {
                        string assemblyFileName = System.String.Format("{0}.dll", opusAssembly);
                        string assemblyPathName = System.IO.Path.Combine(State.OpusDirectory, assemblyFileName);
                        compilerParameters.ReferencedAssemblies.Add(assemblyPathName);
                    }

                    // DotNet assembly
                    foreach (DotNetAssemblyDescription desc in mainPackage.Identifier.Definition.DotNetAssemblies)
                    {
                        string assemblyFileName = System.String.Format("{0}.dll", desc.Name);
                        compilerParameters.ReferencedAssemblies.Add(assemblyFileName);
                    }
                }
                else
                {
                    throw new Exception("C# compiler does not support Resources");
                }

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(compilerParameters.OutputAssembly));

                System.CodeDom.Compiler.CompilerResults results = provider.CompileAssemblyFromSource(compilerParameters, sourceCode.ToArray());

                if (results.Errors.HasErrors || results.Errors.HasWarnings)
                {
                    Log.ErrorMessage("Failed to compile package '{0}'. There are {1} errors.", mainPackage.FullName, results.Errors.Count);
                    foreach (System.CodeDom.Compiler.CompilerError error in results.Errors)
                    {
                        Log.ErrorMessage("\t{0}({1}): {2} {3}", error.FileName, error.Line, error.ErrorNumber, error.ErrorText);
                    }
                    return false;
                }

                if (State.CacheAssembly)
                {
                    using (System.IO.TextWriter writer = new System.IO.StreamWriter(hashPathName))
                    {
                        writer.WriteLine(thisHashCode);
                    }
                }
                else
                {
                    // will not throw if the file doesn't exist
                    System.IO.File.Delete(hashPathName);
                }

                Log.DebugMessage("Written assembly to '{0}'", compilerParameters.OutputAssembly);
                State.ScriptAssemblyPathname = compilerParameters.OutputAssembly;
            }

            assemblyCompileProfile.StopProfile();

            return true;
        }

        // special function for debugging, because you MUST compile from the source files, not source in memory
        public static bool CompileDebuggablePackageIntoAssembly()
        {
            TimeProfile gatherSourceProfile = new TimeProfile(ETimingProfiles.GatherSource);
            gatherSourceProfile.StartProfile();

            IdentifyMainAndDependentPackages(true, false);

            PackageInformation mainPackage = State.PackageInfo.MainPackage;

            Log.DebugMessage("Package is '{0}' in '{1}", mainPackage.Identifier.ToString("-"), mainPackage.Identifier.Root);

            BuilderUtilities.SetBuilderPackage();

            // Create resource file containing package information
            string resourceFilePathName = PackageListResourceFile.WriteResourceFile();

            StringArray definitions = new StringArray();

            // gather source files
            System.Collections.ArrayList sourceFileList = new System.Collections.ArrayList();

            int packageIndex = 0;
            foreach (PackageInformation package in State.PackageInfo)
            {
                PackageIdentifier id = package.Identifier;
                Log.DebugMessage("{0}: '{1}' @ '{2}'", packageIndex, id.ToString("-"), id.Root);

                sourceFileList.Add(id.ScriptPathName);
                Log.DebugMessage("\t'{0}'", id.ScriptPathName);
                if (null != package.Scripts)
                {
                    foreach (string scriptFile in package.Scripts)
                    {
                        sourceFileList.Add(scriptFile);
                        Log.DebugMessage("\t'{0}'", scriptFile);
                    }
                }
                if (null != package.BuilderScripts)
                {
                    foreach (string builderScriptFile in package.BuilderScripts)
                    {
                        sourceFileList.Add(builderScriptFile);
                        Log.DebugMessage("\t'{0}'", builderScriptFile);
                    }
                }

                foreach (string define in package.Identifier.Definition.Definitions)
                {
                    if (!definitions.Contains(define))
                    {
                        definitions.Add(define);
                    }
                }

                ++packageIndex;
            }

            // add/remove other definitions
            definitions.Add(OpusVersionDefineForCompiler);
            definitions.Add(OpusHostPlatformForCompiler);
            // command line definitions
            definitions.AddRange(State.PackageCompilationDefines);
            definitions.RemoveAll(State.PackageCompilationUndefines);
            definitions.Sort();

            gatherSourceProfile.StopProfile();

            Log.Detail("Compiling package assembly");

            TimeProfile assemblyCompileProfile = new TimeProfile(ETimingProfiles.AssemblyCompilation);
            assemblyCompileProfile.StartProfile();

            System.Collections.Generic.Dictionary<string, string> providerOptions = new System.Collections.Generic.Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v3.5");

            if (State.RunningMono)
            {
                Log.DebugMessage("Compiling assembly for Mono");
            }

            using (Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider(providerOptions))
            {
                string[] sourceFiles = sourceFileList.ToArray(typeof(string)) as string[];

                System.CodeDom.Compiler.CompilerParameters compilerParameters = new System.CodeDom.Compiler.CompilerParameters();
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
                compilerParameters.OutputAssembly = System.IO.Path.Combine(System.IO.Path.GetTempPath(), mainPackage.Name) + ".dll";
                string compilerOptions = "/checked+ /unsafe-";
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
                    // Opus assembly
                    foreach (string opusAssembly in mainPackage.Identifier.Definition.OpusAssemblies)
                    {
                        string assemblyFileName = System.String.Format("{0}.dll", opusAssembly);
                        string assemblyPathName = System.IO.Path.Combine(State.OpusDirectory, assemblyFileName);
                        compilerParameters.ReferencedAssemblies.Add(assemblyPathName);
                    }

                    // DotNet assembly
                    foreach (DotNetAssemblyDescription desc in mainPackage.Identifier.Definition.DotNetAssemblies)
                    {
                        string assemblyFileName = System.String.Format("{0}.dll", desc.Name);
                        compilerParameters.ReferencedAssemblies.Add(assemblyFileName);
                    }
                }
                else
                {
                    throw new Exception("C# compiler does not support Resources");
                }

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(compilerParameters.OutputAssembly));
                System.CodeDom.Compiler.CompilerResults results = provider.CompileAssemblyFromFile(compilerParameters, sourceFiles);

                if (results.Errors.HasErrors || results.Errors.HasWarnings)
                {
                    Log.ErrorMessage("Failed to compile package '{0}'. There are {1} errors.", mainPackage.FullName, results.Errors.Count);
                    foreach (System.CodeDom.Compiler.CompilerError error in results.Errors)
                    {
                        Log.ErrorMessage("\t{0}({1}): {2} {3}", error.FileName, error.Line, error.ErrorNumber, error.ErrorText);
                    }
                    return false;
                }

                Log.DebugMessage("Written assembly to '{0}'", compilerParameters.OutputAssembly);
                State.ScriptAssemblyPathname = compilerParameters.OutputAssembly;
            }

            assemblyCompileProfile.StopProfile();

            return true;
        }

        public static bool ExecutePackageAssembly()
        {
            // let's think about a new domain
            //            System.AppDomain domain = System.AppDomain.CreateDomain("tempDomain");

            //            System.Reflection.AssemblyName assemblyName = System.Reflection.AssemblyName.GetAssemblyName(State.DebugAssembly);
            //            System.Reflection.Assembly assembly = domain.Load(assemblyName);
            //            System.AppDomain.Unload(domain);

            TypeArray topLevelTypes = GetTopLevelModuleTypes();

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

            DependencyGraph dependencyGraph = new DependencyGraph();
            State.Set("System", "Graph", dependencyGraph);

            // add modules in for each target configured
            foreach (EPlatform platform in State.BuildPlatforms)
            {
                foreach (EConfiguration configuration in State.BuildConfigurations)
                {
                    BaseTarget baseTarget = BaseTarget.GetInstance(platform, configuration);
                    Log.DebugMessage("Added base target '{0}'", baseTarget);
                    foreach (System.Type topLevelType in topLevelTypes)
                    {
                        dependencyGraph.AddTopLevelModule(topLevelType, baseTarget);
                    }
                }
            }

            Log.DebugMessage("\nAfter adding top level modules...");
            dependencyGraph.Dump();
            dependencyGraph.PopulateGraph();
            Log.DebugMessage("\nAfter adding dependencies...");
            dependencyGraph.Dump();

            TimeProfile dependencyGraphExecutionProfile = new TimeProfile(ETimingProfiles.GraphExecution);
            dependencyGraphExecutionProfile.StartProfile();

            BuildManager buildManager = new BuildManager(dependencyGraph);
            State.BuildManager = buildManager;
            State.ReadOnly = true;
            bool success = buildManager.Execute();

            dependencyGraphExecutionProfile.StopProfile();

            return success;
        }

        public static PackageInformation GetOwningPackage(object obj)
        {
            System.Type objType = obj.GetType();
            TypeUtilities.CheckTypeImplementsInterface(objType, typeof(IModule));

            string packageName = objType.Namespace;
            PackageInformation package = State.PackageInfo[packageName];
            return package;
        }

        public static void LoadPackageAssembly()
        {
            TimeProfile assemblyLoadProfile = new TimeProfile(ETimingProfiles.LoadAssembly);
            assemblyLoadProfile.StartProfile();

            System.Reflection.Assembly scriptAssembly = null;

            try
            {
                scriptAssembly = System.Reflection.Assembly.LoadFile(State.ScriptAssemblyPathname);
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

        public static void ProcessLazyArguments(bool fatal)
        {
            if (State.LazyArguments.Count > 0)
            {
                var actions = ActionManager.ScriptActions;
                if ((null == actions) || (0 == actions.Count))
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder();
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

                StringArray lazyCommandsProcessed = new StringArray();
                foreach (string command in State.LazyArguments.Keys)
                {
                    foreach (var action in actions)
                    {
                        Core.IAction iaction = action.Action;
                        bool isThisCommand = false;
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
                foreach (string command in lazyCommandsProcessed)
                {
                    State.LazyArguments.Remove(command);
                }

                lazyCommandsProcessed = null;
            }
        }

        public static void HandleUnprocessedArguments(bool fatal)
        {
            if (State.LazyArguments.Count > 0)
            {
                string message = "Unrecognized command line arguments:\n";
                foreach (string command in State.LazyArguments.Keys)
                {
                    message += "\t'" + command + "'\n";
                }
                if (fatal)
                {
                    throw new Exception(message);
                }
                else
                {
                    Log.Info(message);
                }
            }
        }

        private static TypeArray GetTopLevelModuleTypes()
        {
            TimeProfile findBuildableModulesProfile = new TimeProfile(ETimingProfiles.IdentifyBuildableModules);
            findBuildableModulesProfile.StartProfile();

            // TODO: not sure I like this; find the top level namespace another way
            // TODO: maybe add a Resource into the assembly to indicate the top level types?
            string topLevelNamespace = System.IO.Path.GetFileNameWithoutExtension(State.ScriptAssemblyPathname);
            TypeArray topLevelTypes = new TypeArray();
            StringArray topLevelTypeNames = new StringArray();
            Log.DebugMessage("Searching for top level modules...");
            try
            {
                System.Type[] assemblyTypes = State.ScriptAssembly.GetTypes();
                foreach (System.Type assemblyType in assemblyTypes)
                {
                    if ((0 == System.String.Compare(assemblyType.Namespace, topLevelNamespace, false)) &&
                        !assemblyType.IsNested)
                    {
                        if (!typeof(IModule).IsAssignableFrom(assemblyType))
                        {
                            //Log.DebugMessage("\tIgnoring type '{0}' as it does not implement Opus.IModule", assemblyType.ToString());
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
                    TypeArray tltToRemove = new TypeArray();
                    foreach (System.Type topLevelType in topLevelTypes)
                    {
                        System.Reflection.FieldInfo[] fields = topLevelType.GetFields(System.Reflection.BindingFlags.Instance |
                                                                                      System.Reflection.BindingFlags.Public |
                                                                                      System.Reflection.BindingFlags.NonPublic);
                        foreach (System.Reflection.FieldInfo field in fields)
                        {
                            System.Type t = field.FieldType;
                            if (topLevelTypes.Contains(t))
                            {
                                tltToRemove.Add(t);
                            }
                        }
                    }

                    foreach (System.Type t in tltToRemove)
                    {
                        Log.DebugMessage("\tRemoving '{0}' as it is used as a field in another top level type", t.FullName);
                        topLevelTypeNames.Remove(t.FullName);
                        topLevelTypes.Remove(t);
                    }
                }
            }
            catch (System.Reflection.ReflectionTypeLoadException exception)
            {
                System.Exception[] loaderExceptions = exception.LoaderExceptions;
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
            StringArray buildModules = State.BuildModules;
            if (null != buildModules)
            {
                TypeArray filteredTopLevelTypes = new TypeArray();
                foreach (string buildModule in buildModules)
                {
                    bool found = false;
                    foreach (System.Type topLevelType in topLevelTypes)
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

            findBuildableModulesProfile.StopProfile();

            return topLevelTypes;
        }
    }
}
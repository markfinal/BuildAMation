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

        public static bool CompilePackageIntoAssembly()
        {
            if (0 == State.PackageInfo.Count)
            {
                throw new Exception("Package has not been specified. Run Opus from the package directory.", false);
            }

            PackageInformation mainPackage = State.PackageInfo[0];

            Log.DebugMessage("Package is '{0}-{1}' in '{2}", mainPackage.Name, mainPackage.Version, mainPackage.Root);

            // recursively discover dependent packages
            XmlPackageDependencyDiscovery.Execute(mainPackage);

            BuilderUtilities.EnsureBuilderPackageExists();

            // Create resource file containing package information
            string resourceFilePathName = PackageListResourceFile.WriteResourceFile();

            // gather source files
            System.Collections.ArrayList sourceFileList = new System.Collections.ArrayList();

            int packageIndex = 0;
            foreach (PackageInformation package in State.PackageInfo)
            {
                Log.DebugMessage("{0}: '{1}-{2}' @ '{3}'", packageIndex, package.Name, package.Version, package.Root);

                sourceFileList.Add(package.ScriptFile);
                Log.DebugMessage("\t'{0}'", package.ScriptFile);
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

                ++packageIndex;
            }

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
                    // mono complaints a lot more fussy about warnings
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

                // version number
                compilerOptions += " /define:" + OpusVersionDefineForCompiler;

                compilerParameters.CompilerOptions = compilerOptions;
                compilerParameters.EmbeddedResources.Add(resourceFilePathName);

                if (provider.Supports(System.CodeDom.Compiler.GeneratorSupport.Resources))
                {
                    System.Reflection.AssemblyName[] referencedAssemblies = System.Reflection.Assembly.GetCallingAssembly().GetReferencedAssemblies();
                    foreach (System.Reflection.AssemblyName refAssembly in referencedAssemblies)
                    {
                        if (("Opus.Core" == refAssembly.Name) ||
                            ("System" == refAssembly.Name) ||
                            ("System.Xml" == refAssembly.Name))
                        {
                            System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(refAssembly);
                            compilerParameters.ReferencedAssemblies.Add(assembly.Location);
                        }
                    }
                }

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(compilerParameters.OutputAssembly));
                System.CodeDom.Compiler.CompilerResults results = provider.CompileAssemblyFromFile(compilerParameters, sourceFiles);

                if (results.Errors.HasErrors || results.Errors.HasWarnings)
                {
                    Log.ErrorMessage("Failed to compile package '{0}'. There are {1} errors.", mainPackage.FullName, results.Errors.Count);
                    foreach (System.CodeDom.Compiler.CompilerError error in results.Errors)
                    {
                        Log.MessageAll("\t{0}({1}): {2} {3}", error.FileName, error.Line, error.ErrorNumber, error.ErrorText);
                    }
                    return false;
                }

                Log.DebugMessage("Written assembly to '{0}'", compilerParameters.OutputAssembly);
                State.ScriptAssemblyPathname = compilerParameters.OutputAssembly;
            }

            return true;
        }

        public static bool ExecutePackageAssembly()
        {
            // let's think about a new domain
            //            System.AppDomain domain = System.AppDomain.CreateDomain("tempDomain");

            //            System.Reflection.AssemblyName assemblyName = System.Reflection.AssemblyName.GetAssemblyName(State.DebugAssembly);
            //            System.Reflection.Assembly assembly = domain.Load(assemblyName);
            //            System.AppDomain.Unload(domain);

            LocateRequiredPackages();

            TypeArray topLevelTypes = GetTopLevelModuleTypes();

            BuilderUtilities.CreateBuilderInstance();

            // validate build root
            if (null == State.BuildRoot)
            {
                throw new Exception("Build root has not been specified", false);
            }

            RegisterTargetToolChainAttribute[] targetToolChains = RegisterTargetToolChainAttribute.TargetToolChains;
            if (null == targetToolChains)
            {
                throw new Exception("No target toolchains were registered", false);
            }

            if (null == State.BuildPlatforms)
            {
                throw new Exception("No build platforms were specified", false);
            }
            if (null == State.BuildConfigurations)
            {
                throw new Exception("No build configurations were specified", false);
            }

            // create targets
            TargetCollection targetCollection = new TargetCollection();
            foreach (EPlatform platform in State.BuildPlatforms)
            {
                foreach (EConfiguration configuration in State.BuildConfigurations)
                {
                    Target target = new Target(platform, configuration);
                    Log.DebugMessage("Added target '{0}'", target);
                    targetCollection.Add(target);
                }
            }

            DependencyGraph dependencyGraph = new DependencyGraph();
            State.Set("System", "Graph", dependencyGraph);
            foreach (Target target in targetCollection)
            {
                foreach (System.Type topLevelType in topLevelTypes)
                {
                    dependencyGraph.AddTopLevelModule(topLevelType, target);
                }
            }
            Log.DebugMessage("\nAfter adding top level modules...");
            dependencyGraph.Dump();
            dependencyGraph.PopulateGraph();
            Log.DebugMessage("\nAfter adding dependencies...");
            dependencyGraph.Dump();

            BuildManager buildManager = new BuildManager(dependencyGraph);
            if (false == buildManager.Execute())
            {
                return false;
            }

            return true;
        }

        public static PackageInformation GetOwningPackage(object obj)
        {
            System.Type objType = obj.GetType();
            if (!typeof(IModule).IsAssignableFrom(objType))
            {
                throw new Exception(System.String.Format("Object '{0}' does not implement the Opus.Core.IModule interface", obj.ToString()));
            }

            string packageName = objType.Namespace;
            PackageInformation package = State.PackageInfo[packageName];
            return package;
        }

        public static void LoadPackageAssembly()
        {
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
        }

        public static void ProcessLazyArguments()
        {
            if (State.LazyArguments.Count > 0)
            {
                string[] copyOfLazyArguments = State.LazyArguments.ToArray();
                ArgumentProcessorAttribute[] argumentProcessors = State.ScriptAssembly.GetCustomAttributes(typeof(ArgumentProcessorAttribute), false) as ArgumentProcessorAttribute[];
                foreach (ArgumentProcessorAttribute argumentProcessor in argumentProcessors)
                {
                    foreach (string command in copyOfLazyArguments)
                    {
                        bool processed = argumentProcessor.Process(command);
                        if (processed)
                        {
                            State.LazyArguments.Remove(command);
                        }
                    }
                }

                copyOfLazyArguments = null;

                HandleUnprocessedArguments();
            }
        }

        public static void HandleUnprocessedArguments()
        {
            if (State.LazyArguments.Count > 0)
            {
                string message = "Unrecognized command line arguments:\n";
                foreach (string command in State.LazyArguments)
                {
                    message += "\t'" + command + "'\n";
                }
                throw new Exception(message, false);
            }
        }

        private static void LocateRequiredPackages()
        {
            // get the resource
            string resourceName = System.String.Format("{0}.PackageInfoResources", State.PackageInfo[0].Name);
            System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(resourceName, State.ScriptAssembly);
            System.Resources.ResourceSet resourceSet = resourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
            foreach (System.Collections.DictionaryEntry resourceDictionaryEntry in resourceSet)
            {
                string[] packageInfo = resourceDictionaryEntry.Key.ToString().Split(new char[] { '_' });
                Log.DebugMessage("Found package {0}-{1} @ {2}", packageInfo[0], packageInfo[1], resourceDictionaryEntry.Value);
                PackageInformation package = new PackageInformation(packageInfo[0], packageInfo[1], resourceDictionaryEntry.Value.ToString());

                if (!State.PackageInfo.Contains(package))
                {
                    State.PackageInfo.Add(package);
                }
            }
        }

        private static TypeArray GetTopLevelModuleTypes()
        {
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
                throw new Exception(System.String.Format("Unable to locate any objects in namespace '{0}'", topLevelNamespace), false);
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
                        throw new Exception(System.String.Format("Unable to locate specified module '{0}' in the list of module types for this package:\n{1}", buildModule, topLevelTypeNames.ToString('\n')), false);
                    }
                }

                topLevelTypes = filteredTopLevelTypes;
            }

            return topLevelTypes;
        }
    }
}
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
    /// Utility class defining the entry point for Bam execution.
    /// </summary>
    public static class EntryPoint
    {
        /// <summary>
        /// Log the Bam version number, at the specified log level.
        /// </summary>
        /// <param name="level">Log level to use.</param>
        public static void
        PrintVersion(
            EVerboseLevel level)
        {
            var message = new System.Text.StringBuilder();
            message.AppendLine($"BuildAMation (Bam) v{Core.Graph.Instance.ProcessState.VersionString} (c) Mark Final, 2010-2019. Licensed under BSD 3-clause. See License.md.");
            message.AppendLine("Parts of this software are licensed under the Microsoft Limited Public License (MS-PL). See MS-PL.md.");
            message.Append("Parts of this software use thirdparty open source software. See 3rdPartyLicenses.md.");
            Core.Log.Message(level, message.ToString());
        }

        /// <summary>
        /// Log the Bam version number, at the current log level.
        /// </summary>
        public static void
        PrintVersion() => PrintVersion(Graph.Instance.VerbosityLevel);

        /// <summary>
        /// Bam execution
        /// - find all packages required
        /// - compile all package scripts into an assembly (unless in a debug project)
        /// - generate metadata for each package
        /// - create top-level modules, i.e. modules in the package in which Bam was invoked (performed by namespace lookup
        /// and all sealed module classes), execute their Init functions, which will recursively create all Modules required
        /// - assign default settings for Modules using a Tool, and execute the settings patches where they exist, so that
        /// module settings are now configured as the user specified them
        /// - begin sorting modules into rank collections
        /// - validate the dependencies created, so that there are no cyclic dependencies, or modules in the wrong rank to
        /// satisfy their dependencies
        /// - expand all TokenizedStrings that have been created
        /// - (display the dependency graph if the user specified the command line option)
        /// - execute the dependency graph, from highest rank to lowest rank, using the build mode specified by the user.
        /// </summary>
        /// <param name="environments">An array of Environments in which to generate modules.</param>
        /// <param name="packageAssembly">Optional Assembly containing the package code. Defaults to null. Only required when building as part of a debug project.</param>
        public static void
        Execute(
            Array<Environment> environments,
            System.Reflection.Assembly packageAssembly = null)
        {
            PrintVersion();

            if (!environments.Any())
            {
                throw new Exception("No build configurations were specified");
            }

            var graph = Graph.Instance;
            if (null != packageAssembly)
            {
                PackageUtilities.IdentifyAllPackages(allowDuplicates: false);
                graph.ScriptAssembly = packageAssembly;
                graph.ScriptAssemblyPathname = packageAssembly.Location;
            }
            else
            {
                PackageUtilities.CompilePackageAssembly(true);
                PackageUtilities.LoadPackageAssembly();
            }

            var packageMetaDataProfile = new TimeProfile(ETimingProfiles.PackageMetaData);
            packageMetaDataProfile.StartProfile();

            // validate that there is at most one local policy
            // if test mode is enabled, then the '.tests' sub-namespaces are also checked
            {
                var localPolicyTypes = graph.ScriptAssembly.GetTypes().Where(t => typeof(ISitePolicy).IsAssignableFrom(t));
                if (!graph.UseTestsNamespace)
                {
                    localPolicyTypes = localPolicyTypes.Where(item => !item.Namespace.EndsWith(".tests", System.StringComparison.Ordinal));
                }
                var numLocalPolicies = localPolicyTypes.Count();
                if (numLocalPolicies > 0)
                {
                    if (numLocalPolicies > 1)
                    {
                        Log.MessageAll(graph.MasterPackage.Name);
                        var masterPolicyType = localPolicyTypes.FirstOrDefault(item => item.Namespace.StartsWith(graph.MasterPackage.Name, System.StringComparison.Ordinal));
                        if (null != masterPolicyType)
                        {
                            var message = new System.Text.StringBuilder();
                            message.AppendLine("More than one site policies exist in the package assembly:");
                            foreach (var policy in localPolicyTypes)
                            {
                                message.AppendLine($"\t{policy.ToString()}");
                            }
                            message.AppendLine($"Choosing master policy type: {masterPolicyType.ToString()}");
                            Log.DebugMessage(message.ToString());
                            Settings.LocalPolicy = System.Activator.CreateInstance(masterPolicyType) as ISitePolicy;
                        }
                        else
                        {
                            var message = new System.Text.StringBuilder();
                            message.AppendLine("Too many site policies exist in the package assembly:");
                            foreach (var policy in localPolicyTypes)
                            {
                                message.AppendLine($"\t{policy.ToString()}");
                            }
                            throw new Exception(message.ToString());
                        }
                    }
                    else
                    {
                        Settings.LocalPolicy = System.Activator.CreateInstance(localPolicyTypes.First()) as ISitePolicy;
                    }
                }
            }

            // find a product definition
            {
                var productDefinitions = graph.ScriptAssembly.GetTypes().Where(t => typeof(IProductDefinition).IsAssignableFrom(t));
                var numProductDefinitions = productDefinitions.Count();
                if (numProductDefinitions > 0)
                {
                    if (numProductDefinitions > 1)
                    {
                        var message = new System.Text.StringBuilder();
                        message.AppendLine("Too many product definitions exist in the package assembly:");
                        foreach (var def in productDefinitions)
                        {
                            message.AppendLine($"\t{def.ToString()}");
                        }
                        throw new Exception(message.ToString());
                    }

                    graph.ProductDefinition = System.Activator.CreateInstance(productDefinitions.First()) as IProductDefinition;
                }
            }

            // get the metadata from the build mode package
            var metaName = $"{graph.Mode}Builder.{graph.Mode}Meta";
            var metaDataType = graph.ScriptAssembly.GetType(metaName);
            if (null == metaDataType)
            {
                throw new Exception(
                    $"No build mode {graph.Mode} meta data type {metaName}"
                );
            }

            if (!typeof(IBuildModeMetaData).IsAssignableFrom(metaDataType))
            {
                throw new Exception(
                    $"Build mode package meta data type {metaDataType.ToString()} does not implement the interface {typeof(IBuildModeMetaData).ToString()}"
                );
            }
            graph.BuildModeMetaData = System.Activator.CreateInstance(metaDataType) as IBuildModeMetaData;

            // packages can have meta data - instantiate where they exist
            foreach (var package in graph.Packages)
            {
                var ns = package.Name;
                var metaType = graph.ScriptAssembly.GetTypes().FirstOrDefault(item =>
                    System.String.Equals(item.Namespace, ns, System.StringComparison.Ordinal) &&
                    typeof(PackageMetaData).IsAssignableFrom(item)
                );
                if (null == metaType)
                {
                    continue;
                }
                if (metaType.IsAbstract)
                {
                    // used for base classes
                    continue;
                }

                if (null != package.MetaData)
                {
                    // already been instantiated
                    continue;
                }
                package.MetaData = Graph.InstantiatePackageMetaData(metaType);
            }

            // look for module configuration override
            {
                var overrides = graph.ScriptAssembly.GetTypes().Where(t => typeof(IOverrideModuleConfiguration).IsAssignableFrom(t));
                var numOverrides = overrides.Count();
                if (numOverrides > 0)
                {
                    if (numOverrides > 1)
                    {
                        var nonTestNamespaces = overrides.Where(item => !item.Namespace.Contains(".tests"));
                        var testNamespaces = overrides.Where(item => item.Namespace.Contains(".tests"));

                        if (nonTestNamespaces.Count() > 1 || testNamespaces.Count() > 1)
                        {
                            var message = new System.Text.StringBuilder();
                            message.AppendLine($"Too many implementations of {typeof(IOverrideModuleConfiguration).ToString()}");
                            foreach (var oride in overrides)
                            {
                                message.AppendLine($"\t{oride.ToString()}");
                            }
                            throw new Exception(message.ToString());
                        }
                        else
                        {
                            if (graph.UseTestsNamespace)
                            {
                                // prefer test namespace overrides
                                if (1 == testNamespaces.Count())
                                {
                                    graph.OverrideModuleConfiguration = System.Activator.CreateInstance(testNamespaces.First()) as IOverrideModuleConfiguration;
                                }
                                else if (1 == nonTestNamespaces.Count())
                                {
                                    graph.OverrideModuleConfiguration = System.Activator.CreateInstance(nonTestNamespaces.First()) as IOverrideModuleConfiguration;
                                }
                            }
                            else
                            {
                                // prefer non-test namespace overrides
                                if (1 == nonTestNamespaces.Count())
                                {
                                    graph.OverrideModuleConfiguration = System.Activator.CreateInstance(nonTestNamespaces.First()) as IOverrideModuleConfiguration;
                                }
                                else if (1 == testNamespaces.Count())
                                {
                                    graph.OverrideModuleConfiguration = System.Activator.CreateInstance(testNamespaces.First()) as IOverrideModuleConfiguration;
                                }
                            }
                        }
                    }
                    else
                    {
                        graph.OverrideModuleConfiguration = System.Activator.CreateInstance(overrides.First()) as IOverrideModuleConfiguration;
                    }
                }
            }

            packageMetaDataProfile.StopProfile();

            var topLevelNamespace = graph.MasterPackage.Name;

            var findBuildableModulesProfile = new TimeProfile(ETimingProfiles.IdentifyBuildableModules);
            findBuildableModulesProfile.StartProfile();

            // Phase 1: Instantiate all modules in the namespace of the package in which the tool was invoked
            // The side-effect of this is that ALL Modules in the Graph will be created, and their Init functions
            // invoked.
            // The end result is a linear list of Modules per build environment.
            Log.Detail("Creating modules...");
            foreach (var env in environments)
            {
                graph.CreateTopLevelModules(graph.ScriptAssembly, env, topLevelNamespace);
            }

            findBuildableModulesProfile.StopProfile();

            var populateGraphProfile = new TimeProfile(ETimingProfiles.PopulateGraph);
            populateGraphProfile.StartProfile();
            // Phase 2: Graph now has a linear list of modules; create a dependency graph
            // NB: all those modules with 0 dependees are the top-level modules
            // NB: no settings have been assigned to modules, nor have patches been applied to settings
            graph.SortDependencies();
            populateGraphProfile.StopProfile();

            // TODO: make validation optional, if it starts showing on profiles
            var validateGraphProfile = new TimeProfile(ETimingProfiles.ValidateGraph);
            validateGraphProfile.StartProfile();
            graph.Validate();
            validateGraphProfile.StopProfile();

            // Phase 3: Create default settings, and apply patches (build + shared) to each module
            // Also apply post-init functions to Modules now that they're in dependency order.
            // NB: some builders can use the patch directly for child objects, so this may be dependent upon the builder
            // Toolchains for modules need to be set here, as they might append macros into each module in order to evaluate paths
            // TODO: a parallel thread can be spawned here, that can check whether command lines have changed
            // the Settings object can be inspected, and a hash generated. This hash can be written to disk, and compared.
            // If a 'verbose' mode is enabled, then more work can be done to figure out what has changed. This would also require
            // serializing the binary Settings object
            var createPatchesProfile = new TimeProfile(ETimingProfiles.CreatePatches);
            createPatchesProfile.StartProfile();
            graph.ApplySettingsPatches();
            createPatchesProfile.StopProfile();

            // expand paths after patching settings, because some of the patches may contain tokenized strings
            // TODO: a thread can be spawned, to check for whether files were in date or not, which will
            // be ready in time for graph execution
            var parseStringsProfile = new TimeProfile(ETimingProfiles.ParseTokenizedStrings);
            parseStringsProfile.StartProfile();
            TokenizedString.ParseAll();
            parseStringsProfile.StopProfile();

            if (CommandLineProcessor.Evaluate(new Options.ViewDependencyGraph()))
            {
                // must come after all strings are parsed, in order to display useful paths
                graph.Dump();
            }

            // Phase 4: Execute dependency graph
            // N.B. all paths (including those with macros) have been delayed expansion until now
            var graphExecutionProfile = new TimeProfile(ETimingProfiles.GraphExecution);
            graphExecutionProfile.StartProfile();
            var executor = new Executor();
            executor.Run();
            graphExecutionProfile.StopProfile();
        }
    }
}

#region License
// Copyright (c) 2010-2016, Mark Final
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
            message.AppendFormat("BuildAMation (Bam) v{0} (c) Mark Final, 2010-2016. Licensed under BSD 3-clause. See License.md.",
                Core.Graph.Instance.ProcessState.VersionString);
            message.AppendLine();
            message.Append("Parts of this software are licensed under the Microsoft Limited Public License (MS-PL). See MS-PL.md.");
            Core.Log.Message(level, message.ToString());
        }

        /// <summary>
        /// Log the Bam version number, at the current log level.
        /// </summary>
        public static void
        PrintVersion()
        {
            PrintVersion(Graph.Instance.VerbosityLevel);
        }

        /// <summary>
        /// Bam execution
        /// - find all packages required
        /// - compile all package scripts into an assembly (unless in a debug project)
        /// - generate metadata for each package
        /// - create top-level modules, i.e. modules in the package in which Bam was invoked (performed by namespace lookup
        /// and all sealed module classes), without any knowledge of inter-dependencies
        /// - invoke the Init functions on each created module, and begin sorting modules into rank collections, and creating
        /// instances of non-top-level modules that are required. If a module has a tool, the default settings are created for
        /// each module.
        /// - validate the dependencies created, so that there are no cyclic dependencies, or modules in the wrong rank to
        /// satisfy their dependencies
        /// - execute the settings patches where they exist, so that module settings are now configured as the user specified them
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

            if (0 == environments.Count)
            {
                throw new Exception("No build configurations were specified");
            }

            if (null != packageAssembly)
            {
                PackageUtilities.IdentifyAllPackages();
                Graph.Instance.ScriptAssembly = packageAssembly;
                Graph.Instance.ScriptAssemblyPathname = packageAssembly.Location;
            }
            else
            {
                var compiledSuccessfully = PackageUtilities.CompilePackageAssembly();
                if (!compiledSuccessfully)
                {
                    throw new Exception("Package compilation failed");
                }
                PackageUtilities.LoadPackageAssembly();
            }

            var packageMetaDataProfile = new TimeProfile(ETimingProfiles.PackageMetaData);
            packageMetaDataProfile.StartProfile();

            // get the metadata from the build mode package
            var graph = Graph.Instance;
            var metaName = System.String.Format("{0}Builder.{0}Meta", graph.Mode);
            var metaDataType = graph.ScriptAssembly.GetType(metaName);
            if (null == metaDataType)
            {
                throw new Exception("No build mode {0} meta data type {1}", graph.Mode, metaName);
            }

            if (!typeof(IBuildModeMetaData).IsAssignableFrom(metaDataType))
            {
                throw new Exception("Build mode package meta data type {0} does not implement the interface {1}", metaDataType.ToString(), typeof(IBuildModeMetaData).ToString());
            }
            graph.BuildModeMetaData = System.Activator.CreateInstance(metaDataType) as IBuildModeMetaData;

            // packages can have meta data - instantiate where they exist
            foreach (var package in graph.Packages)
            {
                var ns = package.Name;
                var metaType = graph.ScriptAssembly.GetTypes().Where(item => item.Namespace == ns && typeof(PackageMetaData).IsAssignableFrom(item)).FirstOrDefault();
                if (null != metaType)
                {
                    try
                    {
                        package.MetaData = System.Activator.CreateInstance(metaType) as PackageMetaData;
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    catch (System.Reflection.TargetInvocationException exception)
                    {
                        throw new Exception(exception, "Failed to create package metadata");
                    }
                }
            }

            packageMetaDataProfile.StopProfile();

            var topLevelNamespace = graph.MasterPackage.Name;

            var findBuildableModulesProfile = new TimeProfile(ETimingProfiles.IdentifyBuildableModules);
            findBuildableModulesProfile.StartProfile();

            // Phase 1: Instantiate all modules in the namespace of the package in which the tool was invoked
            Log.Detail("Creating modules");
            foreach (var env in environments)
            {
                graph.CreateTopLevelModules(graph.ScriptAssembly, env, topLevelNamespace);
            }

            findBuildableModulesProfile.StopProfile();

            var populateGraphProfile = new TimeProfile(ETimingProfiles.PopulateGraph);
            populateGraphProfile.StartProfile();
            // Phase 2: Graph now has a linear list of modules; create a dependency graph
            // NB: all those modules with 0 dependees are the top-level modules
            // NB: default settings have already been defined here
            // not only does this generate the dependency graph, but also creates the default settings for each module, and completes them
            graph.SortDependencies();
            populateGraphProfile.StopProfile();

            // TODO: make validation optional, if it starts showing on profiles
            var validateGraphProfile = new TimeProfile(ETimingProfiles.ValidateGraph);
            validateGraphProfile.StartProfile();
            graph.Validate();
            validateGraphProfile.StopProfile();

            // Phase 3: (Create default settings, and ) apply patches (build + shared) to each module
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

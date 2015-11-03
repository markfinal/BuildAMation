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
    public static class EntryPoint
    {
        public static void
        Execute(
            Array<Environment> environments,
            System.Reflection.Assembly packageAssembly = null)
        {
            if (0 == environments.Count)
            {
                throw new Exception("No build configurations were specified");
            }

            if (null != packageAssembly)
            {
                PackageUtilities.IdentifyAllPackages();
                State.ScriptAssembly = packageAssembly;
                State.ScriptAssemblyPathname = packageAssembly.Location;
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

            var graph = Graph.Instance;
            graph.Mode = State.BuildMode;
            if (null == graph.Mode)
            {
                throw new Exception("Building mode has not been set");
            }

            var packageMetaDataProfile = new TimeProfile(ETimingProfiles.PackageMetaData);
            packageMetaDataProfile.StartProfile();

            // get the metadata from the build mode package
            var metaName = System.String.Format("{0}Builder.{0}Meta", graph.Mode);
            var metaDataType = State.ScriptAssembly.GetType(metaName);
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
                var metaType = State.ScriptAssembly.GetTypes().Where(item => item.Namespace == ns && typeof(PackageMetaData).IsAssignableFrom(item)).FirstOrDefault();
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

            var topLevelNamespace = System.IO.Path.GetFileNameWithoutExtension(State.ScriptAssemblyPathname);

            var findBuildableModulesProfile = new TimeProfile(ETimingProfiles.IdentifyBuildableModules);
            findBuildableModulesProfile.StartProfile();

            // Phase 1: Instantiate all modules in the namespace of the package in which the tool was invoked
            Log.Detail("Creating modules");
            foreach (var env in environments)
            {
                graph.CreateTopLevelModules(State.ScriptAssembly, env, topLevelNamespace);
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

            graph.Dump();

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

#region License
// Copyright 2010-2015 Mark Final
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
#endregion // License
namespace Bam
{
    /// <summary>
    /// Command line tool main entry point
    /// </summary>
    class Program
    {
        static bool UseV2 = true;

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Argument array.</param>
        static void
        Main(
            string[] args)
        {
            if (UseV2)
            {
                Core.State.BuildRoot = "build";
                Core.State.VerbosityLevel = Core.EVerboseLevel.Full;
                Core.State.CompileWithDebugSymbols = true;
                Core.State.BuilderName = Core.V2.CommandLineProcessor.Evaluate(new Core.V2.BuilderName());
                if (null == Core.State.BuilderName)
                {
                    throw new Core.Exception("No builder specified");
                }
                //Core.State.BuilderName = "Native";
                //Core.State.BuilderName = "MakeFile";
                //Core.State.BuilderName = "VSSolution";
                var compiledSuccessfully = Core.PackageUtilities.CompilePackageAssembly();
                if (!compiledSuccessfully)
                {
                    throw new Core.Exception("Package compilation failed");
                }

                Core.PackageUtilities.LoadPackageAssembly();
                var topLevelNamespace = System.IO.Path.GetFileNameWithoutExtension(Core.State.ScriptAssemblyPathname);

                var graph = Core.V2.Graph.Instance;
                graph.Mode = Core.State.BuilderName;

                var debug = new Core.V2.Environment();
                debug.Configuration = Core.EConfiguration.Debug;

                var optimized = new Core.V2.Environment();
                optimized.Configuration = Core.EConfiguration.Optimized;

                // Phase 1: Instantiate all modules in the namespace of the package in which the tool was invoked
                foreach (var env in new[] { debug/*, optimized*/})
                {
                    graph.CreateTopLevelModules(Core.State.ScriptAssembly, env, topLevelNamespace);
                }

                // Phase 2: Graph now has a linear list of modules; create a dependency graph
                // NB: all those modules with 0 dependees are the top-level modules
                // NB: default settings have already been defined here
                var topLevelModules = graph.TopLevelModules;
                Core.Log.DebugMessage("Start: Top level modules");
                foreach (var m in topLevelModules)
                {
                    Core.Log.DebugMessage(m.ToString());
                }
                Core.Log.DebugMessage("End: Top level modules");
                // not only does this generate the dependency graph, but also creates the default settings for each module
                graph.SortDependencies();
                // TODO: make validation optional, if it starts showing on profiles
                graph.Validate();

                // TODO: is it valid for the path expansion to occur prior to patching the settings?
                // if so, a thread can be spawned, to check for whether files were in date or not, which will
                // be ready in time for graph execution
                graph.ExpandPaths();

                // Phase 3: (Create default settings, and ) apply patches (build + shared) to each module
                // NB: some builders can use the patch directly for child objects, so this may be dependent upon the builder
                // Toolchains for modules need to be set here, as they might append macros into each module in order to evaluate paths
                // TODO: a parallel thread can be spawned here, that can check whether command lines have changed
                // the Settings object can be inspected, and a hash generated. This hash can be written to disk, and compared.
                // If a 'verbose' mode is enabled, then more work can be done to figure out what has changed. This would also require
                // serializing the binary Settings object
                graph.ApplySettingsPatches();

                graph.Dump();

                // Phase 4: Execute dependency graph
                // N.B. all paths (including those with macros) have been delayed expansion until now
                var executor = new Core.V2.Executor();
                executor.Run();
            }
            else
            {
                // take control of Ctrl+C
                System.Console.CancelKeyPress += new System.ConsoleCancelEventHandler(HandleCancellation);

                try
                {
                    var profile = new Core.TimeProfile(Core.ETimingProfiles.TimedTotal);
                    profile.StartProfile();

                    var application = new Application(args);
                    application.Run();

                    profile.StopProfile();

                    if (Core.State.ShowTimingStatistics)
                    {
                        Core.TimingProfileUtilities.DumpProfiles();
                    }
                }
                catch (Core.Exception exception)
                {
                    Core.Exception.DisplayException(exception);
                    System.Environment.ExitCode = -1;
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    Core.Exception.DisplayException(exception);
                    System.Environment.ExitCode = -2;
                }
                catch (System.Exception exception)
                {
                    Core.Exception.DisplayException(exception);
                    System.Environment.ExitCode = -3;
                }
                finally
                {
                    if (0 == System.Environment.ExitCode)
                    {
                        Core.Log.Info("\nSucceeded");
                    }
                    else
                    {
                        Core.Log.Info("\nFailed");
                    }
                    Core.Log.DebugMessage("Exit code is {0}", System.Environment.ExitCode);
                }
            }
        }

        private static void
        HandleCancellation(
            object sender,
            System.ConsoleCancelEventArgs e)
        {
            // allow the build to fail gracefully
            var buildManager = Core.State.BuildManager;
            if (null != buildManager)
            {
                buildManager.Cancel();
                e.Cancel = true;
            }
        }
    }
}
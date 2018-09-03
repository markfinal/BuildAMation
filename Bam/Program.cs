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
namespace Bam
{
    /// <summary>
    /// Command line tool main entry point
    /// </summary>
    class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Argument array.</param>
        static void
        Main(
            string[] args)
        {
#if false
            // take control of Ctrl+C
            System.Console.CancelKeyPress += new System.ConsoleCancelEventHandler(HandleCancellation);
#endif

            try
            {
                var totalTimeProfile = new Core.TimeProfile(Core.ETimingProfiles.TimedTotal);
                var processCommandLineProfile = new Core.TimeProfile(Core.ETimingProfiles.ProcessCommandLine);

                totalTimeProfile.StartProfile();
                processCommandLineProfile.StartProfile();

                var verbosityLevel = (Core.EVerboseLevel)Core.CommandLineProcessor.Evaluate(new Core.Options.VerbosityLevel());
                switch (verbosityLevel)
                {
                    case Core.EVerboseLevel.None:
                    case Core.EVerboseLevel.Info:
                    case Core.EVerboseLevel.Detail:
                    case Core.EVerboseLevel.Full:
                        Core.Graph.Instance.VerbosityLevel = verbosityLevel;
                        break;

                    default:
                        throw new Core.Exception("Unrecognized verbosity level, {0}", verbosityLevel);
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.Options.PrintHelp()))
                {
                    CommandLineArgumentHelper.PrintHelp();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.Options.PrintVersion()))
                {
                    CommandLineArgumentHelper.PrintVersion();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.Options.PrintInstallDirectory()))
                {
                    CommandLineArgumentHelper.PrintInstallationDirectory();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.Options.CreateDebugProject()))
                {
                    DebugProject.Create();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.Options.MakePackage()))
                {
                    Core.PackageUtilities.MakePackage();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.Options.AddDependentPackage()))
                {
                    Core.PackageUtilities.AddDependentPackage();
                    return;
                }

                if (Core.CommandLineProcessor.Evaluate(new Core.Options.ShowDefinitionFile()))
                {
                    Core.PackageUtilities.IdentifyAllPackages(allowDuplicates: true, enforceBamAssemblyVersions: false);
                    Core.Graph.Instance.MasterPackage.Show();
                    return;
                }

                // configure
                Core.Graph.Instance.BuildRoot = Core.CommandLineProcessor.Evaluate(new Core.Options.BuildRoot());
                Core.Graph.Instance.Mode = Core.CommandLineProcessor.Evaluate(new Core.Options.BuildMode());
                if (null == Core.Graph.Instance.Mode)
                {
                    throw new Core.Exception("No build mode specified");
                }

                var configs = new Core.Array<Core.Environment>();
                var requestedConfigs = Core.CommandLineProcessor.Evaluate(new Core.Options.BuildConfigurations());
                if (0 == requestedConfigs.Count)
                {
                    // default
                    requestedConfigs.Add(new Core.StringArray("debug"));
                }
                foreach (var configOption in requestedConfigs)
                {
                    foreach (var config in configOption)
                    {
                        var env = new Core.Environment();
                        env.Configuration = Core.Configuration.FromString(config);
                        configs.Add(env);
                    }
                }

                processCommandLineProfile.StopProfile();

                Core.EntryPoint.Execute(configs);

                totalTimeProfile.StopProfile();
            }
            catch (Core.Exception exception)
            {
                Core.Exception.DisplayException(exception);
                System.Environment.ExitCode = -1;
            }
            catch (System.Exception exception)
            {
                Core.Exception.DisplayException(exception);
                System.Environment.ExitCode = -2;
            }
            finally
            {
                if (Core.Graph.Instance.BuildEnvironments.Count > 0)
                {
                    Core.Log.Info((0 == System.Environment.ExitCode) ? "\nBuild Succeeded" : "\nBuild Failed");

                    if (Core.CommandLineProcessor.Evaluate(new Core.Options.PrintStatistics()))
                    {
                        Core.Statistics.Display();
                    }

                    Core.Log.DebugMessage("Exit code {0}", System.Environment.ExitCode);
                }
            }
        }

#if false
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
#endif
    }
}

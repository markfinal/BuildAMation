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
namespace Bam
{
    /// <summary>
    /// Command line tool main entry point
    /// </summary>
    class Program
    {
        static bool UseV2 = true;

        public static class CommandLineArgumentHelper
        {
            public static void
            PrintHelp()
            {
                Core.Log.Info("BuildAMation (Bam) v{0}", Core.State.VersionString);
                Core.Log.Info("© Mark Final, 2010-2015");
                Core.Log.Info("Licensed under BSD 3-clause. See License file.");
                Core.Log.Info("");
                Core.Log.Info("Syntax:");
                Core.Log.Info("    bam [[option[=value]]...]");
                Core.Log.Info("");
                // TODO: this does not cover any arguments in the package assembly
                var argumentTypes = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(Core.V2.ICommandLineArgument).IsAssignableFrom(p) && !p.IsAbstract);
                Core.Log.Info("Options");
                foreach (var argType in argumentTypes)
                {
                    var arg = System.Activator.CreateInstance(argType) as Core.V2.ICommandLineArgument;
                    if (arg is Core.V2.ICustomHelpText)
                    {
                        Core.Log.Info("{0}: {1}", (arg as Core.V2.ICustomHelpText).OptionHelp, arg.ContextHelp);
                    }
                    else
                    {
                        if (null == arg.ShortName)
                        {
                            Core.Log.Info("{0}: {1}", arg.LongName, arg.ContextHelp);
                        }
                        else
                        {
                            Core.Log.Info("{0} (or {1}): {2}", arg.LongName, arg.ShortName, arg.ContextHelp);
                        }
                    }
                }
            }
        }

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
                if (Core.V2.CommandLineProcessor.Evaluate(new Core.V2.PrintHelp()))
                {
                    CommandLineArgumentHelper.PrintHelp();
                    return;
                }

                if (Core.V2.CommandLineProcessor.Evaluate(new Core.V2.PrintVersion()))
                {
                    Core.Log.MessageAll(Core.State.VersionString);
                    return;
                }

                if (Core.V2.CommandLineProcessor.Evaluate(new Core.V2.CreateDebugProject()))
                {
                    V2.DebugProject.Create();
                    return;
                }

                // configure
                Core.State.BuildRoot = "build";
                Core.State.VerbosityLevel = Core.EVerboseLevel.Full;
                Core.State.CompileWithDebugSymbols = true;
                Core.State.BuildMode = Core.V2.CommandLineProcessor.Evaluate(new Core.V2.BuildMode());
                if (null == Core.State.BuildMode)
                {
                    throw new Core.Exception("No builder specified");
                }

                var debug = new Core.V2.Environment();
                debug.Configuration = Core.EConfiguration.Debug;

                var optimized = new Core.V2.Environment();
                optimized.Configuration = Core.EConfiguration.Optimized;

                try
                {
                    Core.V2.EntryPoint.Execute(new Core.Array<Core.V2.Environment>(debug/*, optimized*/));
                }
                catch (Bam.Core.Exception exception)
                {
                    Core.Log.ErrorMessage(exception.Message);
                    if (null != exception.InnerException)
                    {
                        Core.Log.ErrorMessage("Additional details:");
                        Core.Log.ErrorMessage(exception.InnerException.Message);
                    }
                    System.Environment.ExitCode = -1;
                }
                finally
                {
                    Core.Log.Info((0 == System.Environment.ExitCode) ? "\nBuild Succeeded" : "\nBuild Failed");
                    Core.Log.DebugMessage("Exit code {0}", System.Environment.ExitCode);
                }
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
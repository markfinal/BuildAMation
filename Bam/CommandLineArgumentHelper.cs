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
    /// Utility class for printing all things related to Bam help
    /// </summary>
    public static class CommandLineArgumentHelper
    {
        private static void
        PrintOptions(
            System.Collections.Generic.IEnumerable<System.Type> optionTypes)
        {
            var options = new Core.Array<Core.ICommandLineArgument>();
            foreach (var optType in optionTypes)
            {
                var arg = System.Activator.CreateInstance(optType) as Core.ICommandLineArgument;
                options.Add(arg);
            }

            foreach (var arg in options.OrderBy(key => key.LongName))
            {
                if (arg is Core.ICustomHelpText)
                {
                    Core.Log.Info("{0}: {1}", (arg as Core.ICustomHelpText).OptionHelp, arg.ContextHelp);
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
                Core.Log.Info("");
            }
        }

        /// <summary>
        /// Display Bam help, with a version header, and each command line argument from the Core.
        /// If the working directory is a package, compile the package and display any command line arguments
        /// defined in that and any dependent packages.
        /// </summary>
        public static void
        PrintHelp()
        {
            PrintVersion();
            Core.Log.Info("");
            Core.Log.Info("Syntax:");
            Core.Log.Info("    bam [[option[=value]]...]");
            Core.Log.Info("");

            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var argumentTypes = assemblies.SelectMany(s => s.GetTypes()).Where(p => typeof(Core.ICommandLineArgument).IsAssignableFrom(p) && !p.IsAbstract);
            try
            {
                if (Core.PackageUtilities.IsPackageDirectory(Core.Graph.Instance.ProcessState.WorkingDirectory))
                {
                    Core.Graph.Instance.BuildRoot = System.IO.Path.GetTempPath();
                    Core.PackageUtilities.CompilePackageAssembly(enforceBamAssemblyVersions: false, enableClean: false);
                    Core.PackageUtilities.LoadPackageAssembly();
                }
            }
            catch (Core.Exception)
            {
                // purposefully empty, as IsPackageDirectory can throw, but we can ignore it in this scenario
            }

            Core.Log.Info("General options");
            Core.Log.Info("===============");
            PrintOptions(argumentTypes);
            if (null != Core.Graph.Instance.ScriptAssembly)
            {
                var scriptArgs = Core.Graph.Instance.ScriptAssembly.GetTypes().Where(p => typeof(Core.ICommandLineArgument).IsAssignableFrom(p) && !p.IsAbstract);
                Core.Log.Info("Package specific options");
                Core.Log.Info("========================");
                PrintOptions(scriptArgs);
            }
        }

        /// <summary>
        /// Display the version information, CLR version, and location of Bam assemblies
        /// </summary>
        public static void
        PrintVersion()
        {
            Core.EntryPoint.PrintVersion(Core.EVerboseLevel.Info);
            var clrVersion = System.Environment.Version;
            var targetFrameworkVersion = Core.Graph.Instance.ProcessState.TargetFrameworkVersion;
            Core.Log.Message(Core.EVerboseLevel.Info,
                "Using CLR v{0} ({1}) with assemblies in {2}",
                clrVersion.ToString(),
                targetFrameworkVersion,
                Core.Graph.Instance.ProcessState.ExecutableDirectory);
        }

        /// <summary>
        /// Display the installation directory of Bam!
        /// </summary>
        public static void
        PrintInstallationDirectory()
        {
            Core.Log.Info(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }
    }
}

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
namespace Bam
{
    /// <summary>
    /// Utility class for creating a standalone debuggable VisualStudio C# project for the package
    /// A main.cs file is created procedurally, with predefined options, which can be edited by the
    /// developer to change how Bam is invoked (as an alternative to command line options).
    /// </summary>
    public static class DebugProject
    {
        private static void
        WriteEntryPoint(
            string path)
        {
            System.Func<int, string> indent = (level) =>
                {
                    if (0 == level)
                    {
                        return string.Empty;
                    }
                    return new string(' ', level * 4);
                };

            using (System.IO.TextWriter writer = new System.IO.StreamWriter(path))
            {
                writer.NewLine = "\n";
                writer.WriteLine($"{indent(0)}namespace Bam");
                writer.WriteLine($"{indent(0)}{{");
                writer.WriteLine($"{indent(1)}class Program");
                writer.WriteLine($"{indent(1)}{{");
                writer.WriteLine($"{indent(2)}static void Main(string[] args)");
                writer.WriteLine($"{indent(2)}{{");
                writer.WriteLine($"{indent(3)}// configure");
                writer.WriteLine($"{indent(3)}Core.Graph.Instance.VerbosityLevel = Core.EVerboseLevel.Full;");
                writer.WriteLine($"{indent(3)}Core.Graph.Instance.CompileWithDebugSymbols = true;");
                writer.WriteLine($"{indent(3)}Core.Graph.Instance.BuildRoot = \"debug_build\";");
                writer.WriteLine($"{indent(3)}Core.Graph.Instance.Mode = \"Native\";");
                writer.WriteLine($"{indent(3)}var debug = new Core.Environment");
                writer.WriteLine($"{indent(3)}{{");
                writer.WriteLine($"{indent(4)}Configuration = Core.EConfiguration.Debug");
                writer.WriteLine($"{indent(3)}}};");
                writer.WriteLine($"{indent(3)}var optimized = new Core.Environment");
                writer.WriteLine($"{indent(3)}{{");
                writer.WriteLine($"{indent(4)}Configuration = Core.EConfiguration.Optimized");
                writer.WriteLine($"{indent(3)}}};");
                writer.WriteLine($"{indent(3)}var activeConfigs = new Core.Array<Core.Environment>(debug, optimized);");
                writer.WriteLine($"{indent(3)}// execute");
                writer.WriteLine($"{indent(3)}try");
                writer.WriteLine($"{indent(3)}{{");
                writer.WriteLine($"{indent(4)}Core.EntryPoint.Execute(activeConfigs, packageAssembly: System.Reflection.Assembly.GetEntryAssembly());");
                writer.WriteLine($"{indent(3)}}}");
                writer.WriteLine($"{indent(3)}catch (Bam.Core.Exception exception)");
                writer.WriteLine($"{indent(3)}{{");
                writer.WriteLine($"{indent(4)}Core.Exception.DisplayException(exception);");
                writer.WriteLine($"{indent(4)}System.Environment.ExitCode = -1;");
                writer.WriteLine($"{indent(3)}}}");
                writer.WriteLine($@"{indent(3)}Core.Log.Info((0 == System.Environment.ExitCode) ? ""\nBuild Succeeded"" : ""\nBuild Failed"");");
                writer.WriteLine($@"{indent(3)}Core.Log.DebugMessage($""Exit code {{System.Environment.ExitCode}}"");");
                writer.WriteLine($"{indent(2)}}}");
                writer.WriteLine($"{indent(1)}}}");
                writer.WriteLine($"{indent(0)}}}");
            }
        }

        private static void
        WriteLaunchSettings(
            string path,
            string projectName,
            string commandLineArgs)
        {
            System.Func<int, string> indent = (level) =>
            {
                if (0 == level)
                {
                    return string.Empty;
                }
                return new string(' ', level * 4);
            };

            using (System.IO.TextWriter writer = new System.IO.StreamWriter(path))
            {
                writer.NewLine = "\n";
                writer.WriteLine($"{indent(0)}{{");
                writer.WriteLine($"{indent(1)}\"profiles\": {{");
                writer.WriteLine($"{indent(2)}\"{projectName}\": {{");
                writer.WriteLine($"{indent(3)}\"commandName\": \"Project\",");
                writer.WriteLine($"{indent(3)}\"commandLineArgs\": \"{commandLineArgs}\"");
                writer.WriteLine($"{indent(2)}}}");
                writer.WriteLine($"{indent(1)}}}");
                writer.WriteLine($"{indent(0)}}}");
            }
        }

        private static System.Collections.Generic.List<(string, string)>
        GetBamCoreNuGetReferences()
        {
            var loadedAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var core = loadedAssemblies.First(item => item.GetName().Name.Equals("Bam.Core"));
            var all = core.GetReferencedAssemblies();
            var filtered = all.Where(item =>
            {
                var publicKeyToken = string.Empty;
                for (var i = 0; i < item.GetPublicKeyToken().GetLength(0); i++)
                {
                    publicKeyToken += string.Format("{0:x2}", item.GetPublicKeyToken()[i]);
                }

                // if the public key token is not that for the .NET framework, must be a NuGet
                return !publicKeyToken.Equals("b03f5f7f11d50a3a", System.StringComparison.OrdinalIgnoreCase);
            });
            var coreNuGetReferences = new System.Collections.Generic.List<(string, string)>();
            foreach (var nuget in filtered)
            {
                coreNuGetReferences.Add((nuget.Name, nuget.Version.ToString()));
            }
            return coreNuGetReferences;
        }

        /// <summary>
        /// Create the debuggable project.
        /// </summary>
        public static void
        Create()
        {
            Core.Graph.Instance.SkipPackageSourceDownloads = true;
            var allowDuplicates = Core.CommandLineProcessor.Evaluate(new Core.Options.IncludeAllPackageVersions());
            Core.PackageUtilities.IdentifyAllPackages(
                allowDuplicates: allowDuplicates
            );

            var masterPackage = Core.Graph.Instance.MasterPackage;
            var projectPathname = masterPackage.GetDebugPackageProjectPathname();
            var project = new Core.ProjectFile(
                true,
                projectPathname,
                additionalNuGetReferences: GetBamCoreNuGetReferences()
            );
            project.AddEntryPoint("main.cs", WriteEntryPoint);
            project.AddEmbeddedResource(Core.PackageListResourceFile.WriteResXFile);
            project.AddLaunchSettings(
                System.IO.Path.Combine("Properties", "launchSettings.json"),
                projectPathname,
                System.Environment.GetCommandLineArgs().Skip(1).SkipWhile(item => item == "-p" || item == "--createdebugproject"), // skip assembly name and the option to create the debug project
                WriteLaunchSettings);
            project.Write();

            Core.Log.Info($"Successfully created debug project for package '{masterPackage.FullName}'");
            Core.Log.Info($"\t{projectPathname}");
            if (allowDuplicates)
            {
                Core.Log.Info("NOTE: All package versions were included in this project. The project may not build due to duplicate symbols.");
            }
        }
    }
}

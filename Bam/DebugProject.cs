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
                writer.WriteLine("{0}namespace Bam", indent(0));
                writer.WriteLine("{0}{{", indent(0));
                writer.WriteLine("{0}class Program", indent(1));
                writer.WriteLine("{0}{{", indent(1));
                writer.WriteLine("{0}static void Main(string[] args)", indent(2));
                writer.WriteLine("{0}{{", indent(2));
                writer.WriteLine("{0}// configure", indent(3));
                writer.WriteLine("{0}Core.Graph.Instance.VerbosityLevel = Core.EVerboseLevel.Full;", indent(3));
                writer.WriteLine("{0}Core.Graph.Instance.CompileWithDebugSymbols = true;", indent(3));
                writer.WriteLine("{0}Core.Graph.Instance.BuildRoot = \"debug_build\";", indent(3));
                writer.WriteLine("{0}Core.Graph.Instance.Mode = \"Native\";", indent(3));
                writer.WriteLine("{0}var debug = new Core.Environment();", indent(3));
                writer.WriteLine("{0}debug.Configuration = Core.EConfiguration.Debug;", indent(3));
                writer.WriteLine("{0}var optimized = new Core.Environment();", indent(3));
                writer.WriteLine("{0}optimized.Configuration = Core.EConfiguration.Optimized;", indent(3));
                writer.WriteLine("{0}var activeConfigs = new Core.Array<Core.Environment>(debug, optimized);", indent(3));
                writer.WriteLine("{0}// execute", indent(3));
                writer.WriteLine("{0}try", indent(3));
                writer.WriteLine("{0}{{", indent(3));
                writer.WriteLine("{0}Core.EntryPoint.Execute(activeConfigs, packageAssembly: System.Reflection.Assembly.GetEntryAssembly());", indent(4));
                writer.WriteLine("{0}}}", indent(3));
                writer.WriteLine("{0}catch (Bam.Core.Exception exception)", indent(3));
                writer.WriteLine("{0}{{", indent(3));
                writer.WriteLine("{0}Core.Exception.DisplayException(exception);", indent(4));
                writer.WriteLine("{0}System.Environment.ExitCode = -1;", indent(4));
                writer.WriteLine("{0}}}", indent(3));
                writer.WriteLine(@"{0}Core.Log.Info((0 == System.Environment.ExitCode) ? ""\nBuild Succeeded"" : ""\nBuild Failed"");", indent(3));
                writer.WriteLine(@"{0}Core.Log.DebugMessage(""Exit code {{0}}"", System.Environment.ExitCode);", indent(3));
                writer.WriteLine("{0}}}", indent(2));
                writer.WriteLine("{0}}}", indent(1));
                writer.WriteLine("{0}}}", indent(0));
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
                if (item.Name == "System.IO.Compression")
                {
                    // SharpCompress depends on this assembly, but it's out of date (v4.2.1)
                    // and will generate an error for each BAM debug project generated
                    // so just don't include it - we're never going to step into it
                    return false;
                }

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
            Core.PackageUtilities.IdentifyAllPackages(false);

            var masterPackage = Core.Graph.Instance.MasterPackage;
            var projectPathname = masterPackage.GetDebugPackageProjectPathname();
            var project = new Core.ProjectFile(
                true,
                projectPathname,
                additionalNuGetReferences: GetBamCoreNuGetReferences()
            );
            project.AddEntryPoint("main.cs", WriteEntryPoint);
            project.AddEmbeddedResource(Core.PackageListResourceFile.WriteResXFile);
            project.Write();

            Core.Log.Info("Successfully created debug project for package '{0}'", masterPackage.FullName);
            Core.Log.Info("\t{0}", projectPathname);
        }
    }
}

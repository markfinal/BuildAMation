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
namespace MakeFileBuilder
{
    // Notes:
    // A rule is target + prerequisities + receipe
    // A recipe is a collection of commands
    public sealed class MakeFileCommonMetaData
    {
        /// <summary>
        /// Is NMAKE the MakeFile chosen format?
        /// </summary>
        public static bool IsNMAKE = ("NMAKE".Equals(Bam.Core.CommandLineProcessor.Evaluate(new Options.ChooseFormat()), System.StringComparison.Ordinal));

        /// <summary>
        /// A fake Target for order only dependencies
        /// </summary>
        public static Target DIRSTarget = new Target(
            Bam.Core.TokenizedString.CreateVerbatim("$(DIRS)"),
            false,
            null,
            null,
            0,
            string.Empty,
            false
        );

        public MakeFileCommonMetaData()
        {
            this.Directories = new Bam.Core.StringArray();
            this.Environment = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
            this.PackageVariables = new System.Collections.Generic.Dictionary<string, string>();
            if (Bam.Core.OSUtilities.IsLinuxHosting)
            {
                // for system utilities, e.g. mkdir, cp, echo
                this.Environment.Add("PATH", new Bam.Core.StringArray("/bin"));
                // for some tools, e.g. as
                this.Environment["PATH"].Add("/usr/bin");
            }
        }

        private Bam.Core.StringArray Directories { get; set; }
        private System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> Environment { get; set; }
        private System.Collections.Generic.Dictionary<string, string> PackageVariables { get; set; }

        /// <summary>
        /// Add key-value pairs (using strings and TokenizedStringArrays) which represent
        /// an environment variable name, and its value (usually multiple paths), to be used
        /// when the MakeFile is executed.
        /// Duplicates values are not added.
        /// </summary>
        /// <param name="import">The dictionary of key-value pairs to add.</param>
        public void
        ExtendEnvironmentVariables(
            System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray> import)
        {
            if (null == import)
            {
                return;
            }
            lock (this.Environment)
            {
                foreach (var env in import)
                {
                    if (!this.Environment.ContainsKey(env.Key))
                    {
                        this.Environment.Add(env.Key, new Bam.Core.StringArray());
                    }
                    foreach (var path in env.Value)
                    {
                        this.Environment[env.Key].AddUnique(path.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Add a directory to those to be created when running the MakeFile.
        /// Duplicates are not added.
        /// </summary>
        /// <param name="path">Path to the directory to be added.</param>
        public void
        AddDirectory(
            string path)
        {
            lock (this.Directories)
            {
                // at least mingw32-make does not like trailing slashes
                this.Directories.AddUnique(path.TrimEnd(System.IO.Path.DirectorySeparatorChar));
            }
        }

        /// <summary>
        /// Write the environment variables to be exported to the MakeFile.
        /// </summary>
        /// <param name="output">Where to write the environment variables to.</param>
        public void
        ExportEnvironment(
            System.Text.StringBuilder output)
        {
            System.Diagnostics.Debug.Assert(!MakeFileCommonMetaData.IsNMAKE);
            foreach (var env in this.Environment)
            {
                output.AppendLine(
                    $"{env.Key}:={this.UseMacrosInPath(env.Value.ToString(System.IO.Path.PathSeparator))}"
                );
            }
        }

        /// <summary>
        /// Write a phony target that sets the environment variables.
        /// Required for NMAKE since macros are not exported as environment variables.
        /// </summary>
        /// <param name="output"></param>
        public void
        ExportEnvironmentAsPhonyTarget(
            System.Text.StringBuilder output)
        {
            System.Diagnostics.Debug.Assert(MakeFileCommonMetaData.IsNMAKE);
            output.AppendLine(".PHONY: nmakesetenv");
            output.AppendLine("nmakesetenv:");
            foreach (var env in this.Environment)
            {
                // trim the end of 'continuation" characters
                output.AppendLine($"\t@set {env.Key}={env.Value.ToString(System.IO.Path.PathSeparator).TrimEnd(new[] { System.IO.Path.DirectorySeparatorChar })}");
            }
            output.AppendLine();
        }

        /// <summary>
        /// Write the directories to be created when running the MakeFile.
        /// </summary>
        /// <param name="output">Where to write the directories to.</param>
        /// <param name="explicitlyCreateHierarchy">Optional bool indicating that the entire directory hierarchy needs to be make. Defaults to false.</param>
        /// <returns>True if directories were exported, false if none were.</returns>
        public bool
        ExportDirectories(
            System.Text.StringBuilder output,
            bool explicitlyCreateHierarchy = false)
        {
            if (!this.Directories.Any())
            {
                return false;
            }
            if (this.Directories.Any(item => item.Contains(" ")))
            {
                // http://savannah.gnu.org/bugs/?712
                // https://stackoverflow.com/questions/9838384/can-gnu-make-handle-filenames-with-spaces
                Bam.Core.Log.ErrorMessage("WARNING: MakeFiles do not support spaces in pathnames.");
            }
            if (explicitlyCreateHierarchy)
            {
                var extraDirs = new Bam.Core.StringArray();
                foreach (var dir in this.Directories)
                {
                    var current_dir = dir;
                    for (;;)
                    {
                        var parent = System.IO.Path.GetDirectoryName(current_dir);
                        if (null == parent)
                        {
                            break;
                        }
                        if (!System.IO.Directory.Exists(parent) && !this.Directories.Contains(parent))
                        {
                            extraDirs.AddUnique(parent);
                            current_dir = parent;
                            continue;
                        }
                        break;
                    }
                }
                this.Directories.AddRange(extraDirs);
                this.Directories = new Bam.Core.StringArray(this.Directories.OrderBy(item => item.Length));
            }
            if (IsNMAKE)
            {
                output.Append("DIRS = ");
            }
            else
            {
                output.Append("DIRS:=");
            }
            foreach (var dir in this.Directories)
            {
                output.Append($"{this.UseMacrosInPath(dir)} ");
            }
            output.AppendLine();
            if (IsNMAKE)
            {
                output.AppendLine();
            }
            return true;
        }

        /// <summary>
        /// Get the variable name for a package's directory
        /// </summary>
        /// <param name="packageName">Name of package</param>
        /// <returns>Variable name</returns>
        public static string
        VariableForPackageDir(
            string packageName)
        {
            return $"{packageName}_DIR";
        }

        private void
        AppendVariable(
            System.Text.StringBuilder output,
            string path,
            string variableName)
        {
            if (this.PackageVariables.ContainsKey(path))
            {
                if (this.PackageVariables[path].Equals(variableName, System.StringComparison.Ordinal))
                {
                    return;
                }
                if (variableName.EndsWith(".tests_DIR", System.StringComparison.Ordinal))
                {
                    // this is a package test namespace
                    return;
                }

                // unexpectedly, due to paths being the primary lookup, there is no need to add an alias here
                // as it's always resolved to the original variable
                Bam.Core.Log.DebugMessage($"Path '{path}' is already registered with macro '{this.PackageVariables[path]}'. Ignoring the alias '{variableName}'");
                return;
            }
            if (IsNMAKE)
            {
                output.Append($"{variableName} = {path}");
            }
            else
            {
                output.Append(
                    $"{variableName} := {path}"
                );
            }
            output.AppendLine();
            this.PackageVariables.Add(path, $"$({variableName})");
        }

        /// <summary>
        /// Export all package directories
        /// </summary>
        /// <param name="output">StringBuilder to write to</param>
        /// <param name="packageMap">Map of packages</param>
        public void
        ExportPackageDirectories(
            System.Text.StringBuilder output,
            System.Collections.Generic.Dictionary<string, string> packageMap)
        {
            this.AppendVariable(output, Bam.Core.Graph.Instance.Macros["buildroot"].ToString(), "BUILDROOT");
            foreach (var pkg in packageMap)
            {
                var packageVar = VariableForPackageDir(pkg.Key);
                this.AppendVariable(output, pkg.Value, packageVar);
            }
        }

        /// <summary>
        /// Replace a Make string with macros that are appropriate
        /// </summary>
        /// <param name="path">Path to replace</param>
        /// <returns>Make macroised path</returns>
        public string
        UseMacrosInPath(
            string path)
        {
            foreach (var pkg in this.PackageVariables)
            {
                if (!path.Contains(pkg.Key))
                {
                    continue;
                }
                path = path.Replace(pkg.Key, pkg.Value);
            }
            return path;
        }
    }
}

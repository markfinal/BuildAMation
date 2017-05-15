#region License
// Copyright (c) 2010-2017, Mark Final
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
        public MakeFileCommonMetaData()
        {
            this.Directories = new Bam.Core.StringArray();
            this.Environment = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
            if (Bam.Core.OSUtilities.IsLinuxHosting)
            {
                // for system utilities, e.g. mkdir, cp, echo
                this.Environment.Add("PATH", new Bam.Core.StringArray("/bin"));
                // for some tools, e.g. as
                this.Environment["PATH"].Add("/usr/bin");
            }
        }

        private Bam.Core.StringArray Directories
        {
            get;
            set;
        }

        private System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> Environment
        {
            get;
            set;
        }

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
            lock (this)
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
            lock (this)
            {
                this.Directories.AddUnique(path);
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
            foreach (var env in this.Environment)
            {
                output.AppendFormat("{0}:={1}", env.Key, env.Value.ToString(System.IO.Path.PathSeparator));
                output.AppendLine();
            }
        }

        /// <summary>
        /// Write the directories to be created when running the MakeFile.
        /// </summary>
        /// <param name="output">Where to write the directories to.</param>
        public void
        ExportDirectories(
            System.Text.StringBuilder output)
        {
            if (!this.Directories.Any())
            {
                return;
            }
            output.Append("DIRS:=");
            foreach (var dir in this.Directories)
            {
                output.AppendFormat("{0} ", dir);
            }
            output.AppendLine();
        }
    }
}

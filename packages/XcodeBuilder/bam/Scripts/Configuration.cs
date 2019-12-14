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
namespace XcodeBuilder
{
    /// <summary>
    /// Project configurations
    /// </summary>
    sealed class ProjectConfiguration :
        Configuration
    {
        /// <summary>
        /// Create an instance of the ProjectConfiguration.
        /// </summary>
        /// <param name="config">The Bam EConfiguration corresponding to this Configuration.</param>
        /// <param name="project">The Project that the Configuration belongs.</param>
        public ProjectConfiguration(
            Bam.Core.EConfiguration config,
            Project project)
            :
            base(config, project)
        { }
    }

    /// <summary>
    /// Target configurations
    /// </summary>
    sealed class TargetConfiguration :
        Configuration
    {
        /// <summary>
        /// Create an instance of the TargetConfiguration.
        /// </summary>
        /// <param name="config">The Bam EConfiguration corresponding to this Configuration.</param>
        /// <param name="target">The Target that the Configuration belongs to.</param>
        public TargetConfiguration(
            Bam.Core.EConfiguration config,
            Target target)
            :
            base(config, target.Project, target)
        { }
    }

    /// <summary>
    /// Class corresponding to the XCBuildConfiguration in an Xcode project.
    /// </summary>
    abstract class Configuration :
        Object
    {
        private readonly System.Collections.Generic.Dictionary<string, ConfigurationValue> Settings = new System.Collections.Generic.Dictionary<string, ConfigurationValue>();

        /// <summary>
        /// Create an instance of the Configuration.
        /// </summary>
        /// <param name="config">The Bam EConfiguration corresponding to this Configuration.</param>
        /// <param name="project">The Project that the Configuration belongs.</param>
        /// <param name="target">Optional Target for the Configuration. The default is null, indicating a Project configuration.</param>
        protected Configuration(
            Bam.Core.EConfiguration config,
            Project project,
            Target target = null)
            :
            base(project, config.ToString(), "XCBuildConfiguration", project.GUID, target?.GUID)
        {
            this.Config = config;
            this.BuildFiles = new Bam.Core.Array<BuildFile>();
        }

        /// <summary>
        /// Get the EConfiguration associated with this Configuration.
        /// </summary>
        public Bam.Core.EConfiguration Config { get; private set; }

        private void
        AppendBuildCommands(
            Bam.Core.StringArray commands,
            Bam.Core.TokenizedStringArray outputPaths,
            System.Collections.Generic.IEnumerable<string> directories,
            string prefix,
            Bam.Core.Array<ShellScriptBuildPhase> collection)
        {
            lock (collection)
            {
                if (!collection.Any() ||
                    (null != outputPaths) && !collection.Last().HasSufficientSpace(outputPaths.Count))
                {
                    var name = $"{prefix} {this.Config.ToString()}";
                    if (collection.Any())
                    {
                        name += $" {collection.Count + 1}";
                    }
                    var buildPhase = new ShellScriptBuildPhase(this, name);
                    collection.Add(buildPhase);
                    this.Project.AppendShellScriptsBuildPhase(buildPhase);
                }

                var current = collection.Last();
                current.AddCommands(commands);
                current.AddOutputPaths(outputPaths);
                current.AddDirectories(directories);
            }
        }

        private readonly Bam.Core.Array<ShellScriptBuildPhase> PreBuildBuildPhases = new Bam.Core.Array<ShellScriptBuildPhase>();

        /// <summary>
        /// Enumerate all pre-build build phases
        /// </summary>
        public System.Collections.Generic.IEnumerable<ShellScriptBuildPhase> EnumeratePreBuildBuildPhases
        {
            get
            {
                return this.PreBuildBuildPhases;
            }
        }

        /// <summary>
        /// Append pre-build commands to this configuration.
        /// </summary>
        /// <param name="commands">Array of shell commands.</param>
        /// <param name="outputPaths">Array of output paths generated from those commands</param>
        /// <param name="directories">Enumeration of directory strings to create</param>
        public void
        AppendPreBuildCommands(
            Bam.Core.StringArray commands,
            Bam.Core.TokenizedStringArray outputPaths,
            System.Collections.Generic.IEnumerable<string> directories)
        {
            this.AppendBuildCommands(
                commands,
                outputPaths,
                directories,
                "Pre Build",
                this.PreBuildBuildPhases
            );
        }

        private readonly Bam.Core.Array<ShellScriptBuildPhase> PostBuildBuildPhases = new Bam.Core.Array<ShellScriptBuildPhase>();

        /// <summary>
        /// Enumerate all post-build build phases
        /// </summary>
        public System.Collections.Generic.IEnumerable<ShellScriptBuildPhase> EnumeratePostBuildBuildPhases
        {
            get
            {
                return this.PostBuildBuildPhases;
            }
        }

        /// <summary>
        /// Append post-build commands to this configuration.
        /// </summary>
        /// <param name="commands">Array of shell commands.</param>
        /// <param name="outputPaths">Array of output paths generated from those commands</param>
        /// <param name="directories">Enumeration of directory strings to create</param>
        public void
        AppendPostBuildCommands(
            Bam.Core.StringArray commands,
            Bam.Core.TokenizedStringArray outputPaths,
            System.Collections.Generic.IEnumerable<string> directories)
        {
            this.AppendBuildCommands(
                commands,
                outputPaths,
                directories,
                "Post Build",
                this.PostBuildBuildPhases
            );
        }

        /// <summary>
        /// Get the list of BuildFiles for this Configuration.
        /// </summary>
        public Bam.Core.Array<BuildFile> BuildFiles { get; private set; }

        /// <summary>
        /// Get or set the settings for a given key in the Configuration.
        /// </summary>
        /// <param name="key">Named key in the settings.</param>
        /// <returns>Value associated with the key.</returns>
        public ConfigurationValue this[string key]
        {
            get
            {
                return this.Settings[key];
            }

            set
            {
                if (!this.Settings.ContainsKey(key))
                {
                    this.Settings[key] = value;
                }
                else
                {
                    if (!this.Settings[key].Merge(value))
                    {
                        Bam.Core.Log.Info($"Warning: Replacing previous value for key '{key}' with '{value.ToString()}'");
                    }
                }
            }
        }

        /// <summary>
        /// Set this Configuration's product name.
        /// </summary>
        /// <param name="productName">Name of the product.</param>
        public void
        SetProductName(
            Bam.Core.TokenizedString productName)
        {
            this["PRODUCT_NAME"] = new UniqueConfigurationValue(productName.ToString());
        }

        /// <summary>
        /// Serialize the Configuration.
        /// </summary>
        /// <param name="text">StringBuilder to write to.</param>
        /// <param name="indentLevel">Number of tabs to indent by.</param>
        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendLine($"{indent}{this.GUID} /* {this.Name} */ = {{");
            text.AppendLine($"{indent2}isa = {this.IsA};");
            text.AppendLine($"{indent2}buildSettings = {{");
            foreach (var setting in this.Settings.OrderBy(key => key.Key))
            {
                var value = setting.Value.ToString();
                if (null == value)
                {
                    continue;
                }
                text.AppendLine($"{indent3}{setting.Key} = {this.CleansePaths(value)};");
            }
            text.AppendLine($"{indent2}}};");
            text.AppendLine($"{indent2}name = {this.Name};");
            text.AppendLine($"{indent}}};");
        }
    }
}

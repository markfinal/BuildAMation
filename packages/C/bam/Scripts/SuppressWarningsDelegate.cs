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
namespace C
{
    /// <summary>
    /// An abstract class for package authors to inherit from in order to reduce the amount of boiler plate code
    /// needed to manage warning suppressions. This is intended for foreign code that may not have been compiled
    /// at the warning levels that package authors prefer.
    /// A SuppressWarningsDelegate can work in conjunction with regular patches suppression warnings; it is simply
    /// a more concise way of expressing many suppressions.
    /// Each SuppressWarningsDelegate derivation is intended to be compiler specific since you can do compiler major
    /// version comparisons in order to selectively add warning suppressions. You may also do BAM configuration specific
    /// suppressions exclusively, or in conjunction with compiler major version comparisons.
    /// Derived classes should construct a mapping of source path (in the source collection applied to) to warning suppressions
    /// in the derived constructor, using the helper functions in SuppressWarningsDelegate.
    /// </summary>
    public abstract class SuppressWarningsDelegate
    {
        private class Conditions
        {
            private readonly ToolchainVersion minimum_compiler_version;
            private readonly ToolchainVersion maximum_compiler_version;
            private readonly Bam.Core.EConfiguration? matching_configurations;
            private readonly EBit? matching_bit_depths;

            public Conditions(
                ToolchainVersion min_version,
                ToolchainVersion max_version)
            {
                this.minimum_compiler_version = min_version;
                this.maximum_compiler_version = max_version;
            }

            public Conditions(
                Bam.Core.EConfiguration matching_config)
            {
                this.matching_configurations = matching_config;
            }

            public Conditions(
                EBit matching_bit_depth)
            {
                this.matching_bit_depths = matching_bit_depth;
            }

            public Conditions(
                Bam.Core.EConfiguration matching_config,
                EBit matching_bit_depth)
                :
                this(matching_config)
            {
                this.matching_bit_depths = matching_bit_depth;
            }

            public Conditions(
                ToolchainVersion min_version,
                ToolchainVersion max_version,
                Bam.Core.EConfiguration matching_config)
                :
                this(min_version, max_version)
            {
                this.matching_configurations = matching_config;
            }

            public Conditions(
                ToolchainVersion min_version,
                ToolchainVersion max_version,
                EBit matching_bit_depth)
                :
                this(min_version, max_version)
            {
                this.matching_bit_depths = matching_bit_depth;
            }

            public Conditions(
                ToolchainVersion min_version,
                ToolchainVersion max_version,
                Bam.Core.EConfiguration matching_config,
                EBit matching_bit_depth)
                :
                this(min_version, max_version)
            {
                this.matching_configurations = matching_config;
                this.matching_bit_depths = matching_bit_depth;
            }

            public bool
            Match(
                CompilerTool compilerTool,
                Bam.Core.Environment environment,
                EBit bitdepth)
            {
                if (null != this.minimum_compiler_version)
                {
                    if (null != this.maximum_compiler_version)
                    {
                        if (!compilerTool.Version.InRange(this.minimum_compiler_version, this.maximum_compiler_version))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!compilerTool.Version.AtLeast(this.minimum_compiler_version))
                        {
                            return false;
                        }
                    }
                }
                else if (null != this.maximum_compiler_version)
                {
                    if (!compilerTool.Version.AtMost(this.maximum_compiler_version))
                    {
                        return false;
                    }
                }

                if (this.matching_configurations.HasValue && (0 == (environment.Configuration & this.matching_configurations.Value)))
                {
                    return false;
                }

                if (this.matching_bit_depths.HasValue && (bitdepth != this.matching_bit_depths.Value))
                {
                    return false;
                }

                return true;
            }
        }

        private readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Conditions>> suppressions = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Conditions>>();

        private void
        Merge(
            string path,
            System.Collections.Generic.Dictionary<string, Conditions> newSuppressions)
        {
            if (!this.suppressions.ContainsKey(path))
            {
                this.suppressions.Add(path, newSuppressions);
            }
            else
            {
                this.suppressions[path] = this.suppressions[path].Union(newSuppressions).ToDictionary(k => k.Key, v => v.Value);
            }
        }

        /// <summary>
        /// Add suppressions for a path
        /// </summary>
        /// <param name="path">Path to suppress warnings for</param>
        /// <param name="suppression">List of suppression</param>
        protected void
        Add(
            string path,
            params string[] suppression)
        {
            var warnings = new System.Collections.Generic.Dictionary<string, Conditions>();
            foreach (var sup in suppression)
            {
                warnings.Add(sup, null);
            }
            this.Merge(path, warnings);
        }

        /// <summary>
        /// Add suppressions for a path, applied when the toolchain version resides in the specified range. 
        /// </summary>
        /// <param name="path">Path to suppress warnings for</param>
        /// <param name="minVersion">Minimum toolchain version</param>
        /// <param name="maxVersion">Maximum toolchain version</param>
        /// <param name="suppression">List of suppressions.</param>
        protected void
        Add(
            string path,
            ToolchainVersion minVersion,
            ToolchainVersion maxVersion,
            params string[] suppression)
        {
            var warnings = new System.Collections.Generic.Dictionary<string, Conditions>();
            foreach (var sup in suppression)
            {
                warnings.Add(sup, new Conditions(minVersion, maxVersion));
            }
            this.Merge(path, warnings);
        }

        /// <summary>
        /// Add suppressions for a path, on a given configuration
        /// </summary>
        /// <param name="path">Path to suppress warnings for</param>
        /// <param name="config">Configuration to apply suppressions</param>
        /// <param name="suppression">List of suppressions.</param>
        protected void
        Add(
            string path,
            Bam.Core.EConfiguration config,
            params string[] suppression)
        {
            var warnings = new System.Collections.Generic.Dictionary<string, Conditions>();
            foreach (var sup in suppression)
            {
                warnings.Add(sup, new Conditions(config));
            }
            this.Merge(path, warnings);
        }

        /// <summary>
        /// Add suppressions for a path, on a given bitdepth
        /// </summary>
        /// <param name="path">Path to suppress warnings for</param>
        /// <param name="bitdepth">Bit depth to apply suppressions</param>
        /// <param name="suppression">List of suppressions</param>
        protected void
        Add(
            string path,
            EBit bitdepth,
            params string[] suppression)
        {
            var warnings = new System.Collections.Generic.Dictionary<string, Conditions>();
            foreach (var sup in suppression)
            {
                warnings.Add(sup, new Conditions(bitdepth));
            }
            this.Merge(path, warnings);
        }

        /// <summary>
        /// Add suppressions for a path, on a given bitdepth and configuration.
        /// </summary>
        /// <param name="path">Path to suppress warnings for</param>
        /// <param name="config">Configuration to apply suppressions for</param>
        /// <param name="bitdepth">Bit depth to apply suppressions for</param>
        /// <param name="suppression">List of suppressions</param>
        protected void
        Add(
            string path,
            Bam.Core.EConfiguration config,
            EBit bitdepth,
            params string[] suppression)
        {
            var warnings = new System.Collections.Generic.Dictionary<string, Conditions>();
            foreach (var sup in suppression)
            {
                warnings.Add(sup, new Conditions(config, bitdepth));
            }
            this.Merge(path, warnings);
        }

        /// <summary>
        /// Add suppressions for a path, on a given range of toolchain versions and configuration
        /// </summary>
        /// <param name="path">Path to suppress warnings for</param>
        /// <param name="minVersion">Minimum toolchain version to apply</param>
        /// <param name="maxVersion">Maximum toolchain version to apply</param>
        /// <param name="config">Configuration to apply suppressions for</param>
        /// <param name="suppression">List of suppressions</param>
        protected void
        Add(
            string path,
            ToolchainVersion minVersion,
            ToolchainVersion maxVersion,
            Bam.Core.EConfiguration config,
            params string[] suppression)
        {
            var warnings = new System.Collections.Generic.Dictionary<string, Conditions>();
            foreach (var sup in suppression)
            {
                warnings.Add(sup, new Conditions(minVersion, maxVersion, config));
            }
            this.Merge(path, warnings);
        }

        /// <summary>
        /// Add suppressions for a path, on a given range of toolchain versions, and bitdepth
        /// </summary>
        /// <param name="path">Path to suppress warnings for</param>
        /// <param name="minVersion">Minimum toolchain version to apply</param>
        /// <param name="maxVersion">Maximum toolchain version to apply</param>
        /// <param name="bitdepth">Bit depth to apply suppressions for</param>
        /// <param name="suppression">List of suppressions</param>
        protected void
        Add(
            string path,
            ToolchainVersion minVersion,
            ToolchainVersion maxVersion,
            C.EBit bitdepth,
            params string[] suppression)
        {
            var warnings = new System.Collections.Generic.Dictionary<string, Conditions>();
            foreach (var sup in suppression)
            {
                warnings.Add(sup, new Conditions(minVersion, maxVersion, bitdepth));
            }
            this.Merge(path, warnings);
        }

        /// <summary>
        /// Add suppressions for a path, on a given range of toolchain versions, and bitdepth, and configuration
        /// </summary>
        /// <param name="path">Path to suppress warnings for</param>
        /// <param name="minVersion">Minimum toolchain version</param>
        /// <param name="maxVersion">Maximum toolchain version</param>
        /// <param name="config">Configuration apply suppressions to</param>
        /// <param name="bitdepth">Bit depth to apply suppressions to</param>
        /// <param name="suppression">List of suppressions</param>
        protected void
        Add(
            string path,
            ToolchainVersion minVersion,
            ToolchainVersion maxVersion,
            Bam.Core.EConfiguration config,
            EBit bitdepth,
            params string[] suppression)
        {
            var warnings = new System.Collections.Generic.Dictionary<string, Conditions>();
            foreach (var sup in suppression)
            {
                warnings.Add(sup, new Conditions(minVersion, maxVersion, config, bitdepth));
            }
            this.Merge(path, warnings);
        }

        /// <summary>
        /// Execute the suppression delegate on this container of modules
        /// </summary>
        /// <typeparam name="ChildModuleType">Type of Module stored in the container</typeparam>
        /// <param name="module">Container module to apply to</param>
        public void
        Execute<ChildModuleType>(
            CModuleContainer<ChildModuleType> module) where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
        {
            foreach (var item in this.suppressions)
            {
                module[item.Key].ForEach(sourceItem =>
                    {
                        sourceItem.PrivatePatch(settings =>
                        {
                            var compiler = settings as ICommonCompilerSettings;
                            foreach (var warning in item.Value)
                            {
                                if (null != warning.Value)
                                {
                                    var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                                        (settings.Module as CCompilableModuleContainer<ObjectFile>).Compiler :
                                        (settings.Module as ObjectFile).Compiler;
                                    if (warning.Value.Match(compilerUsed, sourceItem.BuildEnvironment, (sourceItem as CModule).BitDepth))
                                    {
                                        // compiler or configuration specific warning
                                        compiler.DisableWarnings.AddUnique(warning.Key);
                                    }
                                }
                                else
                                {
                                    // unconditional suppression
                                    compiler.DisableWarnings.AddUnique(warning.Key);
                                }
                            }
                        });
                    });
            }
        }
    }
}

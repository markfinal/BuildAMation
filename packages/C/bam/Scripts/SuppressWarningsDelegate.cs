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
using Bam.Core;
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
            private readonly ICompilerVersion minimum_compiler_version;
            private readonly ICompilerVersion maximum_compiler_version;
            private readonly Bam.Core.EConfiguration? matching_configurations;

            public Conditions(
                ICompilerVersion min_version,
                ICompilerVersion max_version)
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
                ICompilerVersion min_version,
                ICompilerVersion max_version,
                Bam.Core.EConfiguration matching_config)
            {
                this.minimum_compiler_version = min_version;
                this.maximum_compiler_version = max_version;
                this.matching_configurations = matching_config;
            }

            public bool
            Match(
                CompilerTool compilerTool,
                Bam.Core.Environment environment)
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
                return true;
            }
        }

        private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Conditions>> suppressions = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Conditions>>();

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

        protected void
        Add(
            string path,
            ICompilerVersion minCompilerVersion,
            ICompilerVersion maxCompilerVersion,
            params string[] suppression)
        {
            var warnings = new System.Collections.Generic.Dictionary<string, Conditions>();
            foreach (var sup in suppression)
            {
                warnings.Add(sup, new Conditions(minCompilerVersion, maxCompilerVersion));
            }
            this.Merge(path, warnings);
        }

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

        protected void
        Add(
            string path,
            ICompilerVersion minCompilerVersion,
            ICompilerVersion maxCompilerVersion,
            Bam.Core.EConfiguration config,
            params string[] suppression)
        {
            var warnings = new System.Collections.Generic.Dictionary<string, Conditions>();
            foreach (var sup in suppression)
            {
                warnings.Add(sup, new Conditions(minCompilerVersion, maxCompilerVersion, config));
            }
            this.Merge(path, warnings);
        }

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
                            var compiler = settings as C.ICommonCompilerSettings;
                            foreach (var warning in item.Value)
                            {
                                if (null != warning.Value)
                                {
                                    var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                                        (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                                        (settings.Module as C.ObjectFile).Compiler;
                                    if (warning.Value.Match(compilerUsed, sourceItem.BuildEnvironment))
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

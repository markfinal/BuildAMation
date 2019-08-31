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
    /// Base class for many C modules.
    /// Defines versioning macros.
    /// Defines the default bit-depth.
    /// </summary>
    abstract class CModule :
        Bam.Core.Module
    {
        /// <summary>
        /// Array of modules representing the header files
        /// </summary>
        protected Bam.Core.Array<Bam.Core.Module> headerModules = new Bam.Core.Array<Bam.Core.Module>();

        /// <summary>
        /// Construct an instance.
        /// These are common states that need to be set before \a any Init function, from any derived Module,
        /// is called.
        /// </summary>
        protected CModule()
        {
            this.SetSemanticVersion(1, 0, 0);
            // default bit depth
            this.BitDepth = (EBit)Bam.Core.CommandLineProcessor.Evaluate(new Options.DefaultBitDepth());
        }

        /// <summary>
        /// Set the semantic version of this module
        /// </summary>
        /// <param name="major">Major version</param>
        /// <param name="minor">Minor version</param>
        /// <param name="patch">Patch version</param>
        protected void
        SetSemanticVersion(
            int major,
            int minor,
            int patch) => this.SetSemanticVersion(major.ToString(), minor.ToString(), patch.ToString());

        /// <summary>
        /// Set the semantic version of this module
        /// </summary>
        /// <param name="major">Major version</param>
        /// <param name="minor">Minor version</param>
        protected void
        SetSemanticVersion(
            int major,
            int minor) => this.SetSemanticVersion(major.ToString(), minor.ToString(), null);

        /// <summary>
        /// Set the semantic version of this module
        /// </summary>
        /// <param name="major">Major version</param>
        protected void
        SetSemanticVersion(
            int major) => this.SetSemanticVersion(major.ToString(), null, null);

        /// <summary>
        /// Set the semantic version of this module
        /// </summary>
        /// <param name="version">ISemanticVersion instance</param>
        protected void
        SetSemanticVersion(
            Bam.Core.ISemanticVersion version)
        {
            this.SetSemanticVersion(
                version.MajorVersion.Value.ToString(), // assumed non-null
                version.MinorVersion.HasValue ? version.MinorVersion.Value.ToString() : null,
                version.PatchVersion.HasValue ? version.PatchVersion.Value.ToString() : null
            );
        }

        /// <summary>
        /// Set the semantic version of this module
        /// </summary>
        /// <param name="major">Major version</param>
        /// <param name="minor">Minor version</param>
        /// <param name="patch">Patch version</param>
        protected void
        SetSemanticVersion(
            string major,
            string minor,
            string patch)
        {
            this.Macros[ModuleMacroNames.MajorVersion] = Bam.Core.TokenizedString.CreateVerbatim(major);
            if (minor != null)
            {
                this.Macros[ModuleMacroNames.MinorVersion] = Bam.Core.TokenizedString.CreateVerbatim(minor);
            }
            else
            {
                this.Macros.Remove(ModuleMacroNames.MinorVersion);
            }
            if (patch != null)
            {
                this.Macros.Add(ModuleMacroNames.PatchVersion, Bam.Core.TokenizedString.CreateVerbatim(patch));
            }
            else
            {
                this.Macros.Remove(ModuleMacroNames.PatchVersion);
            }
        }

        /// <summary>
        /// Query whether the module has the C.Prebuilt attribute assigned to it.
        /// </summary>
        public bool IsPrebuilt { get; private set; }

        /// <summary>
        /// Query whether the module has the C.Thirdparty attribute assigned to it, and extract the Windows version .rc resource path.
        /// </summary>
        public string ThirdpartyWindowsVersionResourcePath { get; private set; }

        /// <summary>
        /// Initialize this CModule
        /// </summary>
        protected override void
        Init()
        {
            base.Init();

            this.IsPrebuilt = (this.GetType().GetCustomAttributes(typeof(PrebuiltAttribute), true).Length > 0);

            var thirdpartyAttrs = this.GetType().GetCustomAttributes(typeof(ThirdpartyAttribute), true) as ThirdpartyAttribute[];
            if (thirdpartyAttrs.Length > 0)
            {
                if (thirdpartyAttrs.Length > 1)
                {
                    throw new Bam.Core.Exception(
                        $"Too many {typeof(ThirdpartyAttribute).ToString()} attributes on {this.GetType().ToString()}"
                    );
                }
                this.ThirdpartyWindowsVersionResourcePath = thirdpartyAttrs[0].WindowsVersionResourcePath;
            }

            // if there is a parent from which this module is created, inherit bitdepth
            var graph = Bam.Core.Graph.Instance;
            if (!graph.ModuleStack.Empty)
            {
                var parentModule = graph.ModuleStack.Peek();
                this.BitDepth = (parentModule as CModule).BitDepth;
            }
        }

        /// <summary>
        /// Get or set this Module's bit depth.
        /// </summary>
        public EBit BitDepth { get; set; }

        /// <summary>
        /// Enumerate across the hierarchical tree of Modules.
        /// </summary>
        /// <param name="files">Array of input Modules</param>
        /// <returns>Flattened Module enumeration</returns>
        protected static System.Collections.Generic.IEnumerable<Bam.Core.Module>
        FlattenHierarchicalFileList(
            Bam.Core.Array<Bam.Core.Module> files)
        {
            foreach (var input in files)
            {
                if (input is Bam.Core.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        yield return child;
                    }
                }
                else
                {
                    yield return input;
                }
            }
        }

        /// <summary>
        /// Based on dependency checking, return a list of Modules in order of decreasing dependency.
        /// This is useful for single-pass linkers, in order to resolve all symbols.
        /// </summary>
        /// <param name="libs">Array of modules to sort.</param>
        /// <returns>List of Modules in decreasing order of dependencies</returns>
        protected static System.Collections.Generic.IEnumerable<Bam.Core.Module>
        OrderLibrariesWithDecreasingDependencies(
            Bam.Core.Array<Bam.Core.Module> libs)
        {
            // work on a copy of the flattened list of libraries, as the modules may be rearranged
            var flatLibs = new Bam.Core.Array<Bam.Core.Module>(libs);
            // now ensure that the order of the libraries is such that those with the least number of dependents
            // are at the end
            // this is O(N^2) and some modules may be moved more than once
            for (var i = 0; i < flatLibs.Count;)
            {
                var ontoNext = true;
                for (var j = i + 1; j < flatLibs.Count; ++j)
                {
                    if (!(flatLibs[j] is IForwardedLibraries))
                    {
                        continue;
                    }
                    // if any other module has the first as a dependent, move the dependent to the end
                    if ((flatLibs[j] as IForwardedLibraries).ForwardedLibraries.Contains(flatLibs[i]))
                    {
                        var temp = flatLibs[i];
                        flatLibs.Remove(temp);
                        flatLibs.Add(temp);
                        ontoNext = false;
                        break;
                    }
                }
                if (ontoNext)
                {
                    ++i;
                }
            }
            return flatLibs;
        }

        /// <summary>
        /// Create the collection.
        /// </summary>
        /// <typeparam name="T">Type of the colletion.</typeparam>
        /// <param name="requires">Children are added as a requirement rather than a dependency.</param>
        /// <param name="wildcardPath">Wildcarded path to find children.</param>
        /// <param name="macroModuleOverride">Optional module to serve as macro source. Default to null.</param>
        /// <param name="filter">Optional regular expression filter to remove matches from the path search. Default to null.</param>
        /// <param name="privatePatch">Optional private patch delegate to run against collection. Default to null.</param>
        /// <returns>The new collection Module</returns>
        [System.Obsolete("Please use InternalCreateCollection instead", true)]
        protected T
        InternalCreateContainer<T>(
            bool requires,
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null,
            Bam.Core.Module.PrivatePatchDelegate privatePatch = null)
            where T : CModule, IAddFiles, new()
        {
            return this.InternalCreateCollection<T>(requires, wildcardPath, macroModuleOverride, filter, privatePatch);
        }

        /// <summary>
        /// Create the collection.
        /// </summary>
        /// <typeparam name="T">Type of the colletion.</typeparam>
        /// <param name="requires">Children are added as a requirement rather than a dependency.</param>
        /// <param name="wildcardPath">Wildcarded path to find children.</param>
        /// <param name="macroModuleOverride">Optional module to serve as macro source. Default to null.</param>
        /// <param name="filter">Optional regular expression filter to remove matches from the path search. Default to null.</param>
        /// <param name="privatePatch">Optional private patch delegate to run against collection. Default to null.</param>
        /// <returns>The new collection Module</returns>
        protected T
        InternalCreateCollection<T>(
            bool requires,
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null,
            Bam.Core.Module.PrivatePatchDelegate privatePatch = null)
            where T : CModule, IAddFiles, new()
        {
            var source = Bam.Core.Module.Create<T>(this);
            if (null != privatePatch)
            {
                source.PrivatePatch(privatePatch);
            }

            if (requires)
            {
                this.Requires(source);
            }
            else
            {
                this.DependsOn(source);
            }

            if (null != wildcardPath)
            {
                (source as IAddFiles).AddFiles(wildcardPath, macroModuleOverride: macroModuleOverride, filter: filter);
            }

            return source;
        }

        /// <summary>
        /// Create a collection of header files.
        /// </summary>
        /// <param name="wildcardPath">Optional wildcarded path to search for matches. Default to null.</param>
        /// <param name="macroModuleOverride">Optional module to serve as macro source. Default to null.</param>
        /// <param name="filter">Optional regular expression to filter on the path search. Default to null.</param>
        /// <returns>A collection of header files.</returns>
        [System.Obsolete("Please use CreateHeaderCollection instead", true)]
        public HeaderFileCollection
        CreateHeaderContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            return this.CreateHeaderCollection(wildcardPath, macroModuleOverride, filter);
        }

        /// <summary>
        /// Create a collection of header files.
        /// </summary>
        /// <param name="wildcardPath">Optional wildcarded path to search for matches. Default to null.</param>
        /// <param name="macroModuleOverride">Optional module to serve as macro source. Default to null.</param>
        /// <param name="filter">Optional regular expression to filter on the path search. Default to null.</param>
        /// <returns>A collection of header files.</returns>
        public HeaderFileCollection
        CreateHeaderCollection(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var headers = this.InternalCreateCollection<HeaderFileCollection>(true, wildcardPath, macroModuleOverride, filter);
            this.headerModules.Add(headers);
            return headers;
        }

        /// <summary>
        /// Get a list of order only dependents.
        /// </summary>
        /// <returns>List of dependents.</returns>
        public System.Collections.Generic.IEnumerable<Bam.Core.Module>
        OrderOnlyDependents()
        {
            // order only dependencies - recurse into each, so that all layers
            // of order only dependencies are included
            var queue = new System.Collections.Generic.Queue<Bam.Core.Module>(this.Requirements);
            while (queue.Any())
            {
                var required = queue.Dequeue();
                foreach (var additional in required.Requirements)
                {
                    queue.Enqueue(additional);
                }

                yield return required;
            }
            // any non-C module projects should be order-only dependencies
            foreach (var dependent in this.Dependents)
            {
                if (dependent is C.CModule)
                {
                    continue;
                }
                yield return dependent;
            }
            if (this is IForwardedLibraries forwarded)
            {
                // however, there may be forwarded libraries, and these are useful order only dependents
                foreach (var dependent in forwarded.ForwardedLibraries)
                {
                    yield return dependent;
                }
            }
        }
    }
}

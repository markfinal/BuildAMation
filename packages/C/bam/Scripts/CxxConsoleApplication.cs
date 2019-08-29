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
using Bam.Core;
namespace C.Cxx
{
    /// <summary>
    /// Derive from this module to create a C++ application, linking against the C++ runtime library.
    /// </summary>
    public class ConsoleApplication :
        C.ConsoleApplication
    {
        /// <summary>
        /// Initialize the console application
        /// </summary>
        protected override void
        Init()
        {
            base.Init();
            this.Linker = C.DefaultToolchain.Cxx_Linker(this.BitDepth);
        }

        /// <summary>
        /// Create a collection whose matching sources compile against C++.
        /// </summary>
        /// <returns>The cxx source collection.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        [System.Obsolete("Please use CreateCxxSourceCollection", true)]
        public virtual Cxx.ObjectFileCollection
        CreateCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            return this.CreateCxxSourceCollection(wildcardPath, macroModuleOverride, filter);
        }

        /// <summary>
        /// Create a collection whose matching sources compile against C++.
        /// </summary>
        /// <returns>The cxx source collection.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public virtual Cxx.ObjectFileCollection
        CreateCxxSourceCollection(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var applicationPreprocessor = this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) ? this.ConsolePreprocessor : null;
            var source = this.InternalCreateCollection<Cxx.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, applicationPreprocessor);
            this.sourceModules.Add(source);
            return source;
        }

        /// <summary>
        /// Create a collection whose matching sources compile against Objective C++.
        /// </summary>
        /// <returns>The objective cxx source collection.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        [System.Obsolete("Please use CreateObjectiveCxxSourceCollection", true)]
        public C.ObjCxx.ObjectFileCollection
        CreateObjectiveCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            return this.CreateObjectiveCxxSourceCollection(wildcardPath, macroModuleOverride, filter);
        }

        /// <summary>
        /// Create a collection whose matching sources compile against Objective C++.
        /// </summary>
        /// <returns>The objective cxx source collection.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public C.ObjCxx.ObjectFileCollection
        CreateObjectiveCxxSourceCollection(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var applicationPreprocessor = this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) ? this.ConsolePreprocessor : null;
            var source = this.InternalCreateCollection<C.ObjCxx.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, applicationPreprocessor);
            this.sourceModules.Add(source);
            return source;
        }

        /// <summary>
        /// Extend a collection of C++ object files with another, potentially from another module. Note that module types must match.
        /// Private patches are inherited.
        /// Public patches are used internally to compile against, but are not exposed further.
        /// Note that the referenced module in DependentModule will have its source files marked as PerformCompilation=false
        /// so that it is not attempted to be built standalone.
        /// </summary>
        /// <typeparam name="DependentModule">Collection module type to embed into the specified collection.</typeparam>
        /// <param name="affectedSource">Collection to be extended.</param>
        public void
        ExtendSource<DependentModule>(
            CModuleCollection<ObjectFile> affectedSource)
            where DependentModule : CModuleCollection<ObjectFile>, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            // as the referenced collection of source is to be shoehorned into
            // this module, make sure that the external collection isn't built standalone
            // note that the referenced collection will float to the top of the dependency
            // graph, but just won't do anything
            foreach (var child in dependent.Children)
            {
                if (child is ObjectFileBase childAsObjectFile)
                {
                    childAsObjectFile.PerformCompilation = false;
                }
            }

            affectedSource.ExtendWith(dependent);
            affectedSource.UsePublicPatchesPrivately(dependent);
        }
    }
}

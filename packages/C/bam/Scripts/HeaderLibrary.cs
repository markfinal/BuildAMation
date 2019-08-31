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
namespace C
{
    /// <summary>
    /// Derive from this module to create a library with only header files (i.e. no compilation, archiving or linking).
    /// </summary>
    abstract class HeaderLibrary :
        CModule,
        IForwardedLibraries
    {
        private readonly Bam.Core.Array<Bam.Core.Module> forwardedDeps = new Bam.Core.Array<Bam.Core.Module>();

        /// <summary>
        /// Determine if this module needs updating
        /// </summary>
        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
        }

        /// <summary>
        /// Execute the build for this module
        /// </summary>
        /// <param name="context">in this context</param>
        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    // do nothing
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    // do nothing
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.HeadersOnly(this);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.HeadersOnly(this);
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

        System.Collections.Generic.IEnumerable<Bam.Core.Module> IForwardedLibraries.ForwardedLibraries => this.forwardedDeps;

        /// <summary>
        /// Compile against DependentModule.
        /// As this is a header only library, this dependency implies a public inheritence of DependentModule's public patches.
        /// </summary>
        /// <typeparam name="DependentModule">1st generic type</typeparam>
        public void
        CompileAgainst<DependentModule>() where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.UsePublicPatches(dependent);
            if (dependent is HeaderLibrary)
            {
                this.Requires(dependent);
            }
            // this delays the dependency until a link
            // (and recursively checks the dependent for more forwarded dependencies)
            this.forwardedDeps.AddUnique(dependent);
        }

        /// <summary>
        /// Access the headers forming this library.
        /// </summary>
        public System.Collections.Generic.IEnumerable<Bam.Core.Module> HeaderFiles => FlattenHierarchicalFileList(this.headerModules);
    }
}

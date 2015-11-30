#region License
// Copyright (c) 2010-2015, Mark Final
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
    public abstract class HeaderLibrary :
        CModule,
        IForwardedLibraries
    {
        private Bam.Core.Array<Bam.Core.Module> forwardedDeps = new Bam.Core.Array<Bam.Core.Module>();
        private IHeaderLibraryPolicy Policy;

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }

            var headers = FlattenHierarchicalFileList(this.headerModules).ToReadOnlyCollection();
            this.Policy.HeadersOnly(this, context, headers);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
            case "VSSolution":
            case "Xcode":
                {
                    var className = "C." + mode + "HeaderLibrary";
                    this.Policy = Bam.Core.ExecutionPolicyUtilities<IHeaderLibraryPolicy>.Create(className);
                }
                break;
            }
        }

        System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> IForwardedLibraries.ForwardedLibraries
        {
            get
            {
                return this.forwardedDeps.ToReadOnlyCollection();
            }
        }

        public void
        CompileAgainst<DependentModule>() where DependentModule : CModule, new()
        {
            // no graph dependency, as it's just using patches
            // note that this won't add the module into the graph, unless a link dependency is made
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.UsePublicPatches(dependent);
            if (!(dependent is HeaderLibrary))
            {
                this.forwardedDeps.AddUnique(dependent);
            }
        }
    }
}

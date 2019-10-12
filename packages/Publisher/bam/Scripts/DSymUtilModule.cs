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
namespace Publisher
{
    /// <summary>
    /// Module representing a file having had dsymutil run on it
    /// </summary>
    class DSymUtilModule :
        Bam.Core.Module,
        ICollatedObject
    {
        /// <summary>
        /// Path key to the bundle created by calling dsymutil
        /// </summary>
        public const string DSymBundleKey = "dSYM bundle";

        private Bam.Core.Module sourceModule;
        private string sourcePathKey;
        private ICollatedObject anchor = null;
        private ICollation encapsulatingCollation;

        protected override void
        Init()
        {
            base.Init();

            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<DSymUtilTool>();
            this.RegisterGeneratedFile(
                DSymBundleKey,
                this.CreateTokenizedString(
                    "$(0)/@filename($(1)).dsym",
                    new[] { this.Macros["publishingdir"], this.sourceModule.GeneratedPaths[this.sourcePathKey] }
                )
            );
        }

        protected override void
        EvaluateInternal()
        {
            // TODO
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.Add(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeBuilder.Support.RunCommandLineTool(this, context);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    Bam.Core.Log.DebugMessage("DSym not supported on Xcode builds");
                    break;
#endif

                default:
                    throw new System.NotSupportedException();
            }
        }

        Bam.Core.Module ICollatedObject.SourceModule => this.sourceModule;
        /// <summary>
        /// Set the source module
        /// </summary>
        public Bam.Core.Module SourceModule
        {
            set
            {
                this.sourceModule = value;
            }
        }

        string ICollatedObject.SourcePathKey => this.sourcePathKey;
        /// <summary>
        /// Set the source path key
        /// </summary>
        public string SourcePathKey
        {
            set
            {
                this.sourcePathKey = value;
            }
        }

        Bam.Core.TokenizedString ICollatedObject.PublishingDirectory => this.Macros["publishingdir"];

        ICollatedObject ICollatedObject.Anchor => this.anchor;
        /// <summary>
        /// Set the relative anchor
        /// </summary>
        public ICollatedObject Anchor
        {
            set
            {
                this.anchor = value;
            }
        }

        ICollation ICollatedObject.EncapsulatingCollation => this.encapsulatingCollation;
        /// <summary>
        /// Set the collation that encapsulates this collated object.
        /// </summary>
        public ICollation EncapsulatingCollation
        {
            set
            {
                this.encapsulatingCollation = value;
            }
        }

        /// <summary>
        /// /copydoc Bam.Core.Module.InputModulePaths
        /// </summary>
        public override System.Collections.Generic.IEnumerable<(Bam.Core.Module module, string pathKey)> InputModulePaths
        {
            get
            {
                yield return (this.sourceModule, this.sourcePathKey);
            }
        }
    }
}

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
using Bam.Core;
namespace Publisher
{
    public class StripModule :
        Bam.Core.Module,
        ICollatedObject
    {
#if BAM_V2
        public const string StripBinaryKey = "Stripped Binary Destination";

        private Bam.Core.Module sourceModule;
        private string sourcePathKey;
#else
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Stripped Binary Destination");

        private Bam.Core.Module sourceModule;
        private Bam.Core.PathKey sourcePathKey;
#endif
        private ICollatedObject anchor = null;

#if BAM_V2
#else
        private IStripToolPolicy Policy;
#endif

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<StripTool>();
            this.RegisterGeneratedFile(
#if BAM_V2
                StripBinaryKey,
#else
                Key,
#endif
                this.CreateTokenizedString(
                    "$(0)/@filename($(1))",
                    new[] { this.Macros["publishingdir"], this.sourceModule.GeneratedPaths[this.sourcePathKey] }
                )
            );

            this.Requires(this.sourceModule);
        }

        protected override void
        EvaluateInternal()
        {
            // TODO
            // always strip currently
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
#if BAM_V2
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileSupport.Strip(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeSupport.Strip(this, context);
                    break;
#endif

                default:
                    throw new System.NotSupportedException();
            }
#else
            if (null == this.Policy)
            {
                return;
            }
            this.Policy.Strip(this, context, this.sourceModule.GeneratedPaths[this.sourcePathKey], this.GeneratedPaths[Key]);
#endif
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
#if BAM_V2
#else
            switch (mode)
            {
                case "Native":
                case "MakeFile":
                    {
                        var className = "Publisher." + mode + "Strip";
                        this.Policy = Bam.Core.ExecutionPolicyUtilities<IStripToolPolicy>.Create(className);
                    }
                    break;
            }
#endif
        }

        Bam.Core.Module ICollatedObject.SourceModule
        {
            get
            {
                return this.sourceModule;
            }
        }
        public Bam.Core.Module SourceModule
        {
            set
            {
                this.sourceModule = value;
            }
        }

#if BAM_V2
        string ICollatedObject.SourcePathKey
        {
            get
            {
                return this.sourcePathKey;
            }
        }
        public string SourcePathKey
        {
            set
            {
                this.sourcePathKey = value;
            }
        }
#else
        Bam.Core.PathKey ICollatedObject.SourcePathKey
        {
            get
            {
                return this.sourcePathKey;
            }
        }
        public Bam.Core.PathKey SourcePathKey
        {
            set
            {
                this.sourcePathKey = value;
            }
        }
#endif

        Bam.Core.TokenizedString ICollatedObject.PublishingDirectory
        {
            get
            {
                return this.Macros["publishingdir"];
            }
        }

        ICollatedObject ICollatedObject.Anchor
        {
            get
            {
                return this.anchor;
            }
        }
        public ICollatedObject Anchor
        {
            set
            {
                this.anchor = value;
            }
        }

        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(this.sourcePathKey, this.sourceModule);
            }
        }
    }
}

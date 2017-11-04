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
using Bam.Core;
namespace Publisher
{
#if D_NEW_PUBLISHING
    public abstract class CollatedObject2 :
        Bam.Core.Module,
        ICollatedObject2
    {
        private ICollatedObjectPolicy2 policy = null;
        private Bam.Core.Module sourceModule;
        private Bam.Core.PathKey sourcePathKey;
        private Bam.Core.TokenizedString publishingDirectory;
        private System.Collections.Generic.Dictionary<System.Tuple<Bam.Core.Module, Bam.Core.PathKey>, CollatedObject2> dependents = new System.Collections.Generic.Dictionary<System.Tuple<Module, PathKey>, CollatedObject2>();

        Bam.Core.Module ICollatedObject2.SourceModule
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

        Bam.Core.PathKey ICollatedObject2.SourcePathKey
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

        Bam.Core.TokenizedString ICollatedObject2.PublishingDirectory
        {
            get
            {
                return this.publishingDirectory;
            }
        }
#if true
        public void
        SetPublishingDirectory(
            string original,
            params Bam.Core.TokenizedString[] positional)
        {
            this.publishingDirectory = this.CreateTokenizedString(original, positional);
        }
#else
        public Bam.Core.TokenizedString PublishingDirectory
        {
            set
            {
                this.publishingDirectory = value;
            }
        }
#endif

        public System.Collections.Generic.Dictionary<System.Tuple<Bam.Core.Module, Bam.Core.PathKey>, CollatedObject2> DependentCollations
        {
            get
            {
                return this.dependents;
            }
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<CopyFileWin>();
            }
            else
            {
                this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<CopyFilePosix>();
            }
        }

        public override void
        Evaluate()
        {
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            this.policy.Collate(this, context);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "Publisher." + mode + "CollatedObject2";
            this.policy = Bam.Core.ExecutionPolicyUtilities<ICollatedObjectPolicy2>.Create(className);
        }
    }
#endif

    public abstract class CollatedObject :
        Bam.Core.Module,
        ICollatedObject
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Copied Object");

        private ICollatedObjectPolicy Policy = null;

        protected Collation TheCollator = null;
        private Bam.Core.Module RealSourceModule = null;
        private Bam.Core.TokenizedString SubDirectoryPath = null;
        private CollatedFile ReferenceFile = null;
        protected Bam.Core.TokenizedString RealSourcePath;

        public CollatedObject()
        {
            this.RealSourcePath = this.MakePlaceholderPath();
            this.Macros.Add("CopiedFilename", this.MakePlaceholderPath());
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<CopyFileWin>();
            }
            else
            {
                this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<CopyFilePosix>();
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            this.Policy.Collate(this, context);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "Publisher." + mode + "CollatedObject";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<ICollatedObjectPolicy>.Create(className);
        }

        public Collation Collator
        {
            get
            {
                return this.TheCollator;
            }

            set
            {
                this.TheCollator = value;
            }
        }

        public Bam.Core.Module SourceModule
        {
            get
            {
                return this.RealSourceModule;
            }

            set
            {
                this.RealSourceModule = value;
                if (null != value)
                {
                    if (this.Requirees.Count > 0 && value == this.Requirees[0])
                    {
                        // avoid a circular reference
                        return;
                    }
                    this.Requires(value);
                }
            }
        }

        public virtual Bam.Core.TokenizedString SubDirectory
        {
            get
            {
                return this.SubDirectoryPath;
            }

            set
            {
                this.SubDirectoryPath = value;
            }
        }

        public CollatedFile Reference
        {
            get
            {
                return this.ReferenceFile;
            }

            set
            {
                this.ReferenceFile = value;
                if (null != value)
                {
                    this.Requires(value);
                }
            }
        }

        public virtual TokenizedString SourcePath
        {
            get
            {
                return this.RealSourcePath;
            }

            set
            {
                this.RealSourcePath.Aliased(value);
                this.GeneratedPaths[Key] = this.CreateTokenizedString("$(CopyDir)/@filename($(0))", value);
            }
        }
    }
}

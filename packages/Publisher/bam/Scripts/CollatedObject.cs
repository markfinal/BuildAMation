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
    public abstract class CollatedObject :
        Bam.Core.Module,
        ICollatedObject
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Copied Object");

        private ICollatedObjectPolicy policy = null;

        private Bam.Core.Module sourceModule;
        private Bam.Core.PathKey sourcePathKey;
        private Bam.Core.TokenizedString publishingDirectory;
        private ICollatedObject anchor = null;

        private System.Collections.Generic.Dictionary<System.Tuple<Bam.Core.Module, Bam.Core.PathKey>, CollatedObject> dependents = new System.Collections.Generic.Dictionary<System.Tuple<Module, PathKey>, CollatedObject>();

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

        Bam.Core.TokenizedString ICollatedObject.PublishingDirectory
        {
            get
            {
                return this.publishingDirectory;
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

        // helper function
        public bool IsAnchor
        {
            get
            {
                return null == this.anchor;
            }
        }

        // helper function (XcodeBuilder)
        public bool IsInAnchorPackage
        {
            get
            {
                if (null == this.anchor)
                {
                    return true;
                }
                var srcModule = (this as ICollatedObject).SourceModule;
                if (null == srcModule)
                {
                    return false;
                }
                return (srcModule.PackageDefinition == (this.anchor as ICollatedObject).SourceModule.PackageDefinition);
            }
        }

        public bool IsAnchorAnApplicationBundle
        {
            get
            {
                if (!this.IsAnchor)
                {
                    throw new Bam.Core.Exception("Only available on anchors");
                }

                var isAppBundle = this.publishingDirectory.ToString().Contains(".app");
                return isAppBundle;
            }
        }

        public void
        SetPublishingDirectory(
            string original,
            params Bam.Core.TokenizedString[] positional)
        {
            if (null == this.publishingDirectory)
            {
                this.publishingDirectory = this.CreateTokenizedString(original, positional);
            }
            else
            {
                this.publishingDirectory.Set(original, positional);
            }
        }

        // TODO: add accessors, rather than direct to the field
        public System.Collections.Generic.Dictionary<System.Tuple<Bam.Core.Module, Bam.Core.PathKey>, CollatedObject> DependentCollations
        {
            get
            {
                return this.dependents;
            }
        }

        public string PreExistingSourcePath
        {
            get;
            set;
        }

        public Bam.Core.TokenizedString
        SourcePath
        {
            get
            {
                if (null == this.PreExistingSourcePath)
                {
                    return this.sourceModule.GeneratedPaths[this.sourcePathKey];
                }
                else
                {
                    return Bam.Core.TokenizedString.CreateVerbatim(this.PreExistingSourcePath);
                }
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
            if (null == this.publishingDirectory)
            {
                if (null != this.sourceModule)
                {
                    throw new Bam.Core.Exception("The publishing directory for module '{0}', pathkey '{1}' has yet to be set", this.sourceModule.ToString(), this.sourcePathKey.ToString());
                }
                else
                {
                    // TODO: this may result in a not-yet-parsed TokenizedString exception
                    // but what is the alternative for identifying the path?
                    throw new Bam.Core.Exception("The publishing directory for '{0}' has yet to be set", this.SourcePath);
                }
            }
            this.RegisterGeneratedFile(Key,
                                       this.CreateTokenizedString("$(0)/@filename($(1))",
                                                                  new[] { this.publishingDirectory, this.SourcePath }));
            if (null != this.sourceModule)
            {
                this.Requires(this.sourceModule);
            }
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
            var className = "Publisher." + mode + "CollatedObject";
            this.policy = Bam.Core.ExecutionPolicyUtilities<ICollatedObjectPolicy>.Create(className);
        }
    }
#else
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
#endif
}

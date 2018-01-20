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
                if (null != value)
                {
                    // anchor should exist first
                    this.DependsOn(anchor as Bam.Core.Module);
                }
            }
        }

        public bool Ignore
        {
            get;
            set;
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
            if (null != this.sourceModule)
            {
                this.Requires(this.sourceModule);
                if (!this.sourceModule.GeneratedPaths.ContainsKey(this.sourcePathKey))
                {
                    // this shouldn't happen, but just in case, a sensible error...
                    throw new Bam.Core.Exception("Unable to locate generated path '{0}' in module '{1}' for collation",
                        this.sourcePathKey.ToString(),
                        this.sourceModule.ToString());
                }
            }
            this.RegisterGeneratedFile(Key,
                                       this.CreateTokenizedString("$(0)/#valid($(RenameLeaf),@filename($(1)))",
                                                                  new[] { this.publishingDirectory, this.SourcePath }));
            this.Ignore = false;
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
}

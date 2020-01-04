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
namespace Publisher
{
    /// <summary>
    /// Abstract base class for any collated object
    /// </summary>
    abstract class CollatedObject :
        Bam.Core.Module,
        ICollatedObject
    {
        /// <summary>
        /// Path key to the file copied
        /// </summary>
        public const string CopiedFileKey = "Copied file";

        /// <summary>
        /// Path key to the directory copied
        /// </summary>
        public const string CopiedDirectoryKey = "Copied directory";

        /// <summary>
        /// Path key to the renamed directory copied
        /// </summary>
        public const string CopiedRenamedDirectoryKey = "Copied renamed directory";

        /// <summary>
        /// Path key to the macOS framework copied
        /// </summary>
        public const string CopiedFrameworkKey = "Copied framework";

        private Bam.Core.Module sourceModule;
        private string sourcePathKey;
        private Bam.Core.TokenizedString publishingDirectory;
        private ICollatedObject anchor = null;
        private ICollation encapsulatingCollation;

        private readonly System.Collections.Generic.Dictionary<(Bam.Core.Module module, string pathKey), ICollatedObject> dependents = new System.Collections.Generic.Dictionary<(Bam.Core.Module module, string pathKey), ICollatedObject>();

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

        Bam.Core.TokenizedString ICollatedObject.PublishingDirectory => this.publishingDirectory;

        ICollatedObject ICollatedObject.Anchor => this.anchor;
        /// <summary>
        /// Set the relative anchor point
        /// </summary>
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
        /// Get or set whether to ignore collating this object
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Helper function to determine if this is an anchor
        /// (as an anchor cannot have an anchor)
        /// </summary>
        public bool IsAnchor => null == this.anchor;

        /// <summary>
        /// Helper function for XcodeBuilder, to determine if this collated object
        /// is in the same package as the anchor
        /// </summary>
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

        /// <summary>
        /// Query if the anchor is a macOS application bundle
        /// </summary>
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

        /// <summary>
        /// Set the publishing directory for this collated object
        /// </summary>
        /// <param name="original">Original path</param>
        /// <param name="positional">With any positional arguments</param>
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
                this.publishingDirectory.Set(original, this, positional);
            }
        }

        // TODO: add accessors, rather than direct to the field
        /// <summary>
        /// Get the dependent collations on this collated object
        /// </summary>
        public System.Collections.Generic.Dictionary<(Bam.Core.Module module, string pathKey), ICollatedObject> DependentCollations => this.dependents;

        /// <summary>
        /// Get the source path of this collated object
        /// </summary>
        public Bam.Core.TokenizedString SourcePath => this.sourceModule.GeneratedPaths[this.sourcePathKey];

        /// <summary>
        /// Initialize this module
        /// </summary>
        protected override void
        Init()
        {
            base.Init();
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
                    throw new Bam.Core.Exception(
                        $"The publishing directory for module '{this.sourceModule.ToString()}', pathkey '{this.sourcePathKey.ToString()}' has yet to be set"
                    );
                }
                else
                {
                    // TODO: this may result in a not-yet-parsed TokenizedString exception
                    // but what is the alternative for identifying the path?
                    throw new Bam.Core.Exception(
                        $"The publishing directory for '{this.SourcePath}' has yet to be set"
                    );
                }
            }
            if (null != this.sourceModule)
            {
                this.Requires(this.sourceModule);
                if (!this.sourceModule.GeneratedPaths.ContainsKey(this.sourcePathKey))
                {
                    // this shouldn't happen, but just in case, a sensible error...
                    throw new Bam.Core.Exception(
                        $"Unable to locate generated path '{this.sourcePathKey.ToString()}' in module '{this.sourceModule.ToString()}' for collation"
                    );
                }
            }
            if (this is CollatedFile)
            {
                this.RegisterGeneratedFile(
                    CopiedFileKey,
                    this.CreateTokenizedString(
                        "$(0)/@filename($(1))",
                        new[]
                        {
                            this.publishingDirectory,
                            this.SourcePath
                        }
                    ),
                    true
                );
            }
            else if (this is CollatedDirectory)
            {
                if (this.Macros.ContainsName("RenameLeaf"))
                {
                    this.RegisterGeneratedFile(
                        CopiedRenamedDirectoryKey,
                        this.CreateTokenizedString(
                            "$(0)/$(RenameLeaf)",
                            new[]
                            {
                                this.publishingDirectory
                            }
                        ),
                        true
                    );
                }
                else
                {
                    this.RegisterGeneratedFile(
                        CopiedDirectoryKey,
                        this.CreateTokenizedString(
                            "$(0)/@filename($(1))",
                            new[]
                            {
                                this.publishingDirectory,
                                this.SourcePath
                            }
                        ),
                        true
                    );
                }
            }
            else if (this is CollatedOSXFramework)
            {
                this.RegisterGeneratedFile(
                    CopiedFrameworkKey,
                    this.CreateTokenizedString(
                        "$(0)/@filename($(1))",
                        new[]
                        {
                            this.publishingDirectory,
                            this.SourcePath
                        }
                    ),
                    true
                );
            }
            else
            {
                throw new System.NotSupportedException();
            }
            this.Ignore = false;
        }

        /// <summary>
        /// Execute the tool on this module
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
                    MakeFileBuilder.Support.Add(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    {
                        if (this.Ignore)
                        {
                            return;
                        }
                        NativeBuilder.Support.RunCommandLineTool(this, context);
                    }
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.CollateObject(this);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.CollateObject(this);
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
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

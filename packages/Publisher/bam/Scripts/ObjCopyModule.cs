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
    /// Abstract module representing a file that has had sections copied from another
    /// </summary>
    abstract class ObjCopyModule :
        Bam.Core.Module,
        ICollatedObject
    {
        /// <summary>
        /// The Module that is the source of the objcopy
        /// </summary>
        protected Bam.Core.Module sourceModule;

        /// <summary>
        /// The path key on that source Module
        /// </summary>
        protected string sourcePathKey;

        /// <summary>
        /// The anchor to which this is relative to
        /// </summary>
        protected ICollatedObject anchor = null;

        protected ICollation encapsulatingCollation;

        /// <summary>
        /// Initialize this module
        /// </summary>
        protected override void
        Init()
        {
            base.Init();
            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<ObjCopyTool>();
            this.Requires(sourceModule);
        }

        protected override void
        EvaluateInternal()
        {
            // TODO
            // always generate currently
        }

        /// <summary>
        /// Execute the tool on this Module
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
                    NativeBuilder.Support.RunCommandLineTool(this, context);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    Bam.Core.Log.DebugMessage("ObjCopy not supported on Xcode builds");
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
        /// Set the anchor
        /// </summary>
        public ICollatedObject Anchor
        {
            set
            {
                this.anchor = value;
            }
        }

        ICollation ICollatedObject.EncapsulatingCollation => this.encapsulatingCollation;
        public ICollation EncapsulatingCollation
        {
            set
            {
                this.encapsulatingCollation = value;
            }
        }
    }

    /// <summary>
    /// Module that makes a debug symbol file
    /// </summary>
    sealed class MakeDebugSymbolFile :
        ObjCopyModule
    {
        /// <summary>
        /// Path key for the debug symbol file
        /// </summary>
        public const string DebugSymbolFileKey = "GNU Debug Symbol File";

        /// <summary>
        /// Initialize this module
        /// </summary>
        protected override void
        Init()
        {
            base.Init();

            var trueSourceModule = this.sourceModule;
            // stripping works on the initial collated file
            while (trueSourceModule is ICollatedObject)
            {
                // necessary on Linux, as the real source module needs checking against
                // C.IDynamicLibrary to identify paths as lib<name>.so.X.Y
                trueSourceModule = (trueSourceModule as ICollatedObject).SourceModule;
            }
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux) &&
                trueSourceModule is C.IDynamicLibrary)
            {
                // on Linux with a dynamic library, need to include the full versioning
                this.RegisterGeneratedFile(
                    DebugSymbolFileKey,
                    this.CreateTokenizedString(
                        "$(0)/@filename($(1)).debug",
                        new[] { this.Macros["publishingdir"], this.sourceModule.GeneratedPaths[this.sourcePathKey] }
                    )
                );
            }
            else
            {
                this.RegisterGeneratedFile(
                    DebugSymbolFileKey,
                    this.CreateTokenizedString(
                        "$(0)/@basename($(1)).debug",
                        new[] { this.Macros["publishingdir"], this.sourceModule.GeneratedPaths[this.sourcePathKey] }
                    )
                );
            }
        }

        public override Settings
        MakeSettings()
        {
            return new MakeDebugSymbolFileSettings();
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

        /// <summary>
        /// Make the Module to cause the linkback
        /// </summary>
        /// <param name="strippedCollatedObject">The original stripped binary</param>
        /// <returns>Module to perform the linkback</returns>
        public LinkBackDebugSymbolFile
        LinkBackToDebugSymbols(
            StripModule strippedCollatedObject)
        {
            var linkDebugSymbols = Bam.Core.Module.Create<LinkBackDebugSymbolFile>(preInitCallback: module =>
                {
                    module.DebugSymbolModule = this;
                    module.SourceModule = strippedCollatedObject;
                    module.SourcePathKey = StripModule.StripBinaryKey;
                    module.Macros.Add("publishingdir", strippedCollatedObject.Macros["publishingdir"].Clone(module));
                    module.EncapsulatingCollation = (strippedCollatedObject as ICollatedObject).EncapsulatingCollation;
                });
            linkDebugSymbols.DependsOn(strippedCollatedObject);

            linkDebugSymbols.Macros.Add("publishdir", this.Macros["publishdir"]);

            linkDebugSymbols.PrivatePatch(settings =>
                {
                    var objCopySettings = settings as IObjCopyToolSettings;
                    objCopySettings.OnlyKeepDebug = false;
                });

            return linkDebugSymbols;
        }
    }

    /// <summary>
    /// Module that links debug symbol files back to the original executable
    /// </summary>
    sealed class LinkBackDebugSymbolFile :
        ObjCopyModule
    {
        /// <summary>
        /// Path key to the original executable that has a linkback to the debug symbols
        /// </summary>
        public const string UpdateOriginalExecutable = "Updating original executable with debug linkback";

        /// <summary>
        /// Initialize this module
        /// </summary>
        protected override void
        Init()
        {
            base.Init();

            this.RegisterGeneratedFile(
                UpdateOriginalExecutable,
                this.sourceModule.GeneratedPaths[this.sourcePathKey]
            );
        }

        /// <summary>
        /// Get or set the Module that holds the debug symbols
        /// </summary>
        public MakeDebugSymbolFile DebugSymbolModule { get; set; }

        public override Settings
        MakeSettings()
        {
            return new LinkBackDebugSymbolFileSettings();
        }

        /// <summary>
        /// /copydoc Bam.Core.Module.InputModulePaths
        /// </summary>
        public override System.Collections.Generic.IEnumerable<(Bam.Core.Module module, string pathKey)> InputModulePaths
        {
            get
            {
                yield return (this.DebugSymbolModule, MakeDebugSymbolFile.DebugSymbolFileKey);
            }
        }

        /// <summary>
        /// Run the tool on this module
        /// </summary>
        /// <param name="context">in this context</param>
        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
#if D_PACKAGE_MAKEFILEBUILDER
            if ("MakeFile".Equals(Bam.Core.Graph.Instance.Mode, System.StringComparison.Ordinal))
            {
                // append to the strip rule
                System.Diagnostics.Debug.Assert((this as ICollatedObject).SourceModule is StripModule);
                MakeFileBuilder.Support.Add(
                    this,
                    moduleToAppendTo: (this as ICollatedObject).SourceModule
                );
                return;
            }
#endif
            base.ExecuteInternal(context);
        }
    }
}

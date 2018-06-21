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
    public class ObjCopyModule :
        Bam.Core.Module,
        ICollatedObject
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("ObjCopy Destination");

        private Bam.Core.Module sourceModule;
        private Bam.Core.PathKey sourcePathKey;
        private ICollatedObject anchor = null;

        private IObjCopyToolPolicy Policy;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<ObjCopyTool>();

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
                this.RegisterGeneratedFile(Key,
                    this.CreateTokenizedString("$(0)/@filename($(1)).debug",
                                               new[] { this.Macros["publishingdir"], this.sourceModule.GeneratedPaths[this.sourcePathKey] }));
            }
            else
            {
                this.RegisterGeneratedFile(Key,
                    this.CreateTokenizedString("$(0)/@basename($(1)).debug",
                                               new[] { this.Macros["publishingdir"], this.sourceModule.GeneratedPaths[this.sourcePathKey] }));
            }

            this.Requires(this.sourceModule);
        }

        protected override void
        EvaluateInternal()
        {
            // TODO
            // always generate currently
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }
            this.Policy.ObjCopy(this, context, this.sourceModule.GeneratedPaths[this.sourcePathKey], this.GeneratedPaths[Key]);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
                case "Native":
                case "MakeFile":
                    {
                        var className = "Publisher." + mode + "ObjCopy";
                        this.Policy = Bam.Core.ExecutionPolicyUtilities<IObjCopyToolPolicy>.Create(className);
                    }
                    break;
            }
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

        public ObjCopyModule
        LinkBackToDebugSymbols(
            StripModule strippedCollatedObject)
        {
            var linkDebugSymbols = Bam.Core.Module.Create<ObjCopyModule>(preInitCallback: module =>
                {
                    module.SourceModule = strippedCollatedObject;
                    module.SourcePathKey = StripModule.Key;
                    module.Macros.Add("publishingdir", strippedCollatedObject.Macros["publishingdir"].Clone(module));
                });
            linkDebugSymbols.DependsOn(strippedCollatedObject);

            linkDebugSymbols.Macros.Add("publishdir", this.Macros["publishdir"]);

            linkDebugSymbols.PrivatePatch(settings =>
                {
                    var objCopySettings = settings as IObjCopyToolSettings;
                    objCopySettings.Mode = EObjCopyToolMode.AddGNUDebugLink;
                });

            return linkDebugSymbols;
        }
    }
}

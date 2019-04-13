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
namespace Installer
{
    public class ZipModule :
        Bam.Core.Module
    {
        public const string ZipKey = "Zip Files or Directories";
        private Bam.Core.TokenizedString InputPath;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<ZipWin>();
            }
            else
            {
                this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<ZipPosix>();
            }
            this.RegisterGeneratedFile(
                ZipKey,
                this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/$(zipoutputbasename).zip")
            );

            this.InputPath = this.Macros["pathtozip"];
        }

        protected override void
        EvaluateInternal()
        {
            // always update, so that zip can figure out what needs updating
            /*
            this.ReasonToExecute = null;
            var zipFilePath = this.GeneratedPaths[Key].ToString();
            if (!System.IO.File.Exists(zipFilePath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            */

            // TODO: how to check if zip file is newer than directory?
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

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionBuilder.Support.AddPreBuildSteps(
                        this
                    );
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    {
                        XcodeBuilder.Support.AddPreBuildStepForCommandLineTool(
                            this,
                            out XcodeBuilder.Target target,
                            out XcodeBuilder.Configuration configuration,
                            false,
                            true // because zip returns 12 (nothing to do) upon success for incrementals
                        );

                        target.EnsureOutputFileReferenceExists(
                            this.GeneratedPaths[ZipModule.ZipKey],
                            XcodeBuilder.FileReference.EFileType.ZipArchive,
                            XcodeBuilder.Target.EProductType.Utility);

                        configuration.SetProductName(Bam.Core.TokenizedString.CreateVerbatim("DirectoryZip"));
                    }
                    break;
#endif

                default:
                    throw new System.NotSupportedException();
            }
        }

        public override Bam.Core.TokenizedString WorkingDirectory => this.InputPath;
    }
}

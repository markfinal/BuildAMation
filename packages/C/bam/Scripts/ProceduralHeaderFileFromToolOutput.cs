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
namespace C
{
    /// <summary>
    /// </summary>
    public abstract class ProceduralHeaderFileFromToolOutput :
        C.HeaderFile
    {
        /// <summary>
        /// Override this function to specify the path of the header to be written to.
        /// </summary>
        protected abstract Bam.Core.TokenizedString
        OutputPath
        {
            get;
        }

        protected abstract Bam.Core.ICommandLineTool
        SourceTool
        {
            get;
        }

        protected virtual Bam.Core.TokenizedString
        IncludeDirectory
        {
            get
            {
                return this.CreateTokenizedString("@dir($(0))", this.InputPath);
            }
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.InputPath = this.OutputPath;

            this.Tool = this.SourceTool as Bam.Core.Module;
            this.DependsOn(this.Tool);

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.IncludeDirectory);
                    }

                    var assembler = settings as C.ICommonAssemblerSettings;
                    if (null != assembler)
                    {
                        assembler.IncludePaths.AddUnique(this.IncludeDirectory);
                    }

                    var rcCompiler = settings as C.ICommonWinResourceCompilerSettings;
                    if (null != rcCompiler)
                    {
                        rcCompiler.IncludePaths.AddUnique(this.IncludeDirectory);
                    }
                });
        }

        protected override void
        EvaluateInternal()
        {
            // TODO
            // always build
            this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[HeaderFileKey]);
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.Add(
                        this,
                        redirectOutputToFile: this.GeneratedPaths[HeaderFileKey]
                    );
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    {
                        NativeBuilder.Support.RunCommandLineTool(this, context);
                        NativeBuilder.Support.SendCapturedOutputToFile(
                            this,
                            context,
                            ProceduralHeaderFileFromToolOutput.HeaderFileKey
                        );
                    }
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.GenerateHeader(this);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.GenerateHeader(this);
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(C.ConsoleApplication.ExecutableKey, this.Tool as Bam.Core.Module);
            }
        }
    }
}

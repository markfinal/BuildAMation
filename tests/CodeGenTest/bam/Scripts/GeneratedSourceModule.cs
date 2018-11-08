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
namespace CodeGenTest
{
    public class GeneratedSourceModule :
        C.SourceFile
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<BuildCodeGenTool>();
            this.Requires(this.Tool as Bam.Core.Module);
            this.Macros.AddVerbatim("GenerateBasename", "Generated");
            this.InputPath = this.CreateTokenizedString("$(buildroot)/$(GenerateBasename).c");
        }

        protected override void
        EvaluateInternal() => this.ReasonToExecute = Bam.Core.ExecuteReasoning.Undefined();

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
                        this,
                        addOrderOnlyDependencyOnTool: true // dependent upon the 'generator' being up to date before running
                    );
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    {
                        var output_paths = new Bam.Core.TokenizedStringArray();
                        output_paths.AddUnique(this.GeneratedPaths[SourceFileKey]);
                        XcodeBuilder.Support.AddPreBuildStepForCommandLineTool(
                            this,
                            out XcodeBuilder.Target target,
                            out XcodeBuilder.Configuration configuration,
                            true,
                            false,
                            addOrderOnlyDependencyOnTool: true,
                            outputPaths: output_paths
                        );
                    }
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}

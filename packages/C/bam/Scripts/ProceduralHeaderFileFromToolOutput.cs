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
namespace C
{
    /// <summary>
    /// Produce a header file from the standard out of a tool
    /// </summary>
    public abstract class ProceduralHeaderFileFromToolOutput :
        C.HeaderFile
    {
        /// <summary>
        /// Override this function to specify the path of the header to be written to.
        /// </summary>
        protected abstract Bam.Core.TokenizedString OutputPath { get; }

        /// <summary>
        /// ICommandLineTool used to generate output
        /// </summary>
        protected abstract Bam.Core.ICommandLineTool SourceTool { get; }

        /// <summary>
        /// Set the include directory in use
        /// </summary>
        protected virtual Bam.Core.TokenizedString IncludeDirectory => this.CreateTokenizedString("@dir($(0))", this.InputPath);

        /// <summary>
        /// Set to true to add search paths to the system includes, rather than user includes.
        /// </summary>
        protected virtual bool UseSystemIncludeSearchPaths => false;

        /// <summary>
        /// Initialize this module
        /// </summary>
        protected override void
        Init()
        {
            base.Init();
            this.InputPath = this.OutputPath;

            this.Tool = this.SourceTool as Bam.Core.Module;
            this.DependsOn(this.Tool);

            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        if (this.UseSystemIncludeSearchPaths)
                        {
                            preprocessor.SystemIncludePaths.AddUnique(this.IncludeDirectory);
                        }
                        else
                        {
                            preprocessor.IncludePaths.AddUnique(this.IncludeDirectory);
                        }
                    }

                    if (settings is C.ICommonAssemblerSettings assembler)
                    {
                        assembler.IncludePaths.AddUnique(this.IncludeDirectory);
                    }

                    if (settings is C.ICommonWinResourceCompilerSettings rcCompiler)
                    {
                        rcCompiler.IncludePaths.AddUnique(this.IncludeDirectory);
                    }
                });
        }

        /// <summary>
        /// Determine whether this module needs updating
        /// </summary>
        protected override void
        EvaluateInternal()
        {
            // TODO
            // always build
            this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[HeaderFileKey]);
        }

        /// <summary>
        /// Execute the build of this module
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
                    VSSolutionSupport.GenerateFileFromToolStandardOutput(
                        this,
                        ProceduralHeaderFileFromToolOutput.HeaderFileKey,
                        includeEnvironmentVariables: true
                    );
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.GenerateFileFromToolStandardOutput(
                        this,
                        ProceduralHeaderFileFromToolOutput.HeaderFileKey
                    );
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

#if false
        /// <summary>
        /// Enumerate across all input modules
        /// </summary>
        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(C.ConsoleApplication.ExecutableKey, this.Tool as Bam.Core.Module);
            }
        }
#endif
    }
}

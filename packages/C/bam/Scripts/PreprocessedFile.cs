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
    class PreprocessedFile :
        CModule,
        Bam.Core.IInputPath,
        IRequiresSourceModule
    {
        public const string PreprocessedFileKey = "Preprocessed file";

        protected SourceFile SourceModule;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Tool = C.DefaultToolchain.Preprocessor(this.BitDepth);
        }

        protected override void
        EvaluateInternal()
        {
            // TODO: should be similar to the ObjectFile one, include header dependencies
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
                        redirectOutputToFile: this.GeneratedPaths[PreprocessedFileKey]
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
                            PreprocessedFileKey
                        );
                    }
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.GenerateFileFromToolStandardOutput(
                        this,
                        PreprocessedFileKey,
                        includeEnvironmentVariables: false // since it's running the preprocessor in the IDE, no environment variables necessary
                    );
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.GenerateFileFromToolStandardOutput(
                        this,
                        PreprocessedFileKey
                    );
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

        SourceFile IRequiresSourceModule.Source
        {
            get
            {
                return this.SourceModule;
            }

            set
            {
                if (null != this.SourceModule)
                {
                    this.SourceModule.InputPath.Parse();
                    throw new Bam.Core.Exception(
                        $"Source module already set on this preprocessed file, to '{this.SourceModule.InputPath.ToString()}'"
                    );
                }
                this.SourceModule = value;
                this.DependsOn(value);
                this.RegisterGeneratedFile(
                    PreprocessedFileKey,
                    this.CreateTokenizedString(
                        "$(packagebuilddir)/$(moduleoutputdir)/@changeextension(@isrelative(@trimstart(@relativeto($(0),$(packagedir)),../),@filename($(0))),.c)",
                        value.GeneratedPaths[SourceFile.SourceFileKey]
                    )
                );
            }
        }

        public Bam.Core.TokenizedString InputPath
        {
            get
            {
                if (null == this.SourceModule)
                {
                    throw new Bam.Core.Exception("Source module not yet set on this preprocessed file");
                }
                return this.SourceModule.InputPath;
            }
            set
            {
                if (null != this.SourceModule)
                {
                    this.SourceModule.InputPath.Parse();
                    throw new Bam.Core.Exception(
                        $"Source module already set on this preprocessed file, to '{this.SourceModule.InputPath.ToString()}'"
                    );
                }

                // this cannot be a referenced module, since there will be more than one object
                // of this type (generally)
                // but this does mean there may be many instances of this 'type' of module
                // and for multi-configuration builds there may be many instances of the same path
                var source = Bam.Core.Module.Create<SourceFile>();
                source.InputPath = value;
                (this as IRequiresSourceModule).Source = source;
            }
        }

        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(SourceFile.SourceFileKey, this.SourceModule);
            }
        }
    }
}

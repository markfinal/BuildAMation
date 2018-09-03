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
    /// Base class for all procedurally generated header files.
    /// Override the OutputPath and Contents properties to provide the relevant details of the file.
    /// Optionally override the GuardString property to customize the header include guard text. Otherwise this is the basename of the generated file in upper-case with a _H suffix.
    /// A hash of the contents is persisted on disk, so that changes to the contents will cause the file to be rebuilt.
    /// </summary>
    [Bam.Core.EvaluationRequired(true)]
    public abstract class ProceduralHeaderFile :
        C.HeaderFile
    {
#if BAM_V2
        public const string HashFileKey = "Hash of generated header contents";
#else
        private static Bam.Core.PathKey HashFileKey = Bam.Core.PathKey.Generate("Hash of generated header contents");
#endif

        /// <summary>
        /// Override this function to specify the path of the header to be written to.
        /// </summary>
        protected abstract Bam.Core.TokenizedString
        OutputPath
        {
            get;
        }

        /// <summary>
        /// Override this function to specify the contents of the header file to be written.
        /// </summary>
        protected abstract string
        Contents
        {
            get;
        }

        /// <summary>
        /// Optionally override this function to customize the guard text used in the generated header.
        /// </summary>
        protected virtual string
        GuardString
        {
            get
            {
                var guard = this.CreateTokenizedString("@basename($(0))", this.OutputPath);
                guard.Parse();
                var guardString = guard.ToString().Replace('-', '_'); // replace any characters not suitable for a preprocessor definition with underscores
                guardString = guardString.ToUpper() + "_H";
                return guardString;
            }
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
            this.RegisterGeneratedFile(
                HashFileKey,
                this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/@filename($(0)).hash", this.OutputPath)
            );

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
            this.ReasonToExecute = null;
#if BAM_V2
            var outputPath = this.GeneratedPaths[HeaderFileKey].ToString();
#else
            var outputPath = this.GeneratedPaths[Key].ToString();
#endif
            if (!System.IO.File.Exists(outputPath))
            {
#if BAM_V2
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[HeaderFileKey]);
#else
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
#endif
            }
            // have the contents changed since last time?
            var hashFilePath = this.GeneratedPaths[HashFileKey].ToString();
            var hashCompare = Bam.Core.Hash.CompareAndUpdateHashFile(
                hashFilePath,
                this.Contents
            );
            switch (hashCompare)
            {
                case Bam.Core.Hash.EHashCompareResult.HashesAreDifferent:
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                        this.GeneratedPaths[HeaderFileKey],
                        this.GeneratedPaths[HashFileKey]
                    );
                    break;

                case Bam.Core.Hash.EHashCompareResult.HashFileDoesNotExist:
                case Bam.Core.Hash.EHashCompareResult.HashesAreIdentical:
                    break;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
#if BAM_V2
            var destPath = this.GeneratedPaths[HeaderFileKey].ToString();
#else
            var destPath = this.GeneratedPaths[Key].ToString();
#endif
            var destDir = System.IO.Path.GetDirectoryName(destPath);
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(destDir);
            using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
            {
                writeFile.NewLine = "\n";
                var guard = this.GuardString;
                writeFile.WriteLine("/* Procedurally generated by BuildAMation from module {0} */", this.ToString());
                if (null != guard)
                {
                    writeFile.WriteLine("#ifndef {0}", guard);
                    writeFile.WriteLine("#define {0}", guard);
                    writeFile.WriteLine(this.Contents);
                    writeFile.WriteLine("#endif /* {0} */", guard);
                }
                else
                {
                    writeFile.WriteLine(this.Contents);
                }
            }
            Bam.Core.Log.Info("Written procedurally generated header : {0}", destPath);
        }
    }
}

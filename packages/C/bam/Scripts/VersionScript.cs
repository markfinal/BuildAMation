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
    /// Base class for all ld version scripts.
    /// Override the OutputPath and Contents properties to provide the relevant details of the file.
    /// A hash of the contents is persisted on disk, so that changes to the contents will cause the file to be rebuilt.
    /// </summary>
    [Bam.Core.EvaluationRequired(true)]
    public abstract class VersionScript :
        C.HeaderFile
    {
        /// <summary>
        /// Path key for this module
        /// </summary>
        public const string HashFileKey = "Hash of version script contents";

        /// <summary>
        /// Override this function to specify the path of the version script to be written to.
        /// </summary>
        public abstract Bam.Core.TokenizedString OutputPath { get; }

        /// <summary>
        /// Override this function to specify the contents of the version script to be written.
        /// </summary>
        protected abstract string Contents { get; }

        /// <summary>
        /// Initialize this module
        /// </summary>
        /// <param name="parent">from this parent</param>
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
        }

        /// <summary>
        /// Determine whether this module needs updating
        /// </summary>
        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[HeaderFileKey].ToString();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[HeaderFileKey]);
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

        /// <summary>
        /// Execute the build on this module
        /// </summary>
        /// <param name="context">in this context</param>
        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            var destPath = this.GeneratedPaths[HeaderFileKey].ToString();
            var destDir = System.IO.Path.GetDirectoryName(destPath);
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(destDir);
            using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
            {
                writeFile.NewLine = "\n";
                writeFile.WriteLine(this.Contents);
            }
            Bam.Core.Log.Info($"Written procedurally generated version script : {destPath}");
        }
    }
}

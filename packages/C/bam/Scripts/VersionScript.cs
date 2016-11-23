#region License
// Copyright (c) 2010-2016, Mark Final
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
        private static Bam.Core.PathKey HashFileKey = Bam.Core.PathKey.Generate("Hash of version script contents");

        /// <summary>
        /// Override this function to specify the path of the version script to be written to.
        /// </summary>
        public abstract Bam.Core.TokenizedString
        OutputPath
        {
            get;
        }

        /// <summary>
        /// Override this function to specify the contents of the version script to be written.
        /// </summary>
        protected abstract string
        Contents
        {
            get;
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.InputPath = this.OutputPath;
            this.GeneratedPaths.Add(HashFileKey, this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/@filename($(0)).hash", this.OutputPath));
        }

        private delegate int GetHashFn(string inPath);

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
            }
            // have the contents changed since last time?
            var writeHashFile = true;
            var currentContentsHash = this.Contents.GetHashCode();
            var hashFilePath = this.GeneratedPaths[HashFileKey].Parse();
            if (System.IO.File.Exists(hashFilePath))
            {
                GetHashFn getHash = inPath =>
                {
                    int hash = 0;
                    using (System.IO.TextReader readFile = new System.IO.StreamReader(inPath))
                    {
                        var contents = readFile.ReadToEnd();
                        hash = System.Convert.ToInt32(contents);
                    }
                    return hash;
                };
                var oldHash = getHash(hashFilePath);
                if (oldHash == currentContentsHash)
                {
                    writeHashFile = false;
                }
                else
                {
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.GeneratedPaths[HashFileKey]);
                }
            }
            if (writeHashFile)
            {
                var destDir = System.IO.Path.GetDirectoryName(hashFilePath);
                Bam.Core.IOWrapper.CreateDirectoryIfNotExists(destDir);
                using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(hashFilePath))
                {
                    writeFile.NewLine = "\n";
                    writeFile.Write(currentContentsHash);
                }
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            var destPath = this.GeneratedPaths[Key].Parse();
            var destDir = System.IO.Path.GetDirectoryName(destPath);
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(destDir);
            using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
            {
                writeFile.NewLine = "\n";
                writeFile.WriteLine(this.Contents);
            }
            Bam.Core.Log.Info("Written procedurally generated version script : {0}", destPath);
        }
    }
}

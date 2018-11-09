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
using System.Linq;
namespace Publisher
{
    public abstract class CopyFileTool :
        Bam.Core.PreBuiltTool
    {
        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
        }

        public abstract void
        ConvertPaths(
            CollatedObject module,
            Bam.Core.TokenizedString inSourcePath,
            Bam.Core.TokenizedString inPublishingPath,
            out string resolvedSourcePath,
            out string resolvedDestinationDir);

        public abstract string
        EscapePath(
            string path);
    }

    public sealed class CopyFilePosix :
        CopyFileTool
    {
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new PosixCopyFileSettings(module);

        public override Bam.Core.TokenizedString Executable => Bam.Core.TokenizedString.CreateVerbatim(Bam.Core.OSUtilities.GetInstallLocation("bash").First());

        public override Bam.Core.TokenizedStringArray InitialArguments
        {
            get
            {
                var initArgs = new Bam.Core.TokenizedStringArray();
                initArgs.Add(Bam.Core.TokenizedString.CreateVerbatim("-c"));
                initArgs.Add(Bam.Core.TokenizedString.CreateVerbatim("\""));
                initArgs.Add(Bam.Core.TokenizedString.CreateVerbatim("cp"));
                return initArgs;
            }
        }

        public override Bam.Core.TokenizedStringArray TerminatingArguments
        {
            get
            {
                var termArgs = new Bam.Core.TokenizedStringArray();
                termArgs.Add(Bam.Core.TokenizedString.CreateVerbatim("\""));
                return termArgs;
            }
        }

        public override void
        ConvertPaths(
            CollatedObject module,
            Bam.Core.TokenizedString inSourcePath,
            Bam.Core.TokenizedString inPublishingPath,
            out string resolvedSourcePath,
            out string resolvedDestinationDir)
        {
            if (module is CollatedDirectory)
            {
                // Posix cp only requires the destination to be added when there is a rename
                if (module.Macros.Contains("RenameLeaf"))
                {
                    var dirSep = System.IO.Path.DirectorySeparatorChar;
                    resolvedSourcePath = $"{inSourcePath.ToString()}{dirSep}*";
                    resolvedDestinationDir = $"{inPublishingPath.ToString()}{dirSep}{module.Macros["RenameLeaf"].ToString()}{dirSep}";
                }
                else
                {
                    resolvedSourcePath = inSourcePath.ToString();
                    resolvedDestinationDir = inPublishingPath.ToString();
                }
            }
            else
            {
                resolvedSourcePath = inSourcePath.ToString();
                resolvedDestinationDir = inPublishingPath.ToString();
            }
        }

        public override string
        EscapePath(
            string path) => Bam.Core.IOWrapper.EscapeSpacesInPath(path);
    }

    public sealed class CopyFileWin :
        CopyFileTool
    {
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new WinCopyFileSettings(module);

        public override Bam.Core.TokenizedString Executable => Bam.Core.TokenizedString.CreateVerbatim(Bam.Core.OSUtilities.GetInstallLocation("xcopy.exe").First());

        public override void
        ConvertPaths(
            CollatedObject module,
            Bam.Core.TokenizedString inSourcePath,
            Bam.Core.TokenizedString inPublishingPath,
            out string resolvedSourcePath,
            out string resolvedDestinationDir)
        {
            resolvedSourcePath = inSourcePath.ToString();

            if (module is CollatedDirectory)
            {
                var dirSep = System.IO.Path.DirectorySeparatorChar;
                // Windows XCOPY requires the directory name to be added to the destination regardless
                if (module.Macros.Contains("RenameLeaf"))
                {
                    resolvedDestinationDir = $"{inPublishingPath.ToString()}{dirSep}{module.Macros["RenameLeaf"].ToString()}{dirSep}";
                }
                else
                {
                    resolvedDestinationDir = $"{inPublishingPath.ToString()}{dirSep}{System.IO.Path.GetFileName(inSourcePath.ToString())}{dirSep}";
                }
            }
            else
            {
                resolvedDestinationDir = inPublishingPath.ToString();
            }
        }

        public override string
        EscapePath(
            string path) => Bam.Core.IOWrapper.EncloseSpaceContainingPathWithDoubleQuotes(path);
    }
}

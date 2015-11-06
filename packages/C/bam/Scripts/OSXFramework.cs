#region License
// Copyright (c) 2010-2015, Mark Final
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
    public abstract class OSXFramework :
        CModule
    {
        public class Path
        {
            public Path(
                Bam.Core.TokenizedString source,
                Bam.Core.TokenizedString destination =  null)
            {
                this.SourcePath = source;
                this.DestinationPath = destination;
            }

            public Bam.Core.TokenizedString SourcePath
            {
                get;
                private set;
            }

            public Bam.Core.TokenizedString DestinationPath
            {
                get;
                private set;
            }
        }

        protected OSXFramework()
        {
            this.Macros["FrameworkLibraryPath"] = this.MakePlaceholderPath();
        }

        private void
        GetIDName()
        {
            var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");

            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = "xcrun";
            processStartInfo.Arguments = System.String.Format("--sdk {0} otool -DX {1}",
                clangMeta.SDK,
                this.CreateTokenizedString("$(0)/$(FrameworkLibraryPath)", this.FrameworkPath).Parse());
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Bam.Core.Exception("Unable to get id name of '{0}'", this.Macros["FrameworkLibraryPath"].Parse());
            }
            var idName = process.StandardOutput.ReadToEnd().TrimEnd (new [] { System.Environment.NewLine[0] });
            this.Macros["IDName"] = Bam.Core.TokenizedString.CreateVerbatim(idName);
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.GetIDName();
        }

        public abstract Bam.Core.TokenizedString FrameworkPath
        {
            get;
        }

        public abstract Bam.Core.Array<Path> DirectoriesToPublish
        {
            get;
        }

        public abstract Bam.Core.Array<Path> FilesToPublish
        {
            get;
        }

        public abstract Bam.Core.Array<Path> SymlinksToPublish
        {
            get;
        }
    }
}

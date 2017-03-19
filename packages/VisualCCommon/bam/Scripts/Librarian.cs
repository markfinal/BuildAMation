#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace VisualCCommon
{
    public abstract class LibrarianBase :
        C.LibrarianTool
    {
        protected LibrarianBase()
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.Macros.Add("InstallPath", meta.InstallDir);
            this.Macros.AddVerbatim("libprefix", string.Empty);
            this.Macros.AddVerbatim("libext", ".lib");

            if (null != meta.RequiredExecutablePaths)
            {
                this.EnvironmentVariables.Add("PATH", meta.RequiredExecutablePaths);
            }
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new VisualC.ArchiverSettings(module);
            return settings;
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["ArchiverPath"];
            }
        }

        public override string UseResponseFileOption
        {
            get
            {
                return "@";
            }
        }
    }

    [C.RegisterLibrarian("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public sealed class Librarian32 :
        LibrarianBase
    {
        public Librarian32()
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.Macros.Add("ArchiverPath", Bam.Core.TokenizedString.Create(@"$(0)\lib.exe", null, new Bam.Core.TokenizedStringArray(meta.Bin32Dir)));
        }
    }

    [C.RegisterLibrarian("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    public sealed class Librarian64 :
        LibrarianBase
    {
        public Librarian64()
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.Macros.Add("ArchiverPath", Bam.Core.TokenizedString.Create(@"$(0)\lib.exe", null, new Bam.Core.TokenizedStringArray(meta.Bin64Dir)));
        }
    }
}

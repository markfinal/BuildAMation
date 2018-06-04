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
namespace VisualCCommon
{
    public abstract class LibrarianBase :
        C.LibrarianTool
    {
        private string
        getLibrarianPath(
            C.EBit depth)
        {
            const string executable = "lib.exe";
            foreach (var path in this.EnvironmentVariables["PATH"])
            {
                var installLocation = Bam.Core.OSUtilities.GetInstallLocation(
                    executable,
                    path.ToString(),
                    this.GetType().Name,
                    throwOnFailure: false
                );
                if (null != installLocation)
                {
                    return installLocation.First();
                }
            }
            var message = new System.Text.StringBuilder();
            message.AppendFormat("Unable to locate {0} for {1}-bit on these search locations:", executable, (int)depth);
            message.AppendLine();
            foreach (var path in this.EnvironmentVariables["PATH"])
            {
                message.AppendFormat("\t{0}", path.ToString());
                message.AppendLine();
            }
            throw new Bam.Core.Exception(message.ToString());
        }

        protected LibrarianBase(
            C.EBit depth)
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            var discovery = meta as C.IToolchainDiscovery;
            discovery.discover(depth);
            this.Macros.Add("InstallPath", meta.InstallDir);
            this.EnvironmentVariables = meta.Environment(depth);
            var fullLibExePath = this.getLibrarianPath(depth);
            this.Macros.Add("ArchiverPath", Bam.Core.TokenizedString.CreateVerbatim(fullLibExePath));
            this.Macros.AddVerbatim("libprefix", string.Empty);
            this.Macros.AddVerbatim("libext", ".lib");
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
            :
            base(C.EBit.ThirtyTwo)
        { }
    }

    [C.RegisterLibrarian("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    public sealed class Librarian64 :
        LibrarianBase
    {
        public Librarian64()
            :
            base(C.EBit.SixtyFour)
        { }
    }
}

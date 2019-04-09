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
using System.Linq;
namespace Publisher
{
    public abstract class ZipTool :
        Bam.Core.PreBuiltTool
    {
        protected override void
        EvaluateInternal() => this.ReasonToExecute = null;
    }

    public sealed class ZipPosix :
        ZipTool
    {
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new ZipSettings(module);

        public override Bam.Core.TokenizedString Executable => Bam.Core.TokenizedString.CreateVerbatim(Bam.Core.OSUtilities.GetInstallLocation("bash").First());

        public override Bam.Core.TokenizedStringArray InitialArguments
        {
            get
            {
                var initArgs = new Bam.Core.TokenizedStringArray();
                initArgs.Add(Bam.Core.TokenizedString.CreateVerbatim("-c"));
                initArgs.Add(Bam.Core.TokenizedString.CreateVerbatim("\""));
                initArgs.Add(Bam.Core.TokenizedString.CreateVerbatim("zip"));
                return initArgs;
            }
        }

        public override Bam.Core.TokenizedStringArray TerminatingArguments
        {
            get
            {
                var termArgs = new Bam.Core.TokenizedStringArray();
                termArgs.Add(Bam.Core.TokenizedString.CreateVerbatim("* \""));
                return termArgs;
            }
        }

        public override Bam.Core.Array<int> SuccessfulExitCodes
        {
            get
            {
                var exit_codes = base.SuccessfulExitCodes;
                exit_codes.Add(12); // means 'nothing to do'
                return exit_codes;
            }
        }
    }

    public sealed class ZipWin :
        ZipTool
    {
        private Bam.Core.TokenizedString sevenZipExePath;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
#if D_NUGET_NUGET_CLIENT && D_NUGET_7_ZIP_COMMANDLINE
            var nugetHomeDir = NuGet.Common.NuGetEnvironment.GetFolderPath(NuGet.Common.NuGetFolderPath.NuGetHome);
            var nugetPackageDir = System.IO.Path.Combine(nugetHomeDir, "packages");
            var repo = new NuGet.Repositories.NuGetv3LocalRepository(nugetPackageDir);
            var sevenZipInstalls = repo.FindPackagesById("7-Zip.CommandLine");
            if (!sevenZipInstalls.Any())
            {
                // this should not happen as package restoration should handle this
                throw new Bam.Core.Exception("Unable to locate any NuGet package for 7-zip");
            }
            var thisPackage = Bam.Core.Graph.Instance.Packages.First(item => item.Name.Equals("Publisher", System.StringComparison.Ordinal));
            var required7Zip = thisPackage.NuGetPackages.First(item => item.Identifier.Equals("7-Zip.CommandLine", System.StringComparison.Ordinal));
            var requested7Zip = sevenZipInstalls.First(item => item.Version.ToNormalizedString().Equals(required7Zip.Version, System.StringComparison.Ordinal));
            var sevenzip_tools_dir = System.IO.Path.Combine(requested7Zip.ExpandedPath, "tools");
            var sevenzipa_exe_path = System.IO.Path.Combine(sevenzip_tools_dir, "7za.exe");
            if (!System.IO.File.Exists(sevenzipa_exe_path))
            {
                throw new Bam.Core.Exception($"Unable to locate 7za.exe from NuGet package at '{sevenzipa_exe_path}'");
            }
            this.sevenZipExePath = Bam.Core.TokenizedString.CreateVerbatim(sevenzipa_exe_path);
#endif

            // since the toolPath macro is needed to evaluate the Executable property
            // in the check for existence
            base.Init(parent);
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) => new SevenZipSettings(module);

        public override Bam.Core.TokenizedString Executable => this.sevenZipExePath;

        public override Bam.Core.TokenizedStringArray TerminatingArguments
        {
            get
            {
                var termArgs = new Bam.Core.TokenizedStringArray();
                termArgs.Add(Bam.Core.TokenizedString.CreateVerbatim("*"));
                return termArgs;
            }
        }
    }
}

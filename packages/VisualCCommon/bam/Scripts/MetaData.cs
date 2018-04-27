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
namespace VisualCCommon
{
    public abstract class MetaData :
        Bam.Core.PackageMetaData
    {
        protected System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string,object>();

        private void
        findvswhere()
        {
            const string vswhere_version = "vswhere.2.4.1"; // match packages.config in the Bam project

            // find vswhere from the NuGet package download
            var nugetDir = Bam.Core.Graph.Instance.ProcessState.NuGetDirectory;
            var vswhere_dir = System.IO.Path.Combine(nugetDir, vswhere_version);
            if (!System.IO.Directory.Exists(vswhere_dir))
            {
                throw new Bam.Core.Exception("Unable to locate NuGet package for {0} at {1}", vswhere_version, vswhere_dir);
            }
            var vswhere_tools_dir = System.IO.Path.Combine(vswhere_dir, "tools");
            var vswhere_exe_path = System.IO.Path.Combine(vswhere_tools_dir, "vswhere.exe");
            if (!System.IO.File.Exists(vswhere_exe_path))
            {
                throw new Bam.Core.Exception("Unable to locate vswhere.exe from NuGet package at '{0}'", vswhere_exe_path);
            }
            this.vswherePath = vswhere_exe_path;
        }

        private static string
        RunExecutable(
            string executable,
            string arguments)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = executable;
            processStartInfo.Arguments = arguments;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true; // swallow
            processStartInfo.UseShellExecute = false;
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                return null;
            }
            return process.StandardOutput.ReadToEnd().TrimEnd(System.Environment.NewLine.ToCharArray());
        }

        protected string
        vswhere_getinstallpath(
            int vs_major_version)
        {
            if (vs_major_version >= 15)
            {
                var result = RunExecutable(this.vswherePath, System.String.Format("-property installationPath -version {0}", vs_major_version));
                return result;
            }
            else
            {
                var result = RunExecutable(this.vswherePath, System.String.Format("-legacy -property installationPath -version [{0}]", vs_major_version));
                if (System.String.IsNullOrEmpty(result))
                {
                    throw new Bam.Core.Exception("Unable to locate installation directory for Visual Studio major version {0}", vs_major_version);
                }
                return result;
            }
        }

        public MetaData()
        {
            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }
            this.findvswhere();
        }

        private string vswherePath
        {
            get;
            set;
        }
    }
}

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
namespace Qt
{
    public sealed class Toolset :
        QtCommon.Toolset
    {
        private string installPath = null;

        protected override string
        GetInstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return this.installPath;
            }

            if (Bam.Core.State.HasCategory("Qt") && Bam.Core.State.Has("Qt", "InstallPath"))
            {
                this.installPath = Bam.Core.State.Get("Qt", "InstallPath") as string;
                Bam.Core.Log.DebugMessage("Qt install path set from command line to '{0}'", this.installPath);
                return this.installPath;
            }

            string installPath = null;
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Trolltech\Versions\4.7.1"))
                {
                    if (null == key)
                    {
                        throw new Bam.Core.Exception("Qt libraries for 4.7.1 were not installed");
                    }

                    installPath = key.GetValue("InstallDir") as string;
                    if (null == installPath)
                    {
                        throw new Bam.Core.Exception("Unable to locate InstallDir registry key for Qt 4.7.1");
                    }
                    Bam.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);
                }
            }
            else if (Bam.Core.OSUtilities.IsLinuxHosting)
            {
                installPath = @"/usr/local/Trolltech/Qt-4.7.1"; // default installation directory
            }
            else if (Bam.Core.OSUtilities.IsOSXHosting)
            {
                // Qt headers and libs are installed in /Library/Frameworks/ ...
                installPath = @"/Developer/Tools/Qt";
            }
            else
            {
                throw new Bam.Core.Exception("Qt identification has not been implemented on the current platform");
            }
            this.installPath = installPath;

            return installPath;
        }

        protected override string
        GetVersionNumber()
        {
            return "4.7.1";
        }
    }
}

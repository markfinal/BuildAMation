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
using System.Linq;
namespace QtCommon
{
    class QtInstallPath :
        Bam.Core.IStringCommandLineArgument,
        Bam.Core.ICommandLineArgumentDefault<string>
    {
        string Bam.Core.ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Define the Qt install location";
            }
        }

        string Bam.Core.ICommandLineArgument.LongName
        {
            get
            {
                return "--Qt.installPath";
            }
        }

        string Bam.Core.ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string Bam.Core.ICommandLineArgumentDefault<string>.Default
        {
            get
            {
                var graph = Bam.Core.Graph.Instance;
                var qtVersion = graph.Packages.Where(item => item.Name == "Qt").ElementAt(0).Version;

                switch (Bam.Core.OSUtilities.CurrentOS)
                {
                    case Bam.Core.EPlatform.Windows:
                        return GetWindowsInstallPath(qtVersion);

                    case Bam.Core.EPlatform.Linux:
                        return GetLinuxInstallPath(qtVersion);

                    case Bam.Core.EPlatform.OSX:
                        return GetOSXInstallPath(qtVersion);
                }

                throw new Bam.Core.Exception("Unable to determine default Qt {0} installation", qtVersion);
            }
        }

        private static string
        GetWindowsInstallPath(
            string qtVersion)
        {
            using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(System.String.Format(@"Trolltech\Versions\{0}", qtVersion)))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception("Qt libraries for {0} were not installed", qtVersion);
                }

                var installPath = key.GetValue("InstallDir") as string;
                if (null == installPath)
                {
                    throw new Bam.Core.Exception("Unable to locate InstallDir registry key for Qt {0}", qtVersion);
                }
                Bam.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);
                return installPath;
            }
        }

        private static string
        GetLinuxInstallPath(
            string qtVersion)
        {
            var installPath = System.String.Format("/usr/local/Trolltech/Qt-{0}", qtVersion);
            return installPath;
        }

        private static string
        GetOSXInstallPath(
            string qtVersion)
        {
            return @"/Developer/Tools/Qt";
        }
    }
}

#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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
                using (Microsoft.Win32.RegistryKey key = Bam.Core.Win32RegistryUtilities.OpenCUSoftwareKey(@"Microsoft\Windows\CurrentVersion\Uninstall\Qt 5.3.1"))
                {
                    if (null == key)
                    {
                        throw new Bam.Core.Exception("Qt libraries for 5.3.1 were not installed");
                    }

                    installPath = key.GetValue("InstallLocation") as string;
                    if (null == installPath)
                    {
                        throw new Bam.Core.Exception("Unable to locate InstallLocation registry key for Qt 5.3.1");
                    }

                    // precompiled binaries now have a subdirectory indicating their flavour
                    if (Bam.Core.OSUtilities.Is64Bit(baseTarget))
                    {
                        installPath += @"\5.3\msvc2013_64_opengl";
                    }
                    else
                    {
                        installPath += @"\5.3\msvc2013_opengl";
                    }

                    Bam.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);
                }
            }
            else if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                var homeDir = System.Environment.GetEnvironmentVariable("HOME");
                if (null != homeDir)
                {
                    installPath = System.IO.Path.Combine(homeDir, "Qt5.3.1");
                    installPath = System.IO.Path.Combine(installPath, "5.3");
                    installPath = System.IO.Path.Combine(installPath, "gcc_64");
                }
                else
                {
                    installPath = @"/usr/local/Qt-5.3.1/5.3/gcc_64";
                }
            }
            else if (Bam.Core.OSUtilities.IsOSXHosting)
            {
                var homeDir = System.Environment.GetEnvironmentVariable("HOME");
                if (null != homeDir)
                {
                    installPath = System.IO.Path.Combine(homeDir, "Qt5.3.1");
                    installPath = System.IO.Path.Combine(installPath, "5.3");
                    installPath = System.IO.Path.Combine(installPath, "clang_64");
                }
                else
                {
                    installPath = @"/usr/local/Qt-5.3.1/5.3/clang_64";
                }
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
            return "5.3.1";
        }
    }
}

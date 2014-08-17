#region License
// Copyright 2010-2014 Mark Final
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
#endregion
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
                using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Trolltech\Versions\4.6.3"))
                {
                    if (null == key)
                    {
                        throw new Bam.Core.Exception("Qt libraries for 4.6.3 were not installed");
                    }

                    installPath = key.GetValue("InstallDir") as string;
                    if (null == installPath)
                    {
                        throw new Bam.Core.Exception("Unable to locate InstallDir registry key for Qt 4.6.3");
                    }
                    Bam.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);
                }
            }
            else if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                installPath = @"/usr/local/Trolltech/Qt-4.6.3"; // default installation directory
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
            return "4.6.3";
        }
    }
}

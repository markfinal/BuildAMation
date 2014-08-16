// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
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
            else if (Bam.Core.OSUtilities.IsUnixHosting)
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

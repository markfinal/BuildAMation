// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Toolset : QtCommon.Toolset
    {
        private string installPath = null;

        protected override string GetInstallPath(Opus.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return this.installPath;
            }

            if (Opus.Core.State.HasCategory("Qt") && Opus.Core.State.Has("Qt", "InstallPath"))
            {
                this.installPath = Opus.Core.State.Get("Qt", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Qt install path set from command line to '{0}'", this.installPath);
                return this.installPath;
            }

            string installPath = null;
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Trolltech\Versions\4.6.3"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("Qt libraries for 4.6.3 were not installed");
                    }

                    installPath = key.GetValue("InstallDir") as string;
                    if (null == installPath)
                    {
                        throw new Opus.Core.Exception("Unable to locate InstallDir registry key for Qt 4.6.3");
                    }
                    Opus.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);
                }
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting)
            {
                installPath = @"/usr/local/Trolltech/Qt-4.6.3"; // default installation directory
            }
            else if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                // Qt headers and libs are installed in /Library/Frameworks/ ...
                installPath = @"/Developer/Tools/Qt";
            }
            else
            {
                throw new Opus.Core.Exception("Qt identification has not been implemented on the current platform");
            }
            this.installPath = installPath;

            return installPath;
        }

        protected override string GetVersionNumber()
        {
            return "4.6.3";
        }
    }
}
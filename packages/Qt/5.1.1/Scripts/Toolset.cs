#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
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
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenCUSoftwareKey(@"Microsoft\Windows\CurrentVersion\Uninstall\Qt 5.1.1"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("Qt libraries for 5.1.1 were not installed");
                    }

                    installPath = key.GetValue("InstallLocation") as string;
                    if (null == installPath)
                    {
                        throw new Opus.Core.Exception("Unable to locate InstallLocation registry key for Qt 5.1.1");
                    }

                    // precompiled binaries now have a subdirectory indicating their flavour
                    installPath += @"\5.1.1\msvc2010_opengl";

                    Opus.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);
                }
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting)
            {
                var homeDir = System.Environment.GetEnvironmentVariable("HOME");
                if (null != homeDir)
                {
                    installPath = System.IO.Path.Combine(homeDir, "Qt5.1.1");
                    installPath = System.IO.Path.Combine(installPath, "5.1.1");
                    installPath = System.IO.Path.Combine(installPath, "gcc_64");
                }
                else
                {
                    installPath = @"/usr/local/Qt-5.1.1/5.1.1/gcc_64";
                }
            }
            else if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                var homeDir = System.Environment.GetEnvironmentVariable("HOME");
                if (null != homeDir)
                {
                    installPath = System.IO.Path.Combine(homeDir, "Qt5.1.1");
                    installPath = System.IO.Path.Combine(installPath, "5.1.1");
                    installPath = System.IO.Path.Combine(installPath, "clang_64");
                }
                else
                {
                    installPath = @"/usr/local/Qt-5.1.1/5.1.1/clang_64";
                }
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
            return "5.1.1";
        }
    }
}
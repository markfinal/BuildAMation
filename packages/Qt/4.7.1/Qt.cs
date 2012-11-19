// <copyright file="Qt.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>

[assembly:Opus.Core.RegisterToolset("Qt", typeof(Qt.Toolset))]

namespace Qt
{
    public sealed class Qt : QtCommon.QtCommon
    {
        // NEW STYLE
#if true
#else
        public static string VersionString
        {
            get
            {
                return "4.7.1";
            }
        }
    
        static Qt()
        {
            if (Opus.Core.State.HasCategory("Qt") && Opus.Core.State.Has("Qt", "InstallPath"))
            {
                installPath = Opus.Core.State.Get("Qt", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Qt install path set from command line to '{0}'", installPath);
            }

            if (null == installPath)
            {
                if (Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Trolltech\Versions\4.7.1"))
                    {
                        if (null == key)
                        {
                            throw new Opus.Core.Exception("Qt libraries for 4.7.1 were not installed");
                        }

                        installPath = key.GetValue("InstallDir") as string;
                        if (null == installPath)
                        {
                            throw new Opus.Core.Exception("Unable to locate InstallDir registry key for Qt 4.7.1");
                        }
                        Opus.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);
                    }
                }
                else if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    installPath = @"/usr/local/Trolltech/Qt-4.7.1"; // default installation directory
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
            }
        }
#endif

        public Qt(Opus.Core.Target target)
        {
            // NEW STYLE
#if true
            // TODO: investigate this - Qt is a ThirdpartyModule, with no toolset
            // what is the best course of action?
            string installPath = Opus.Core.ToolsetFactory.CreateToolset(typeof(Toolset)).InstallPath((Opus.Core.BaseTarget)target);
            //string installPath = target.Toolset.InstallPath((Opus.Core.BaseTarget)target);
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                this.BinPath = installPath;
            }
            else
            {
                this.BinPath = System.IO.Path.Combine(installPath, "bin");
            }
#else
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                this.BinPath = installPath;
            }
            else
            {
                this.BinPath = System.IO.Path.Combine(installPath, "bin");
            }
#endif

            this.LibPath = System.IO.Path.Combine(installPath, "lib");
            this.includePaths.Add(System.IO.Path.Combine(installPath, "include"));
        }
    }
}

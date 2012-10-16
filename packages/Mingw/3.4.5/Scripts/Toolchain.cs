// <copyright file="Toolchain.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed class Toolchain : MingwCommon.Toolchain
    {
        //NEW STYLE
#if true
#else
        private string installPath;
        private string binFolder;
#endif

        public Toolchain(Opus.Core.Target target)
        {
            // NEW STYLE
#if true
#else
            if (Opus.Core.State.HasCategory("Mingw") && Opus.Core.State.Has("Mingw", "InstallPath"))
            {
                this.installPath = Opus.Core.State.Get("Mingw", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Mingw install path set from command line to '{0}'", this.installPath);
            }

            if (null == installPath)
            {
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\Windows\CurrentVersion\Uninstall\MinGW"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("mingw was not installed");
                    }

                    this.installPath = key.GetValue("InstallLocation") as string;
                    Opus.Core.Log.DebugMessage("Mingw: Install path from registry '{0}'", this.installPath);
                }
            }

            this.binFolder = System.IO.Path.Combine(this.installPath, "bin");
            this.Environment = new Opus.Core.StringArray();
            this.Environment.Add(this.binFolder);
#endif
        }

#if false
        public override string InstallPath(Opus.Core.Target target)
        {
            return this.installPath;
        }

        public override string BinPath(Opus.Core.Target target)
        {
            return this.binFolder;
        }

        public override Opus.Core.StringArray Environment
        {
            get;
            protected set;
        }
#endif

        public static string GetVersion
        {
            get
            {
                if (!Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    return "non-Windows-proxy";
                }

                string packageName = "Mingw";
                Opus.Core.PackageInformation package = Opus.Core.State.PackageInfo[packageName];
                if (null == package)
                {
                    string packages = null;
                    foreach (Opus.Core.PackageInformation p in Opus.Core.State.PackageInfo)
                    {
                        packages += p.Name + " ";
                    }
                    throw new Opus.Core.Exception(System.String.Format("Unable to locate package '{0}'; packages available are '{1}'", packageName, packages));
                }
                string version = package.Version;
                return version;
            }
        }
    }
}
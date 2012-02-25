// <copyright file="Toolchain.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Intel package</summary>
// <author>Mark Final</author>
namespace Intel
{
    public sealed class Toolchain : GccCommon.Toolchain
    {
        private string installPath;
        private string binFolder;

        public Toolchain(Opus.Core.Target target)
        {
            if (Opus.Core.State.HasCategory("Intel") && Opus.Core.State.Has("Intel", "InstallPath"))
            {
                this.installPath = Opus.Core.State.Get("Intel", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Intel install path set from command line to '{0}'", this.installPath);
            }

            if (null == this.installPath)
            {
                this.installPath = "/opt/intel/bin";
            }

            this.binFolder = this.installPath;
        }

        public override string InstallPath(Opus.Core.Target target)
        {
            return this.installPath;
        }

        public override string BinPath(Opus.Core.Target target)
        {
            return this.binFolder;
        }

        public static string VersionString
        {
            get
            {
                if (!Opus.Core.OSUtilities.IsUnixHosting)
                {
                    return "non-Unix-proxy";
                }

                string packageName = "Intel";
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
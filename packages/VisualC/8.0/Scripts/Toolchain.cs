// <copyright file="Toolchain.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
namespace VisualC
{
    public sealed class Toolchain : VisualCCommon.Toolchain
    {
        private string installPath;
        private string bin32Folder;
        private string bin64Folder;
        private string bin6432Folder;
        private string lib32Folder;
        private string lib64Folder;

        static Toolchain()
        {
            if (!Opus.Core.State.HasCategory("VSSolutionBuilder"))
            {
                Opus.Core.State.AddCategory("VSSolutionBuilder");
                Opus.Core.State.Add<System.Type>("VSSolutionBuilder", "SolutionType", typeof(VisualC.Solution));
            }
        }

        public Toolchain(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            if (Opus.Core.State.HasCategory("VisualC") && Opus.Core.State.Has("VisualC", "InstallPath"))
            {
                this.installPath = Opus.Core.State.Get("VisualC", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("VisualC 2005 install path set from command line to '{0}'", this.installPath);
            }

            if (null == this.installPath)
            {
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\VisualStudio\Sxs\VC7"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("VisualStudio was not installed");
                    }

                    this.installPath = key.GetValue("8.0") as string;
                    if (null == this.installPath)
                    {
                        throw new Opus.Core.Exception("VisualStudio 2005 was not installed");
                    }

                    this.installPath = this.installPath.TrimEnd(new[] { System.IO.Path.DirectorySeparatorChar });
                    Opus.Core.Log.DebugMessage("VisualStudio 2005: Installation path from registry '{0}'", this.installPath);
                }
            }

            this.bin32Folder = System.IO.Path.Combine(this.installPath, "bin");
            this.bin64Folder = System.IO.Path.Combine(this.bin32Folder, "amd64");
            this.bin6432Folder = System.IO.Path.Combine(this.bin32Folder, "x86_amd64");

            this.lib32Folder = System.IO.Path.Combine(this.installPath, "lib");
            this.lib64Folder = System.IO.Path.Combine(this.lib32Folder, "amd64");

            string parent = System.IO.Directory.GetParent(this.installPath).FullName;
            string common7 = System.IO.Path.Combine(parent, "Common7");
            string ide = System.IO.Path.Combine(common7, "IDE");
            //string tools = System.IO.Path.Combine(common7, "Tools");

            this.Environment = new Opus.Core.StringArray();
            this.Environment.Add(ide);
        }

        public override string InstallPath(Opus.Core.Target target)
        {
            return this.installPath;
        }

        public override string BinPath(Opus.Core.Target target)
        {
            if (target.Platform == Opus.Core.EPlatform.Win64)
            {
                if (Opus.Core.OSUtilities.Is64BitHosting)
                {
                    return this.bin64Folder;
                }
                else
                {
                    return this.bin6432Folder;
                }
            }
            else
            {
                return this.bin32Folder;
            }
        }

        public override Opus.Core.StringArray Environment
        {
            get;
            protected set;
        }

        public override string LibPath(Opus.Core.Target target)
        {
            if (target.Platform == Opus.Core.EPlatform.Win64)
            {
                return this.lib64Folder;
            }
            else
            {
                return this.lib32Folder;
            }
        }

        public static string VersionString
        {
            get
            {
                if (!Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    return "non-Windows-proxy";
                }

                string packageName = "VisualC";
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

                string versionNumber = package.Version;
                int LCID = 1033; // Culture (English - United States)
                string platformName;
                string editionName;
                string vcRegistryKeyPath;

                // try professional version first
                platformName = "VS";
                editionName = "PRO";
                vcRegistryKeyPath = GetVCRegistryKeyPath(platformName, versionNumber, editionName, LCID);
                if (Opus.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(vcRegistryKeyPath))
                {
                    string versionString = GetVersionAndEditionString(vcRegistryKeyPath, package);
                    return versionString;
                }
                else
                {
                    // now try Express edition
                    platformName = "VC";
                    editionName = "EXP";
                    vcRegistryKeyPath = GetVCRegistryKeyPath(platformName, versionNumber, editionName, LCID);
                    if (Opus.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(vcRegistryKeyPath))
                    {
                        string versionString = GetVersionAndEditionString(vcRegistryKeyPath, package);
                        return versionString;
                    }
                    else
                    {
                        throw new Opus.Core.Exception("Unable to locate registry key for the VisualStudio service pack");
                    }
                }
            }
        }

        public override string ObjectFileExtension
        {
            get
            {
                return ".obj";
            }
        }

        public override string StaticLibraryExtension
        {
            get
            {
                return ".lib";
            }
        }
    }
}
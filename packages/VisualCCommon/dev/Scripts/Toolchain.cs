// <copyright file="Toolchain.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract class Toolchain : C.Toolchain
    {
        public virtual VisualStudioProcessor.EVisualStudioTarget VisualStudioTarget
        {
            get
            {
                return VisualStudioProcessor.EVisualStudioTarget.VCPROJ;
            }
        }

        public abstract string LibPath(Opus.Core.Target target);

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

        public override string StaticImportLibraryExtension
        {
            get
            {
                return this.StaticLibraryExtension;
            }
        }

        protected static string GetVCRegistryKeyPath(string platformName, string versionNumber, string editionName, int LCID)
        {
            string keyPath = System.String.Format(@"Microsoft\DevDiv\{0}\Servicing\{1}\{2}\{3}", platformName, versionNumber, editionName, LCID);
            return keyPath;
        }

        protected static string GetVersionAndEditionString(string registryKeyPath, Opus.Core.PackageInformation package)
        {
            string edition = null;
            using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(registryKeyPath))
            {
                if (null == key)
                {
                    throw new Opus.Core.Exception("Unable to locate registry key for the VisualStudio service pack");
                }

                edition = key.GetValue("SPName") as string;
            }

            string version = System.String.Format("{0}{1}", package.Version, edition);
            return version;
        }
    }
}
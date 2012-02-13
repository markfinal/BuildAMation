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

        public override string PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        public override string ObjectFileSuffix
        {
            get
            {
                return ".obj";
            }
        }

        public override string StaticLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        public override string StaticLibrarySuffix
        {
            get
            {
                return ".lib";
            }
        }

        public override string StaticImportLibraryPrefix
        {
            get
            {
                return this.StaticLibraryPrefix;
            }
        }

        public override string StaticImportLibrarySuffix
        {
            get
            {
                return this.StaticLibrarySuffix;
            }
        }

        public override string DynamicLibraryPrefix
        {
            get
            {
                return this.StaticLibraryPrefix;
            }
        }

        public override string DynamicLibrarySuffix
        {
            get
            {
                return ".dll";
            }
        }

        public override string ExecutableSuffix
        {
            get
            {
                return ".exe";
            }
        }

        public override string MapFileSuffix
        {
            get
            {
                return ".map";
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
                    throw new Opus.Core.Exception(System.String.Format("Unable to locate registry key, '{0}', for the VisualStudio service pack", registryKeyPath), false);
                }

                edition = key.GetValue("SPName") as string;
            }

            string version = System.String.Format("{0}{1}", package.Version, edition);
            return version;
        }
    }
}
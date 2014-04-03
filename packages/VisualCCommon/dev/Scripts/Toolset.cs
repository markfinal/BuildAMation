// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract class Toolset : Opus.Core.IToolset
    {
        public string installPath;
        public string bin32Folder;
        public string bin64Folder;
        public string bin6432Folder;
        public Opus.Core.StringArray lib32Folder = new Opus.Core.StringArray();
        public Opus.Core.StringArray lib64Folder = new Opus.Core.StringArray();
        protected Opus.Core.StringArray environment = new Opus.Core.StringArray();

        protected abstract void GetInstallPath();
        protected abstract string GetVersion(Opus.Core.BaseTarget baseTarget);

        protected System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType>();

        protected Toolset()
        {
            this.toolConfig[typeof(C.INullOpTool)] = new Opus.Core.ToolAndOptionType(null, null);
            this.toolConfig[typeof(C.IThirdPartyTool)] = new Opus.Core.ToolAndOptionType(null, typeof(C.ThirdPartyOptionCollection));
        }

        protected virtual string GetBinPath(Opus.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath();

            if (baseTarget.HasPlatform(Opus.Core.EPlatform.Win64))
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

        private static string GetVCRegistryKeyPath(string platformName, string versionNumber, string editionName, int LCID)
        {
            string keyPath = System.String.Format(@"Microsoft\DevDiv\{0}\Servicing\{1}\{2}\{3}", platformName, versionNumber, editionName, LCID);
            return keyPath;
        }

        private static string GetVersionAndEditionString(string registryKeyPath, string versionNumber)
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

            string version = System.String.Format("{0}{1}", versionNumber, edition);
            return version;
        }

        protected string GetVersionString(string versionNumber)
        {
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
                string versionString = GetVersionAndEditionString(vcRegistryKeyPath, versionNumber);
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
                    string versionString = GetVersionAndEditionString(vcRegistryKeyPath, versionNumber);
                    return versionString;
                }
                else
                {
                    // now try regular edition
                    platformName = "VC";
                    editionName = "RED";
                    vcRegistryKeyPath = GetVCRegistryKeyPath(platformName, versionNumber, editionName, LCID);
                    if (Opus.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(vcRegistryKeyPath))
                    {
                        string versionString = GetVersionAndEditionString(vcRegistryKeyPath, versionNumber);
                        return versionString;
                    }
                    else
                    {
                        // now try latest 
                        platformName = "VC";
                        editionName = "CompilerCore";
                        vcRegistryKeyPath = GetVCRegistryKeyPath(platformName, versionNumber, editionName, LCID);
                        if (Opus.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(vcRegistryKeyPath))
                        {
                            string versionString = GetVersionAndEditionString(vcRegistryKeyPath, versionNumber);
                            return versionString;
                        }
                        else
                        {
                            throw new Opus.Core.Exception("Unable to locate registry key, '{0}', for the VisualStudio {1} service pack", vcRegistryKeyPath, versionNumber);
                        }
                    }
                }
            }
        }

        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            return this.GetBinPath(baseTarget);
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get
            {
                this.GetInstallPath();
                return this.environment;
            }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath();
            return this.installPath;
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return this.GetVersion(baseTarget);
        }

        bool Opus.Core.IToolset.HasTool(System.Type toolType)
        {
            return this.toolConfig.ContainsKey(toolType);
        }

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            if (!(this as Opus.Core.IToolset).HasTool(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].Tool;
        }

        System.Type Opus.Core.IToolset.ToolOptionType(System.Type toolType)
        {
            if (!(this as Opus.Core.IToolset).HasTool(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].OptionsType;
        }

        #endregion
    }
}

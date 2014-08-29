#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace VisualCCommon
{
    public abstract class Toolset :
        Bam.Core.IToolset
    {
        public string installPath;
        public string bin32Folder;
        public string bin64Folder;
        public string bin6432Folder;
        public Bam.Core.StringArray lib32Folder = new Bam.Core.StringArray();
        public Bam.Core.StringArray lib64Folder = new Bam.Core.StringArray();
        protected Bam.Core.StringArray environment = new Bam.Core.StringArray();

        protected abstract void
        GetInstallPath();

        protected abstract string
        GetVersion(
            Bam.Core.BaseTarget baseTarget);

        protected System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType>();

        protected
        Toolset()
        {
            this.toolConfig[typeof(C.INullOpTool)] = new Bam.Core.ToolAndOptionType(null, null);
            this.toolConfig[typeof(C.IThirdPartyTool)] = new Bam.Core.ToolAndOptionType(null, typeof(C.ThirdPartyOptionCollection));
        }

        protected virtual string
        GetBinPath(
            Bam.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath();

            if (baseTarget.HasPlatform(Bam.Core.EPlatform.Win64))
            {
                if (Bam.Core.OSUtilities.Is64BitHosting)
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

        private static string
        GetVCRegistryKeyPath(
            string platformName,
            string versionNumber,
            string editionName,
            int LCID)
        {
            var keyPath = System.String.Format(@"Microsoft\DevDiv\{0}\Servicing\{1}\{2}\{3}", platformName, versionNumber, editionName, LCID);
            return keyPath;
        }

        private static string
        GetVersionAndEditionString(
            string registryKeyPath,
            string versionNumber)
        {
            string edition = null;
            using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(registryKeyPath))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception(System.String.Format("Unable to locate registry key, '{0}', for the VisualStudio service pack", registryKeyPath), false);
                }

                edition = key.GetValue("SPName") as string;
            }

            var version = System.String.Format("{0}{1}", versionNumber, edition);
            return version;
        }

        protected string
        GetVersionString(
            string versionNumber)
        {
            var LCID = 1033; // Culture (English - United States)
            string platformName;
            string editionName;
            string vcRegistryKeyPath;

            // try professional version first
            platformName = "VS";
            editionName = "PRO";
            vcRegistryKeyPath = GetVCRegistryKeyPath(platformName, versionNumber, editionName, LCID);
            if (Bam.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(vcRegistryKeyPath))
            {
                var versionString = GetVersionAndEditionString(vcRegistryKeyPath, versionNumber);
                return versionString;
            }
            else
            {
                // now try Express edition
                platformName = "VC";
                editionName = "EXP";
                vcRegistryKeyPath = GetVCRegistryKeyPath(platformName, versionNumber, editionName, LCID);
                if (Bam.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(vcRegistryKeyPath))
                {
                    var versionString = GetVersionAndEditionString(vcRegistryKeyPath, versionNumber);
                    return versionString;
                }
                else
                {
                    // now try regular edition
                    platformName = "VC";
                    editionName = "RED";
                    vcRegistryKeyPath = GetVCRegistryKeyPath(platformName, versionNumber, editionName, LCID);
                    if (Bam.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(vcRegistryKeyPath))
                    {
                        var versionString = GetVersionAndEditionString(vcRegistryKeyPath, versionNumber);
                        return versionString;
                    }
                    else
                    {
                        // now try latest
                        platformName = "VC";
                        editionName = "CompilerCore";
                        vcRegistryKeyPath = GetVCRegistryKeyPath(platformName, versionNumber, editionName, LCID);
                        if (Bam.Core.Win32RegistryUtilities.Does32BitLMSoftwareKeyExist(vcRegistryKeyPath))
                        {
                            var versionString = GetVersionAndEditionString(vcRegistryKeyPath, versionNumber);
                            return versionString;
                        }
                        else
                        {
                            throw new Bam.Core.Exception("Unable to locate registry key, '{0}', for the VisualStudio {1} service pack", vcRegistryKeyPath, versionNumber);
                        }
                    }
                }
            }
        }

        #region IToolset Members

        string
        Bam.Core.IToolset.BinPath(
            Bam.Core.BaseTarget baseTarget)
        {
            return this.GetBinPath(baseTarget);
        }

        Bam.Core.StringArray Bam.Core.IToolset.Environment
        {
            get
            {
                this.GetInstallPath();
                return this.environment;
            }
        }

        string
        Bam.Core.IToolset.InstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath();
            return this.installPath;
        }

        string
        Bam.Core.IToolset.Version(
            Bam.Core.BaseTarget baseTarget)
        {
            return this.GetVersion(baseTarget);
        }

        bool
        Bam.Core.IToolset.HasTool(
            System.Type toolType)
        {
            return this.toolConfig.ContainsKey(toolType);
        }

        Bam.Core.ITool
        Bam.Core.IToolset.Tool(
            System.Type toolType)
        {
            if (!(this as Bam.Core.IToolset).HasTool(toolType))
            {
                throw new Bam.Core.Exception("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].Tool;
        }

        System.Type
        Bam.Core.IToolset.ToolOptionType(
            System.Type toolType)
        {
            if (!(this as Bam.Core.IToolset).HasTool(toolType))
            {
                throw new Bam.Core.Exception("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].OptionsType;
        }

        #endregion
    }
}

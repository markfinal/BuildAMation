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

        protected System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool> toolMap = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool>();
        protected System.Collections.Generic.Dictionary<System.Type, System.Type> toolOptionsMap = new System.Collections.Generic.Dictionary<System.Type, System.Type>();

        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
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

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            if (!this.toolMap.ContainsKey(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolMap[toolType];
        }

        System.Type Opus.Core.IToolset.ToolOptionType(System.Type toolType)
        {
            if (!this.toolOptionsMap.ContainsKey(toolType))
            {
                // if there is no tool then there will be no optionset
                if (!this.toolMap.ContainsKey(toolType))
                {
                    return null;
                }

                throw new Opus.Core.Exception("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolOptionsMap[toolType];
        }

        #endregion
    }
}

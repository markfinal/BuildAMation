// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public abstract class Toolset : Opus.Core.IToolset
    {
        protected string installPath;
        protected string binPath;
        protected Opus.Core.StringArray environment = new Opus.Core.StringArray();
        //public Opus.Core.StringArray includePaths = new Opus.Core.StringArray();

        protected abstract void GetInstallPath(Opus.Core.BaseTarget baseTarget);
        //protected abstract string GetVersion(Opus.Core.BaseTarget baseTarget);

        protected System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType>();

        protected MingwDetailData details;

        protected Toolset()
        {
            this.toolConfig[typeof(C.INullOpTool)] = new Opus.Core.ToolAndOptionType(null, null);
            this.toolConfig[typeof(C.IThirdPartyTool)] = new Opus.Core.ToolAndOptionType(null, typeof(C.ThirdPartyOptionCollection));
        }

        public MingwDetailData MingwDetail
        {
            get
            {
                return this.details;
            }
        }

        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return this.binPath;
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get
            {
                // TODO: is this needed?
                //this.GetInstallPath();
                return this.environment;
            }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return this.installPath;
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return this.details.Version;
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

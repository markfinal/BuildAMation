// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public abstract class Toolset :
        Bam.Core.IToolset
    {
        protected string installPath;
        protected string binPath;
        protected Bam.Core.StringArray environment = new Bam.Core.StringArray();
        //public Bam.Core.StringArray includePaths = new Bam.Core.StringArray();

        protected abstract void
        GetInstallPath(
            Bam.Core.BaseTarget baseTarget);
        //protected abstract string GetVersion(Bam.Core.BaseTarget baseTarget);

        protected System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType>();

        protected MingwDetailData details;

        protected
        Toolset()
        {
            this.toolConfig[typeof(C.INullOpTool)] = new Bam.Core.ToolAndOptionType(null, null);
            this.toolConfig[typeof(C.IThirdPartyTool)] = new Bam.Core.ToolAndOptionType(null, typeof(C.ThirdPartyOptionCollection));
        }

        public MingwDetailData MingwDetail
        {
            get
            {
                return this.details;
            }
        }

        #region IToolset Members

        string
        Bam.Core.IToolset.BinPath(
            Bam.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return this.binPath;
        }

        Bam.Core.StringArray Bam.Core.IToolset.Environment
        {
            get
            {
                // TODO: is this needed?
                //this.GetInstallPath();
                return this.environment;
            }
        }

        string
        Bam.Core.IToolset.InstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return this.installPath;
        }

        string
        Bam.Core.IToolset.Version(
            Bam.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return this.details.Version;
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

// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    public abstract class Toolset :
        Bam.Core.IToolset
    {
        protected string installPath;
        protected System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType>();
        protected ComposerXECommon.GccDetailData gccDetail;

        protected abstract string Version
        {
            get;
        }

        private void
        GetInstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return;
            }

            string installPath = null;
            if (Bam.Core.State.HasCategory("ComposerXE") && Bam.Core.State.Has("ComposerXE", "InstallPath"))
            {
                installPath = Bam.Core.State.Get("ComposerXE", "InstallPath") as string;
                Bam.Core.Log.DebugMessage("ComposerXE install path set from command line to '{0}'", installPath);
            }

            if (null == installPath)
            {
                installPath = "/opt/intel";
            }

            this.installPath = installPath;
            this.gccDetail = ComposerXECommon.GccDetailGatherer.DetermineSpecs(baseTarget, this);
        }

        public GccDetailData GccDetail
        {
            get
            {
                return this.gccDetail;
            }
        }

        #region IToolset implementation
        string
        Bam.Core.IToolset.Version(
            Bam.Core.BaseTarget baseTarget)
        {
            return this.Version;
        }

        string
        Bam.Core.IToolset.InstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return this.installPath;
        }

        string
        Bam.Core.IToolset.BinPath(
            Bam.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return System.IO.Path.Combine(this.installPath, "bin");
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

        Bam.Core.StringArray Bam.Core.IToolset.Environment
        {
            get
            {
                throw new System.NotImplementedException ();
            }
        }
        #endregion
    }
}

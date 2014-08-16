// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract class Toolset :
        Bam.Core.IToolset
    {
        protected string installPath;
        protected System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType>();
        protected GccCommon.GccDetailData gccDetail;

        protected
        Toolset()
        {
            this.toolConfig[typeof(C.INullOpTool)] = new Bam.Core.ToolAndOptionType(null, null);
            this.toolConfig[typeof(C.IThirdPartyTool)] = new Bam.Core.ToolAndOptionType(null, typeof(C.ThirdPartyOptionCollection));
            this.toolConfig[typeof(C.IPosixSharedLibrarySymlinksTool)] =
                new Bam.Core.ToolAndOptionType(new GccCommon.PosixSharedLibrarySymlinksTool(this), typeof(GccCommon.PosixSharedLibrarySymlinksOptionCollection));
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
            if (Bam.Core.State.HasCategory("Gcc") && Bam.Core.State.Has("Gcc", "InstallPath"))
            {
                installPath = Bam.Core.State.Get("Gcc", "InstallPath") as string;
                Bam.Core.Log.DebugMessage("Gcc install path set from command line to '{0}'", installPath);
            }

            if (null == installPath)
            {
                installPath = "/usr/bin";
            }

            this.installPath = installPath;
            this.gccDetail = GccCommon.GccDetailGatherer.DetermineSpecs(baseTarget, this);
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
            this.GetInstallPath(baseTarget);
            return this.gccDetail.Version;
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
            return this.installPath;
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

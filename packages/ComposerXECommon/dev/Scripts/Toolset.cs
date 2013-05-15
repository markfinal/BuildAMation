// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    public abstract class Toolset : Opus.Core.IToolset
    {
        protected string installPath;
        protected System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType>();
        protected ComposerXECommon.GccDetailData gccDetail;

        protected abstract string Version
        {
            get;
        }

        private void GetInstallPath(Opus.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return;
            }

            string installPath = null;
            if (Opus.Core.State.HasCategory("ComposerXE") && Opus.Core.State.Has("ComposerXE", "InstallPath"))
            {
                installPath = Opus.Core.State.Get("ComposerXE", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("ComposerXE install path set from command line to '{0}'", installPath);
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
        string Opus.Core.IToolset.Version (Opus.Core.BaseTarget baseTarget)
        {
            return this.Version;
        }

        string Opus.Core.IToolset.InstallPath (Opus.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return this.installPath;
        }

        string Opus.Core.IToolset.BinPath (Opus.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return System.IO.Path.Combine(this.installPath, "bin");
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

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get
            {
                throw new System.NotImplementedException ();
            }
        }
        #endregion
    }
}

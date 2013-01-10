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
        protected System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool> toolMap = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool>();
        protected System.Collections.Generic.Dictionary<System.Type, System.Type> toolOptionsMap = new System.Collections.Generic.Dictionary<System.Type, System.Type>();
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
            return this.installPath;
        }

        Opus.Core.ITool Opus.Core.IToolset.Tool (System.Type toolType)
        {
            if (!this.toolMap.ContainsKey(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolMap[toolType];
        }

        System.Type Opus.Core.IToolset.ToolOptionType (System.Type toolType)
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

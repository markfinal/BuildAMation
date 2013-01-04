// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Toolset : Opus.Core.IToolset
    {
        protected abstract string GetInstallPath(Opus.Core.BaseTarget baseTarget);
        protected abstract string GetVersionNumber();

        protected System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool> toolMap = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool>();
        protected System.Collections.Generic.Dictionary<System.Type, System.Type> toolOptionsMap = new System.Collections.Generic.Dictionary<System.Type, System.Type>();

        public Toolset()
        {
            this.toolMap[typeof(IMocTool)] = new MocTool(this);
            this.toolOptionsMap[typeof(IMocTool)] = typeof(MocOptionCollection);
        }

        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            string installPath = this.GetInstallPath(baseTarget);
            string binPath = System.IO.Path.Combine(installPath, "bin");
            return binPath;
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.BaseTarget baseTarget)
        {
            string installPath = this.GetInstallPath(baseTarget);
            return installPath;
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return this.GetVersionNumber();
        }

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            if (!this.toolMap.ContainsKey(toolType))
            {
                throw new Opus.Core.Exception(System.String.Format("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString()), false);
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

                throw new Opus.Core.Exception(System.String.Format("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString()), false);
            }

            return this.toolOptionsMap[toolType];
        }

        #endregion
    }
}
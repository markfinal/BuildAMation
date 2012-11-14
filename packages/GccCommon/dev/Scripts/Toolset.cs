// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract class Toolset : Opus.Core.IToolset
    {
        protected string installPath;
        public Opus.Core.StringArray includePaths = new Opus.Core.StringArray();
        public string cxxIncludePath;
        protected System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool> toolMap = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool>();
        protected System.Collections.Generic.Dictionary<System.Type, System.Type> toolOptionsMap = new System.Collections.Generic.Dictionary<System.Type, System.Type>();

        protected abstract void GetInstallPath(Opus.Core.BaseTarget baseTarget);
        protected abstract string GetVersion(Opus.Core.BaseTarget baseTarget);
        public abstract string GetMachineType(Opus.Core.BaseTarget baseTarget);

        #region IToolset implementation
        string Opus.Core.IToolset.Version (Opus.Core.BaseTarget baseTarget)
        {
            return this.GetVersion(baseTarget);
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
                throw new Opus.Core.Exception(System.String.Format("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString()), false);
            }

            return this.toolMap[toolType];
        }

        System.Type Opus.Core.IToolset.ToolOptionType (System.Type toolType)
        {
            if (!this.toolOptionsMap.ContainsKey(toolType))
            {
                throw new Opus.Core.Exception(System.String.Format("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString()), false);
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

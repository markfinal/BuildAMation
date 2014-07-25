// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ClangCommon package</summary>
// <author>Mark Final</author>
namespace ClangCommon
{
    public abstract class Toolset :
        Opus.Core.IToolset
    {
        private string installPath;

        protected System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType>();
        protected abstract string SpecificInstallPath(Opus.Core.BaseTarget baseTarget);
        protected abstract string SpecificVersion(Opus.Core.BaseTarget baseTarget);

        protected
        Toolset()
        {
            this.toolConfig[typeof(C.ICompilerTool)] = new Opus.Core.ToolAndOptionType(new CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)] = new Opus.Core.ToolAndOptionType(new CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.INullOpTool)] = new Opus.Core.ToolAndOptionType(null, null);
            this.toolConfig[typeof(C.IThirdPartyTool)] = new Opus.Core.ToolAndOptionType(null, typeof(C.ThirdPartyOptionCollection));
        }

        #region IToolset Members

        string
        Opus.Core.IToolset.BinPath(
            Opus.Core.BaseTarget baseTarget)
        {
            return (this as Opus.Core.IToolset).InstallPath (baseTarget);
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string
        Opus.Core.IToolset.InstallPath(
            Opus.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return this.installPath;
            }

            string installPath = null;
            if (Opus.Core.State.HasCategory("Clang") && Opus.Core.State.Has("Clang", "InstallPath"))
            {
                installPath = Opus.Core.State.Get("Clang", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Clang install path set from command line to '{0}'", installPath);
                this.installPath = installPath;
                return installPath;
            }

            installPath = this.SpecificInstallPath(baseTarget);
            if (null != installPath)
            {
                this.installPath = installPath;
                return installPath;
            }

            throw new Opus.Core.Exception("Unable to locate clang toolchain");
        }

        bool
        Opus.Core.IToolset.HasTool(
            System.Type toolType)
        {
            return this.toolConfig.ContainsKey(toolType);
        }

        Opus.Core.ITool
        Opus.Core.IToolset.Tool(
            System.Type toolType)
        {
            if (!(this as Opus.Core.IToolset).HasTool(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].Tool;
        }

        System.Type
        Opus.Core.IToolset.ToolOptionType(
            System.Type toolType)
        {
            if (!(this as Opus.Core.IToolset).HasTool(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].OptionsType;
        }

        string
        Opus.Core.IToolset.Version(
            Opus.Core.BaseTarget baseTarget)
        {
            return this.SpecificVersion(baseTarget);
        }

        #endregion
    }
}

// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ClangCommon package</summary>
// <author>Mark Final</author>
namespace ClangCommon
{
    public abstract class Toolset :
        Bam.Core.IToolset
    {
        private string installPath;

        protected System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType>();
        protected abstract string SpecificInstallPath(Bam.Core.BaseTarget baseTarget);
        protected abstract string SpecificVersion(Bam.Core.BaseTarget baseTarget);

        protected
        Toolset()
        {
            this.toolConfig[typeof(C.ICompilerTool)] = new Bam.Core.ToolAndOptionType(new CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)] = new Bam.Core.ToolAndOptionType(new CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.INullOpTool)] = new Bam.Core.ToolAndOptionType(null, null);
            this.toolConfig[typeof(C.IThirdPartyTool)] = new Bam.Core.ToolAndOptionType(null, typeof(C.ThirdPartyOptionCollection));
        }

        #region IToolset Members

        string
        Bam.Core.IToolset.BinPath(
            Bam.Core.BaseTarget baseTarget)
        {
            return (this as Bam.Core.IToolset).InstallPath (baseTarget);
        }

        Bam.Core.StringArray Bam.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string
        Bam.Core.IToolset.InstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return this.installPath;
            }

            string installPath = null;
            if (Bam.Core.State.HasCategory("Clang") && Bam.Core.State.Has("Clang", "InstallPath"))
            {
                installPath = Bam.Core.State.Get("Clang", "InstallPath") as string;
                Bam.Core.Log.DebugMessage("Clang install path set from command line to '{0}'", installPath);
                this.installPath = installPath;
                return installPath;
            }

            installPath = this.SpecificInstallPath(baseTarget);
            if (null != installPath)
            {
                this.installPath = installPath;
                return installPath;
            }

            throw new Bam.Core.Exception("Unable to locate clang toolchain");
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

        string
        Bam.Core.IToolset.Version(
            Bam.Core.BaseTarget baseTarget)
        {
            return this.SpecificVersion(baseTarget);
        }

        #endregion
    }
}

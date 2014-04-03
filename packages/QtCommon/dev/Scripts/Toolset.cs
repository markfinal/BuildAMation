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
        protected virtual string GetBinPath(Opus.Core.BaseTarget baseTarget)
        {
            string installPath = this.GetInstallPath(baseTarget);
            if (!baseTarget.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                string binPath = System.IO.Path.Combine(installPath, "bin");
                return binPath;
            }
            else
            {
                return installPath;
            }
        }
        public virtual string GetIncludePath(Opus.Core.BaseTarget baseTarget)
        {
            if (!baseTarget.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                string installPath = this.GetInstallPath(baseTarget);
                string includePath = System.IO.Path.Combine(installPath, "include");
                return includePath;
            }
            else
            {
                // frameworks are used on OSX
                return string.Empty;
            }
        }
        public virtual string GetLibraryPath(Opus.Core.BaseTarget baseTarget)
        {
            if (!baseTarget.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                string installPath = this.GetInstallPath(baseTarget);
                string libPath = System.IO.Path.Combine(installPath, "lib");
                return libPath;
            }
            else
            {
                // frameworks are used on OSX
                return string.Empty;
            }
        }
        public virtual bool IncludePathIncludesQtModuleName
        {
            get
            {
                return false;
            }
        }

        protected System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType>();

        public Toolset()
        {
            this.toolConfig[typeof(IMocTool)] = new Opus.Core.ToolAndOptionType(new MocTool(this), typeof(MocOptionCollection));
        }

        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            return this.GetBinPath(baseTarget);
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
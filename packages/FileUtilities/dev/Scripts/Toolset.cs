// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
[assembly:Opus.Core.RegisterToolset("FileUtilities", typeof(FileUtilities.Toolset))]

namespace FileUtilities
{
    public sealed class Toolset : Opus.Core.IToolset
    {
        private System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType>();

        public Toolset()
        {
            this.toolConfig[typeof(ICopyFilesTool)] = new Opus.Core.ToolAndOptionType(new CopyFilesTool(), typeof(CopyFilesOptionCollection));
            this.toolConfig[typeof(ISymLinkTool)] = new Opus.Core.ToolAndOptionType(new SymLinkTool(), typeof(SymLinkOptionCollection));
        }

        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return "dev";
        }

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            if (!this.toolConfig.ContainsKey(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].Tool;
        }

        System.Type Opus.Core.IToolset.ToolOptionType(System.Type toolType)
        {
            if (!this.toolConfig.ContainsKey(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].OptionsType;
        }

        #endregion
    }
}
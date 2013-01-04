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
        private System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool> toolMap = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool>();
        private System.Collections.Generic.Dictionary<System.Type, System.Type> toolOptionsMap = new System.Collections.Generic.Dictionary<System.Type, System.Type>();

        public Toolset()
        {
            this.toolMap[typeof(ICopyFilesTool)] = new CopyFilesTool();
            this.toolMap[typeof(ISymLinkTool)] = new SymLinkTool();

            this.toolOptionsMap[typeof(ICopyFilesTool)] = typeof(CopyFilesOptionCollection);
            this.toolOptionsMap[typeof(ISymLinkTool)] = typeof(SymLinkOptionCollection);
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
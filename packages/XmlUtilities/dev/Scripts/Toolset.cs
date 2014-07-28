// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>

[assembly:Opus.Core.RegisterToolset("XmlUtilities", typeof(XmlUtilities.Toolset))]

namespace XmlUtilities
{
    public sealed class Toolset :
        Opus.Core.IToolset
    {
        private System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType>();

        public
        Toolset()
        {
            this.toolConfig[typeof(IXmlWriterTool)] =
                new Opus.Core.ToolAndOptionType(new XmlWriterTool(), typeof(XmlWriterOptionCollection));
            this.toolConfig[typeof(IOSXPlistWriterTool)] =
                new Opus.Core.ToolAndOptionType(new XmlWriterTool(), typeof(OSXPlistWriterOptionCollection));
            this.toolConfig[typeof(ITextFileTool)] =
                new Opus.Core.ToolAndOptionType(new TextFileTool(), typeof(TextFileOptionCollection));
        }

        #region IToolset Members

        string
        Opus.Core.IToolset.BinPath(
            Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string
        Opus.Core.IToolset.InstallPath(
            Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        string
        Opus.Core.IToolset.Version(
            Opus.Core.BaseTarget baseTarget)
        {
            return "dev";
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

        #endregion
    }
}

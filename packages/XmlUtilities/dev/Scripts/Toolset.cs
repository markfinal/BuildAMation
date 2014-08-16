// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>

[assembly:Bam.Core.RegisterToolset("XmlUtilities", typeof(XmlUtilities.Toolset))]

namespace XmlUtilities
{
    public sealed class Toolset :
        Bam.Core.IToolset
    {
        private System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType>();

        public
        Toolset()
        {
            this.toolConfig[typeof(IXmlWriterTool)] =
                new Bam.Core.ToolAndOptionType(new XmlWriterTool(), typeof(XmlWriterOptionCollection));
            this.toolConfig[typeof(IOSXPlistWriterTool)] =
                new Bam.Core.ToolAndOptionType(new XmlWriterTool(), typeof(OSXPlistWriterOptionCollection));
            this.toolConfig[typeof(ITextFileTool)] =
                new Bam.Core.ToolAndOptionType(new TextFileTool(), typeof(TextFileOptionCollection));
        }

        #region IToolset Members

        string
        Bam.Core.IToolset.BinPath(
            Bam.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Bam.Core.StringArray Bam.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string
        Bam.Core.IToolset.InstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        string
        Bam.Core.IToolset.Version(
            Bam.Core.BaseTarget baseTarget)
        {
            return "dev";
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

        #endregion
    }
}

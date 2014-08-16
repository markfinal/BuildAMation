// <copyright file="ToolAndOptionType.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public sealed class ToolAndOptionType
    {
        public
        ToolAndOptionType(
            ITool tool,
            System.Type optionsType)
        {
            this.Tool = tool;
            this.OptionsType = optionsType;
        }

        public ITool Tool
        {
            get;
            private set;
        }

        public System.Type OptionsType
        {
            get;
            private set;
        }
    }
}

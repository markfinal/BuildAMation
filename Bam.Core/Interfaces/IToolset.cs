// <copyright file="IToolset.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface IToolset
    {
        string
        Version(
            BaseTarget baseTarget);

        string
        InstallPath(
            BaseTarget baseTarget);

        string
        BinPath(
            BaseTarget baseTarget);

        StringArray Environment
        {
            get;
        }

        bool
        HasTool(
            System.Type toolType);

        ITool
        Tool(
            System.Type toolType);

        System.Type
        ToolOptionType(
            System.Type toolType);
    }
}

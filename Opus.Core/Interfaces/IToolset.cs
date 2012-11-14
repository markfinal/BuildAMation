// <copyright file="IToolset.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IToolset
    {
        string Version(Opus.Core.BaseTarget baseTarget);

        string InstallPath(Opus.Core.BaseTarget baseTarget);

        string BinPath(Opus.Core.BaseTarget baseTarget);
        
        Opus.Core.StringArray Environment
        {
            get;
        }

        ITool Tool(System.Type toolType);

        System.Type ToolOptionType(System.Type toolType);
    }
}
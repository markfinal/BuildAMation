// <copyright file="IWinResourceCompilerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalWin32ResourceCompilerOptionsDelegateAttribute),
                                   typeof(ExportWin32ResourceCompilerOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetWinResourceCompilerToolset")]
    public interface IWinResourceCompilerTool :
        Opus.Core.ITool
    {
        string CompiledResourceSuffix
        {
            get;
        }

        string InputFileSwitch
        {
            get;
        }

        string OutputFileSwitch
        {
            get;
        }
    }
}

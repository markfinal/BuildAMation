// <copyright file="IWinResourceCompilerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalWin32ResourceCompilerOptionsDelegateAttribute),
                                   typeof(ExportWin32ResourceCompilerOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetWinResourceCompilerToolset")]
    public interface IWinResourceCompilerTool :
        Bam.Core.ITool
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

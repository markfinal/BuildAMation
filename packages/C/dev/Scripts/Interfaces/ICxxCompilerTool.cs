// <copyright file="ICxxCompilerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalCompilerOptionsDelegateAttribute),
                                   typeof(ExportCompilerOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetCxxCompilerToolset")]
    public interface ICxxCompilerTool :
        ICompilerTool
    {}
}

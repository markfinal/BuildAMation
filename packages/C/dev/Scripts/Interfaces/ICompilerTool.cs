// <copyright file="ICompilerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalCompilerOptionsDelegateAttribute),
                                   typeof(ExportCompilerOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetCCompilerToolset")]
    public interface ICompilerTool :
        Bam.Core.ITool
    {
        string PreprocessedOutputSuffix
        {
            get;
        }

        string ObjectFileSuffix
        {
            get;
        }

        string ObjectFileOutputSubDirectory
        {
            get;
        }

        Bam.Core.StringArray
        IncludePaths(
            Bam.Core.BaseTarget baseTarget);

        Bam.Core.StringArray IncludePathCompilerSwitches
        {
            get;
        }
    }
}

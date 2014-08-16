// <copyright file="IObjCxxCompilerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalCompilerOptionsDelegateAttribute),
                                   typeof(ExportCompilerOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetObjCxxCompilerToolset")]
    public interface IObjCxxCompilerTool :
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

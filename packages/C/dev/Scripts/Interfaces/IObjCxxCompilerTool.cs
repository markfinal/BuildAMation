// <copyright file="IObjCxxCompilerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalCompilerOptionsDelegateAttribute),
                                   typeof(ExportCompilerOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetObjCxxCompilerToolset")]
    public interface IObjCxxCompilerTool :
        Opus.Core.ITool
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

        Opus.Core.StringArray
        IncludePaths(
            Opus.Core.BaseTarget baseTarget);

        Opus.Core.StringArray IncludePathCompilerSwitches
        {
            get;
        }
    }
}

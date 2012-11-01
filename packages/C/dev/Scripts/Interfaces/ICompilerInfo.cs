// <copyright file="ICompilerInfo.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Opus.Core.LocalAndExportTypesAttribute(typeof(LocalCompilerOptionsDelegateAttribute),
                                            typeof(ExportCompilerOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetCCompilerToolset")]
    public interface ICompilerTool : Opus.Core.ITool
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

        // TODO: change this to BaseTarget
        Opus.Core.StringArray IncludePaths(Opus.Core.Target target);

        Opus.Core.StringArray IncludePathCompilerSwitches
        {
            get;
        }
    }
}
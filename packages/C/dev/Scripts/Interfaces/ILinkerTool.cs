// <copyright file="ILinkerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalLinkerOptionsDelegateAttribute),
                                   typeof(ExportLinkerOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetLinkerToolset")]
    public interface ILinkerTool : Opus.Core.ITool
    {
        string ExecutableSuffix
        {
            get;
        }

        string MapFileSuffix
        {
            get;
        }

        string StartLibraryList
        {
            get;
        }

        string EndLibraryList
        {
            get;
        }

        string DynamicLibraryPrefix
        {
            get;
        }

        string DynamicLibrarySuffix
        {
            get;
        }

        string BinaryOutputSubDirectory
        {
            get;
        }

        Opus.Core.StringArray LibPaths(Opus.Core.BaseTarget baseTarget);
    }
}
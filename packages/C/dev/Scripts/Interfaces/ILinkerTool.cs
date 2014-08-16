// <copyright file="ILinkerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalLinkerOptionsDelegateAttribute),
                                   typeof(ExportLinkerOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetLinkerToolset")]
    public interface ILinkerTool :
        Bam.Core.ITool
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

        Bam.Core.StringArray
        LibPaths(
            Bam.Core.BaseTarget baseTarget);
    }
}

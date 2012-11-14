// <copyright file="ILinkerTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    // TODO: want to split this out into regular linker stuff, and Windows import library interface
    [Opus.Core.LocalAndExportTypesAttribute(typeof(LocalLinkerOptionsDelegateAttribute),
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

        string ImportLibraryPrefix
        {
            get;
        }

        string ImportLibrarySuffix
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

        string ImportLibrarySubDirectory
        {
            get;
        }

        string BinaryOutputSubDirectory
        {
            get;
        }

        Opus.Core.StringArray LibPaths(Opus.Core.Target target);
    }
}
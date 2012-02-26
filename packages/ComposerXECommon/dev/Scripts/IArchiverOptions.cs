// <copyright file="IArchiverOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    public interface IArchiverOptions
    {
        ComposerXECommon.EArchiverCommand Command
        {
            get;
            set;
        }

        bool DoNotWarnIfLibraryCreated
        {
            get;
            set;
        }
    }
}
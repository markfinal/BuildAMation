// <copyright file="PosixSharedLibrarySymlinksOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    // TODO: this does not implement any options interface
    public sealed partial class PosixSharedLibrarySymlinksOptionCollection :
        C.PosixSharedLibrarySymlinksOptionCollection
    {
        public
        PosixSharedLibrarySymlinksOptionCollection(
            Opus.Core.DependencyNode node) : base(node)
        {}
    }
}

// <copyright file="IModuleCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface IModuleCollection :
        INestedDependents
    {
        void
        RegisterUpdateOptions(
            UpdateOptionCollectionDelegateArray delegateArray,
            Location root,
            string pattern);

        void
        RegisterUpdateOptions(
            UpdateOptionCollectionDelegateArray delegateArray,
            Location root,
            string pattern,
            Opus.Core.Location.EExists exists);
    }
}

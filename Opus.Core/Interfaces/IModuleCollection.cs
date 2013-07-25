// <copyright file="IModuleCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IModuleCollection : INestedDependents
    {
#if true
        void RegisterUpdateOptions(UpdateOptionCollectionDelegateArray delegateArray, Location root, params string[] pathSegments);
#else
        IModule GetChildModule(object owner, params string[] pathSegments);
#endif
    }
}

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
        // TODO: consider moving this to BaseModule, because it should be applicable to ANY module
        void RegisterUpdateOptions(UpdateOptionCollectionDelegateArray delegateArray, Location root, params string[] pathSegments);
#else
        IModule GetChildModule(object owner, params string[] pathSegments);
#endif
    }
}

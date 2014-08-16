// <copyright file="IInjectModules.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IInjectModules
    {
        System.Type
        GetInjectedModuleType(
            BaseTarget baseTarget);

        string
        GetInjectedModuleNameSuffix(
            BaseTarget baseTarget);

        void
        ModuleCreationFixup(
            DependencyNode node);

        DependencyNode
        GetInjectedParentNode(
            DependencyNode node);
    }
}

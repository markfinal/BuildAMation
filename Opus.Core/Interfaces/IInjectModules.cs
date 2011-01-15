// <copyright file="IInjectModules.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IInjectModules
    {
        ModuleCollection GetInjectedModules(Opus.Core.Target target);
    }
}
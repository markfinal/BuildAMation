// <copyright file="ModuleTargetsAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=false)]
    public sealed class ModuleTargetsAttribute :
        BaseTargetFilteredAttribute
    {}
}

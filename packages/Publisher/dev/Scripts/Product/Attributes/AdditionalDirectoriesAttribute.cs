// <copyright file="AdditionalDirectoriesAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class AdditionalDirectoriesAttribute : Opus.Core.BaseTargetFilteredAttribute
    {
    }
}

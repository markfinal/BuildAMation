// <copyright file="EProjectConfigurationType.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public enum EProjectConfigurationType
    {
        Undefined = 0,
        Application = 1,
        DynamicLibrary = 2,
        StaticLibrary = 4,
        Utility = 10
    }
}
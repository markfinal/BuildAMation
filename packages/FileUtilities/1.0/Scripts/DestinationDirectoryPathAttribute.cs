// <copyright file="DestinationDirectoryPathAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class DestinationDirectoryPathAttribute : System.Attribute
    {
        public DestinationDirectoryPathAttribute()
        {
        }
    }
}
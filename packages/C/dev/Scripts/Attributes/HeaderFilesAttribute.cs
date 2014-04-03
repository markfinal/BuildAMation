// <copyright file="HeaderFilesAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple=true)]
    public sealed class HeaderFilesAttribute : System.Attribute
    {
        public HeaderFilesAttribute()
        {
        }
    }
}
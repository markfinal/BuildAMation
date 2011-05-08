// <copyright file="PagesAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public sealed class PagesAttribute : System.Attribute
    {
        public PagesAttribute()
        {
        }
    }
}
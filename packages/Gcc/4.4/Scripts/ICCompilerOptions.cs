// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public interface ICCompilerOptions
    {
        Gcc.EVisibility Visibility
        {
            get;
            set;
        }
    }
}

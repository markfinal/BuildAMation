// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public interface ICCompilerOptions
    {
        Mingw.EVisibility Visibility
        {
            get;
            set;
        }
    }
}
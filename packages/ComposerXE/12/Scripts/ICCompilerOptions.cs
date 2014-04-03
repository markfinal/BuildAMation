// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXE package</summary>
// <author>Mark Final</author>
namespace ComposerXE
{
    public interface ICCompilerOptions
    {
        ComposerXE.EVisibility Visibility
        {
            get;
            set;
        }
    }
}
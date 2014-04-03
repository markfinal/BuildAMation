// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>LLVMGcc package</summary>
// <author>Mark Final</author>
namespace LLVMGcc
{
    public interface ICCompilerOptions
    {
        LLVMGcc.EVisibility Visibility
        {
            get;
            set;
        }
    }
}
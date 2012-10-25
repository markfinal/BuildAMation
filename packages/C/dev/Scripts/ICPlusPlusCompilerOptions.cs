// <copyright file="ICPlusPlusCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ICPlusPlusCompilerOptions
    {
        /// <summary>
        /// Specify the type of exception handling used by the compiler
        /// </summary>
        C.CPlusPlus.EExceptionHandler ExceptionHandler
        {
            get;
            set;
        }
    }
}
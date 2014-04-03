// <copyright file="ICxxCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ICxxCompilerOptions
    {
        /// <summary>
        /// Specify the type of exception handling used by the compiler
        /// </summary>
        C.Cxx.EExceptionHandler ExceptionHandler
        {
            get;
            set;
        }
    }
}
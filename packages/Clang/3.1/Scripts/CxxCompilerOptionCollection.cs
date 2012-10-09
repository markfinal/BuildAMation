// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class CxxCompilerOptionCollection : CCompilerOptionCollection, C.ICPlusPlusCompilerOptions
    {
        #region ICPlusPlusCompilerOptions Members

        C.CPlusPlus.EExceptionHandler C.ICPlusPlusCompilerOptions.ExceptionHandler
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        #endregion
    }
}

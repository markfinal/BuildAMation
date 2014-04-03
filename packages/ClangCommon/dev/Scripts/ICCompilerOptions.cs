// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ClangCommon package</summary>
// <author>Mark Final</author>
namespace ClangCommon
{
    public interface ICCompilerOptions
    {
        bool PositionIndependentCode
        {
            get;
            set;
        }

        bool SixtyFourBit
        {
            get;
            set;
        }
    }
}

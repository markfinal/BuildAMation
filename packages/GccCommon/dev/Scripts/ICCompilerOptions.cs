// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public interface ICCompilerOptions
    {
        bool AllWarnings
        {
            get;
            set;
        }

        bool ExtraWarnings
        {
            get;
            set;
        }

        bool StrictAliasing
        {
            get;
            set;
        }

        bool PositionIndependentCode
        {
            get;
            set;
        }

        bool InlineFunctions
        {
            get;
            set;
        }

        bool Pedantic
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

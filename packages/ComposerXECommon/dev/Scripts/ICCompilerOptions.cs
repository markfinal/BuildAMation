// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    public interface ICCompilerOptions
    {
        bool AllWarnings
        {
            get;
            set;
        }

        bool StrictDiagnostics
        {
            get;
            set;
        }

        bool EnableRemarks
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

        bool SixtyFourBit
        {
            get;
            set;
        }
    }
}
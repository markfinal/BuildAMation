// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>IntelCommon package</summary>
// <author>Mark Final</author>
namespace IntelCommon
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
    }
}
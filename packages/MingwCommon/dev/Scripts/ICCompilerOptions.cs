// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
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

        bool InlineFunctions
        {
            get;
            set;
        }
    }
}
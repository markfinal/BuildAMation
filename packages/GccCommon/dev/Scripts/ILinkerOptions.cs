// <copyright file="ILinkerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public interface ILinkerOptions
    {
        bool CanUseOrigin
        {
            get;
            set;
        }

        bool AllowUndefinedSymbols
        {
            get;
            set;
        }

        Bam.Core.StringArray RPath
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

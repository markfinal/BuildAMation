// <copyright file="ILinkerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>IntelCommon package</summary>
// <author>Mark Final</author>
namespace IntelCommon
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

        Opus.Core.StringArray RPath
        {
            get;
            set;
        }
    }
}
// <copyright file="ILinkerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public interface ILinkerOptions
    {
        bool NoLogo
        {
            get;
            set;
        }

        string StackReserveAndCommit
        {
            get;
            set;
        }

        Opus.Core.StringArray IgnoredLibraries
        {
            get;
            set;
        }

        bool IncrementalLink
        {
            get;
            set;
        }
    }
}

// <copyright file="IToolchainOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public interface IToolchainOptions
    {
        VisualCCommon.ERuntimeLibrary RuntimeLibrary
        {
            get;
            set;
        }
    }
}
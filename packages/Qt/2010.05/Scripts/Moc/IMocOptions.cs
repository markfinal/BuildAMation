// <copyright file="IMocOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public interface IMocOptions
    {
        string MocOutputPath
        {
            get;
            set;
        }

        Opus.Core.DirectoryCollection IncludePaths
        {
            get;
            set;
        }
    }
}
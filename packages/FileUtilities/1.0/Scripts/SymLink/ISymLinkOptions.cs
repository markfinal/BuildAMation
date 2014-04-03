// <copyright file="ISymLinkOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public interface ISymLinkOptions
    {
        string LinkDirectory
        {
            get;
            set;
        }

        string LinkName
        {
            get;
            set;
        }

        EType Type
        {
            get;
            set;
        }
    }
}
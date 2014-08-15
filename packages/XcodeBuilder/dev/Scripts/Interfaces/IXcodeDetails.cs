// <copyright file="IXcodeDetails.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public interface IXcodeDetails
    {
        EXcodeVersion SupportedVersion
        {
            get;
        }
    }
}

// <copyright file="Delegate.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeProjectProcessor package</summary>
// <author>Mark Final</author>
namespace XcodeProjectProcessor
{
    public delegate void Delegate(object sender, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target);
}

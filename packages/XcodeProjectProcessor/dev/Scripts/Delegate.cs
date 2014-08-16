// <copyright file="Delegate.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeProjectProcessor package</summary>
// <author>Mark Final</author>
namespace XcodeProjectProcessor
{
    public delegate void
    Delegate(
        object sender,
        XcodeBuilder.PBXProject project,
        XcodeBuilder.XcodeNodeData currentObject,
        XcodeBuilder.XCBuildConfiguration configuration,
        Bam.Core.Option option,
        Bam.Core.Target target);
}

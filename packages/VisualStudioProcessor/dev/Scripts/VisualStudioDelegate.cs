// <copyright file="VisualStudioDelegate.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualStudioProcessor package</summary>
// <author>Mark Final</author>
namespace VisualStudioProcessor
{
    public delegate ToolAttributeDictionary
    Delegate(
        object sender,
        Bam.Core.Option option,
        Bam.Core.Target target,
        VisualStudioProcessor.EVisualStudioTarget vsTarget);
}

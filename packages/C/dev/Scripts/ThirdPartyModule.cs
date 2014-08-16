// <copyright file="ThirdPartyModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C third party library (externally built libraries)
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(IThirdPartyTool))]
    public abstract class ThirdPartyModule :
        Bam.Core.BaseModule
    {
        public virtual void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {}
    }
}

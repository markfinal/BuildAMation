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
    [Opus.Core.ModuleToolAssignment(typeof(IThirdPartyTool))]
    public abstract class ThirdPartyModule :
        Opus.Core.BaseModule
    {
        public virtual void
        RegisterOutputFiles(
            Opus.Core.BaseOptionCollection options,
            Opus.Core.Target target,
            string modulePath)
        {}
    }
}

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
    [Opus.Core.ModuleToolAssignment(null)]
    public abstract class ThirdPartyModule : Opus.Core.BaseModule
    {
        public override Opus.Core.BaseOptionCollection Options
        {
            get
            {
                return null;
            }
            set
            {
                // do nothing
            }
        }

        public abstract Opus.Core.StringArray Libraries(Opus.Core.Target target);
    }
}
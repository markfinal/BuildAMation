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
    [Opus.Core.AssignToolForModule]
    public abstract class ThirdPartyModule : Opus.Core.IModule
    {
        public void ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        public Opus.Core.BaseOptionCollection Options
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

        public Opus.Core.DependencyNode OwningNode
        {
            get;
            set;
        }

        public abstract Opus.Core.StringArray Libraries(Opus.Core.Target target);

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;
    }
}
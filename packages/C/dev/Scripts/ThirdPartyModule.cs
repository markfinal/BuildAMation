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
    public abstract class ThirdPartyModule : Opus.Core.IModule
    {
        void Opus.Core.IModule.ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        Opus.Core.BaseOptionCollection Opus.Core.IModule.Options
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

        Opus.Core.DependencyNode Opus.Core.IModule.OwningNode
        {
            get;
            set;
        }

        public Opus.Core.ProxyModulePath ProxyPath
        {
            get;
            set;
        }

        public abstract Opus.Core.StringArray Libraries(Opus.Core.Target target);

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;
    }
}
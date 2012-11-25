// <copyright file="BaseModule.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public abstract class BaseModule : IModule
    {
        public event UpdateOptionCollectionDelegate UpdateOptions;

        public virtual BaseOptionCollection Options
        {
            get;
            set;
        }

        ProxyModulePath IModule.ProxyPath
        {
            get;
            set;
        }

        public void ExecuteOptionUpdate(Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this as IModule, target);
            }
        }

        public DependencyNode OwningNode
        {
            get;
            set;
        }
    }
}
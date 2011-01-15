// <copyright file="Assembly.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    public abstract class Assembly : Opus.Core.IModule
    {
        public void ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        public Opus.Core.ModuleCollection GetNestedDependents(Opus.Core.Target target)
        {
            return null;
        }

        public Opus.Core.BaseOptionCollection Options
        {
            get;
            set;
        }

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;
    }
}
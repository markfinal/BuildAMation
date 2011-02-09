// <copyright file="ObjectFileCollectionBase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class ObjectFileCollectionBase : Opus.Core.IModuleCollection
    {
        protected System.Collections.Generic.List<ObjectFile> list = new System.Collections.Generic.List<ObjectFile>();

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        public Opus.Core.BaseOptionCollection Options
        {
            get;
            set;
        }

        public Opus.Core.DependencyNode OwningNode
        {
            get;
            set;
        }

        public void ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (this.UpdateOptions != null)
            {
                this.UpdateOptions(this, target);
            }
        }

        public Opus.Core.ModuleCollection GetNestedDependents(Opus.Core.Target target)
        {
            Opus.Core.ModuleCollection collection = new Opus.Core.ModuleCollection();

            foreach (ObjectFile objectFile in this.list)
            {
                collection.Add(objectFile as Opus.Core.IModule);
            }

            return collection;
        }
    }
}
// <copyright file="BaseModule.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    /// <summary>
    /// BaseModules are the base class for all real modules in package scripts.
    /// These are constructed by the Opus Core when they are required.
    /// Nested modules that appear as fields are either constructed automatically by
    /// the default constructor of their parent, or in the custom construct required to be
    /// written by the package author. As such, there must always be a default constructor
    /// in BaseModule.
    /// </summary>
    public abstract class BaseModule : IModule
    {
        public event UpdateOptionCollectionDelegate UpdateOptions;

        public virtual BaseOptionCollection Options
        {
            get;
            set;
        }

        public ProxyModulePath ProxyPath
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

        private DependencyNode owningNode = null;
        public DependencyNode OwningNode
        {
            get
            {
                return this.owningNode;
            }

            set
            {
                if (null != this.owningNode)
                {
                    throw new Exception("Module {0} cannot have it's node reassigned to {1}", this.owningNode.UniqueModuleName, value.UniqueModuleName);
                }

                this.owningNode = value;
            }
        }

        private LocationMap locationMap = new LocationMap();
        public LocationMap Locations
        {
            get
            {
                return this.locationMap;
            }
        }

        public Location MakeLocationDirectory(string name, params string[] segments)
        {
            if (this.locationMap.Contains(name))
            {
                throw new Exception("Location '{0}' already exists for module '{1}'", name, this.OwningNode.UniqueModuleName);
            }

            var location = new Opus.Core.LocationDirectory(this, segments);
            this.locationMap[name] = location;
            return location;
        }
    }
}
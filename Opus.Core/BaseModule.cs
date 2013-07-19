// <copyright file="BaseModule.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public abstract class BaseModule : IModule
    {
        // TOOD: would like to be able to do this, but the point at which modules and nodes are
        // created is ill-defined
#if false
        public BaseModule(Opus.Core.DependencyNode owningNode)
        {
            this.OwningNode = owningNode;
            var target = owningNode.Target;

            var package = this.OwningNode.Package;

            this.Locations["TargetDir"] = new Opus.Core.LocationDirectory(package.BuildDirectoryLocation, TargetUtilities.DirectoryName(target));
        }
#endif

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

        public DependencyNode OwningNode
        {
            get;
            // TODO: would like to make this private since it should be readonly
            set;
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
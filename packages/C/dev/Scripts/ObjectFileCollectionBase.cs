// <copyright file="ObjectFileCollectionBase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class ObjectFileCollectionBase :
        Bam.Core.BaseModule,
        Bam.Core.IModuleCollection
    {
        protected Bam.Core.Array<ObjectFile> list = new Bam.Core.Array<ObjectFile>();

        protected Bam.Core.LocationArray Includes
        {
            get;
            set;
        }

        protected Bam.Core.LocationArray Excludes
        {
            get;
            set;
        }

        public void
        Include(
            Bam.Core.Location baseLocation,
            string pattern)
        {
            if (null == this.Includes)
            {
                this.Includes = new Bam.Core.LocationArray();
            }
            this.Includes.Add(new Bam.Core.ScaffoldLocation(baseLocation, pattern, Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.Exists));
        }

        public void
        Exclude(
            Bam.Core.Location baseLocation,
            string pattern)
        {
            if (null == this.Excludes)
            {
                this.Excludes = new Bam.Core.LocationArray();
            }
            this.Excludes.Add(new Bam.Core.ScaffoldLocation(baseLocation, pattern, Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.Exists));
        }

        private Bam.Core.LocationArray
        EvaluatePaths()
        {
            if (null == this.Includes)
            {
                return null;
            }

            var includePathList = new Bam.Core.LocationArray();
            foreach (var include in this.Includes)
            {
                includePathList.AddRangeUnique(include.GetLocations());
            }
            if (null == this.Excludes)
            {
                return includePathList;
            }

            var excludePathList = new Bam.Core.LocationArray();
            foreach (var exclude in this.Excludes)
            {
                excludePathList.AddRangeUnique(exclude.GetLocations());
            }

            // TODO: is there a better way to handle this? an 'as' cast results in null
            var rawComplement = includePathList.Complement(excludePathList);
            var complement = new Bam.Core.LocationArray(rawComplement);
            return complement;
        }

        protected virtual System.Collections.Generic.List<Bam.Core.IModule>
        MakeChildModules(
            Bam.Core.LocationArray locationList)
        {
            throw new Bam.Core.Exception("Derived classes should implement this function");
        }

        Bam.Core.ModuleCollection
        Bam.Core.INestedDependents.GetNestedDependents(
            Bam.Core.Target target)
        {
            var collection = new Bam.Core.ModuleCollection();

            // add in modules that were inserted other than by an Include() call
            foreach (var module in this.list)
            {
                collection.Add(module);
            }

            var locationList = this.EvaluatePaths();
            if (null == locationList)
            {
                return collection;
            }

            var childModules = this.MakeChildModules(locationList);
            if (null != this.DeferredUpdates)
            {
                foreach (var objectFile in childModules)
                {
                    var objectFileDeferredLocation = (objectFile as ObjectFile).SourceFileLocation;
                    if (this.DeferredUpdates.ContainsKey(objectFileDeferredLocation))
                    {
                        foreach (var updateDelegate in this.DeferredUpdates[objectFileDeferredLocation])
                        {
                            objectFile.UpdateOptions += updateDelegate;
                        }
                    }

                    collection.Add(objectFile);
                }
            }
            else
            {
                foreach (var objectFile in childModules)
                {
                    collection.Add(objectFile as Bam.Core.IModule);
                }
            }
            return collection;
        }

        private System.Collections.Generic.Dictionary<Bam.Core.Location, Bam.Core.UpdateOptionCollectionDelegateArray> DeferredUpdates
        {
            get;
            set;
        }

        public void
        RegisterUpdateOptions(
            Bam.Core.UpdateOptionCollectionDelegateArray delegateArray,
            Bam.Core.Location baseLocation,
            string pattern)
        {
            this.RegisterUpdateOptions(delegateArray, baseLocation, pattern, Bam.Core.Location.EExists.Exists);
        }

        public void
        RegisterUpdateOptions(
            Bam.Core.UpdateOptionCollectionDelegateArray delegateArray,
            Bam.Core.Location baseLocation,
            string pattern,
            Bam.Core.Location.EExists exists)
        {
            if (null == this.DeferredUpdates)
            {
                this.DeferredUpdates = new System.Collections.Generic.Dictionary<Bam.Core.Location, Bam.Core.UpdateOptionCollectionDelegateArray>(new Bam.Core.LocationComparer());
            }

            var matchingLocation = new Bam.Core.ScaffoldLocation(baseLocation, pattern, Bam.Core.ScaffoldLocation.ETypeHint.File, exists);
            if (!this.DeferredUpdates.ContainsKey(matchingLocation))
            {
                this.DeferredUpdates[matchingLocation] = new Bam.Core.UpdateOptionCollectionDelegateArray();
            }
            this.DeferredUpdates[matchingLocation].AddRangeUnique(delegateArray);
        }
    }
}

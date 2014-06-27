// <copyright file="MocFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    /// <summary>
    /// Create meta data from a collection of C++ header or source files
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(IMocTool))]
    public abstract class MocFileCollection :
        Opus.Core.BaseModule,
        Opus.Core.IModuleCollection,
    Opus.Core.ICommonOptionCollection
    {
        private Opus.Core.Array<MocFile> list = new Opus.Core.Array<MocFile>();

        protected Opus.Core.LocationArray Includes
        {
            get;
            set;
        }

        protected Opus.Core.LocationArray Excludes
        {
            get;
            set;
        }

        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            if (null == this.Includes)
            {
                this.Includes = new Opus.Core.LocationArray();
            }
            this.Includes.Add(new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File));
        }

        public void Exclude(Opus.Core.Location baseLocation, string pattern)
        {
            if (null == this.Excludes)
            {
                this.Excludes = new Opus.Core.LocationArray();
            }
            this.Excludes.Add(new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File));
        }

        private Opus.Core.LocationArray EvaluatePaths()
        {
            if (null == this.Includes)
            {
                return null;
            }

            var includePathList = new Opus.Core.LocationArray();
            foreach (var include in this.Includes)
            {
                includePathList.AddRangeUnique(include.GetLocations());
            }
            if (null == this.Excludes)
            {
                return includePathList;
            }

            var excludePathList = new Opus.Core.LocationArray();
            foreach (var exclude in this.Excludes)
            {
                excludePathList.AddRangeUnique(exclude.GetLocations());
            }

            // TODO: is there a better way to handle this? an 'as' cast results in null
            var rawComplement = includePathList.Complement(excludePathList);
            var complement = new Opus.Core.LocationArray(rawComplement);
            return complement;
        }

        private System.Collections.Generic.List<Opus.Core.IModule> MakeChildModules(Opus.Core.LocationArray locationList)
        {
            var moduleCollection = new System.Collections.Generic.List<Opus.Core.IModule>();
            foreach (var location in locationList)
            {
                var mocFile = new MocFile();
                mocFile.SourceFileLocation = location;
                moduleCollection.Add(mocFile);
            }
            return moduleCollection;
        }

        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            var collection = new Opus.Core.ModuleCollection();

            // add in modules obtained through mechanisms other than paths
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
                    var location = (objectFile as MocFile).SourceFileLocation;
                    if (this.DeferredUpdates.ContainsKey(location))
                    {
                        foreach (var updateDelegate in this.DeferredUpdates[location])
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
                    collection.Add(objectFile as Opus.Core.IModule);
                }
            }
            return collection;
        }

        #region IModuleCollection Members

        private System.Collections.Generic.Dictionary<Opus.Core.Location, Opus.Core.UpdateOptionCollectionDelegateArray> DeferredUpdates
        {
            get;
            set;
        }

        public void RegisterUpdateOptions(Opus.Core.UpdateOptionCollectionDelegateArray delegateArray,
                                          Opus.Core.Location baseLocation,
                                          string pattern)
        {
            if (null == this.DeferredUpdates)
            {
                this.DeferredUpdates = new System.Collections.Generic.Dictionary<Opus.Core.Location, Opus.Core.UpdateOptionCollectionDelegateArray>(new Opus.Core.LocationComparer());
            }

            this.DeferredUpdates[new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File)] = delegateArray;
        }

        #endregion

        #region ICommonOptionCollection implementation

        Opus.Core.BaseOptionCollection Opus.Core.ICommonOptionCollection.CommonOptionCollection
        {
            get;
            set;
        }

        #endregion
    }
}
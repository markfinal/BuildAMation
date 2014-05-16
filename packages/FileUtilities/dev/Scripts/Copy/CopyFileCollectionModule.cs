// <copyright file="CopyFileCollectionModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [Opus.Core.ModuleToolAssignment(typeof(ICopyFileTool))]
    public class CopyFileCollection : Opus.Core.BaseModule, Opus.Core.IModuleCollection, Opus.Core.IIdentifyExternalDependencies
    {
        private System.Collections.Generic.List<Opus.Core.IModule> copyFiles = new System.Collections.Generic.List<Opus.Core.IModule>();

        protected Opus.Core.Array<Opus.Core.Location> Includes
        {
            get;
            set;
        }

        protected Opus.Core.Array<Opus.Core.Location> Excludes
        {
            get;
            set;
        }

        public void Include(Opus.Core.Target target, Opus.Core.LocationKey outputLocationKey, params System.Type[] moduleTypes)
        {
            // each file to copy needs to know where the parent was set to copy next to
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            Utilities.GetBesideModule(this, target, out besideModule, out dependentModule);

            foreach (var moduleType in moduleTypes)
            {
                CopyFile file = new CopyFile(besideModule, dependentModule);
                file.Set(moduleType, outputLocationKey);
                this.copyFiles.Add(file);
            }
        }

        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            if (null == this.Includes)
            {
                this.Includes = new Opus.Core.Array<Opus.Core.Location>();
            }

            this.Includes.Add(new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File));
        }

        public void Exclude(Opus.Core.Location baseLocation, string pattern)
        {
            if (null == this.Excludes)
            {
                this.Excludes = new Opus.Core.Array<Opus.Core.Location>();
            }

            this.Excludes.Add(new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.File));
        }

        private Opus.Core.Array<Opus.Core.Location> EvaluatePaths()
        {
            if (null == this.Includes)
            {
                return null;
            }

            var includePathList = new Opus.Core.Array<Opus.Core.Location>();
            foreach (var include in this.Includes)
            {
                includePathList.AddRangeUnique(include.GetLocations());
            }
            if (null == this.Excludes)
            {
                return includePathList;
            }

            var excludePathList = new Opus.Core.Array<Opus.Core.Location>();
            foreach (var exclude in this.Excludes)
            {
                excludePathList.AddRangeUnique(exclude.GetLocations());
            }

            var complement = includePathList.Complement(excludePathList);
            return complement;
        }

        private System.Collections.Generic.List<Opus.Core.IModule> MakeChildModules(Opus.Core.Array<Opus.Core.Location> locationList)
        {
            var moduleCollection = new System.Collections.Generic.List<Opus.Core.IModule>();
            foreach (var location in locationList)
            {
                var copyFile = new CopyFile();
                copyFile.SourceFileLocation = location;
                moduleCollection.Add(copyFile);
            }
            return moduleCollection;
        }

        #region IModuleCollection implementation

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

        #region INestedDependents implementation

        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            var collection = new Opus.Core.ModuleCollection();

            // add in modules that were inserted other than by an Include() call
            foreach (var module in this.copyFiles)
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
                    var objectFileDeferredLocation = (objectFile as CopyFile).SourceFileLocation;
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
                    collection.Add(objectFile as Opus.Core.IModule);
                }
            }
            return collection;
        }

        #endregion

        #region IIdentifyExternalDependencies implementation

        Opus.Core.TypeArray Opus.Core.IIdentifyExternalDependencies.IdentifyExternalDependencies(Opus.Core.Target target)
        {
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            Utilities.GetBesideModule(this, target, out besideModule, out dependentModule);
            if (null == besideModule)
            {
                return null;
            }

            // each nested file needs to know where it is being copied to
            foreach (CopyFile file in this.copyFiles)
            {
                file.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target delegateTarget) {
                    var options = module.Options as ICopyFileOptions;
                    options.DestinationModuleType = dependentModule;
                    options.DestinationModuleOutputLocation = besideModule.OutputFileLocation;
                };
            }

            return new Opus.Core.TypeArray(dependentModule);
        }

        #endregion
    }
}

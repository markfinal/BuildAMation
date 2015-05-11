#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace C
{
namespace V2
{
    public abstract class BaseObjectFiles<ChildModuleType> :
        Bam.Core.V2.Module,
        Bam.Core.V2.IModuleGroup
        where ChildModuleType : Bam.Core.V2.Module, Bam.Core.V2.IInputPath, Bam.Core.V2.IChildModule, new()
    {
        private System.Collections.Generic.List<ChildModuleType> children = new System.Collections.Generic.List<ChildModuleType>();

        public ChildModuleType AddFile(string path)
        {
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.V2.Module.Create<ChildModuleType>();
            child.InputPath = Bam.Core.V2.TokenizedString.Create(path, this);
            (child as Bam.Core.V2.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        public ChildModuleType AddFile(Bam.Core.V2.FileKey generatedFileKey, Bam.Core.V2.Module module)
        {
            if (!module.GeneratedPaths.ContainsKey(generatedFileKey))
            {
                throw new System.Exception(System.String.Format("No generated path found with key '{0}'", generatedFileKey.Id));
            }
            var child = Bam.Core.V2.Module.Create<ChildModuleType>();
            child.InputPath = module.GeneratedPaths[generatedFileKey];
            (child as Bam.Core.V2.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            child.DependsOn(module);
            return child;
        }

        protected override void ExecuteInternal()
        {
            // do nothing
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
            // TODO: might have to get the policy, for the sharing settings
        }

        public override void Evaluate()
        {
            foreach (var child in this.children)
            {
                if (!child.IsUpToDate)
                {
                    return;
                }
            }
            this.IsUpToDate = true;
        }
    }
}
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

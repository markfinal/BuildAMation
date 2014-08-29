#region License
// Copyright 2010-2014 Mark Final
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
namespace QtCommon
{
    /// <summary>
    /// Create meta data from a collection of C++ header or source files
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(IMocTool))]
    public abstract class MocFileCollection :
        Bam.Core.BaseModule,
        Bam.Core.IModuleCollection,
        Bam.Core.ICommonOptionCollection
    {
        private Bam.Core.Array<MocFile> list = new Bam.Core.Array<MocFile>();

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
            this.Includes.Add(new Bam.Core.ScaffoldLocation(baseLocation, pattern, Bam.Core.ScaffoldLocation.ETypeHint.File));
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
            this.Excludes.Add(new Bam.Core.ScaffoldLocation(baseLocation, pattern, Bam.Core.ScaffoldLocation.ETypeHint.File));
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

        private System.Collections.Generic.List<Bam.Core.IModule>
        MakeChildModules(
            Bam.Core.LocationArray locationList)
        {
            var moduleCollection = new System.Collections.Generic.List<Bam.Core.IModule>();
            foreach (var location in locationList)
            {
                var mocFile = new MocFile();
                mocFile.SourceFileLocation = location;
                moduleCollection.Add(mocFile);
            }
            return moduleCollection;
        }

        Bam.Core.ModuleCollection
        Bam.Core.INestedDependents.GetNestedDependents(
            Bam.Core.Target target)
        {
            var collection = new Bam.Core.ModuleCollection();

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
                    collection.Add(objectFile as Bam.Core.IModule);
                }
            }
            return collection;
        }

        #region IModuleCollection Members

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

        #endregion

        #region ICommonOptionCollection implementation

        Bam.Core.BaseOptionCollection Bam.Core.ICommonOptionCollection.CommonOptionCollection
        {
            get;
            set;
        }

        #endregion
    }
}

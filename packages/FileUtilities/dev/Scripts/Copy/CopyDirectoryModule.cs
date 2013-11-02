// <copyright file="CopyDirectoryModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [Opus.Core.ModuleToolAssignment(typeof(ICopyFileTool))]
    public class CopyDirectory : Opus.Core.BaseModule, Opus.Core.IModuleCollection
    {
        private Opus.Core.Array<CopyFile> copyFiles = new Opus.Core.Array<CopyFile>();

        public Opus.Core.Location CommonBaseDirectory
        {
            get;
            private set;
        }

        public void Include(Opus.Core.Location baseLocation, string pattern, Opus.Core.Target target)
        {
            // each file to copy needs to know where the parent was set to copy next to
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            Utilities.GetBesideModule(this, target, out besideModule, out dependentModule);

            this.CommonBaseDirectory = new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            // copy recursively
            var allFilesRecursiveScaffold = new Opus.Core.ScaffoldLocation(this.CommonBaseDirectory, "**", Opus.Core.ScaffoldLocation.ETypeHint.File);
            var locations = allFilesRecursiveScaffold.GetLocations();
            foreach (var location in locations)
            {
                CopyFile file = new CopyFile(besideModule, dependentModule);
                file.ProxyPath.Assign(this.ProxyPath);
                file.SourceFile.AbsoluteLocation = location;
                this.copyFiles.Add(file);
            }
        }

        public void Exclude(Opus.Core.Location baseLocation, string pattern)
        {
            // copy recursively
            var dirs = new Opus.Core.ScaffoldLocation(baseLocation, pattern, Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            var allFilesRecursiveScaffold = new Opus.Core.ScaffoldLocation(dirs, "**", Opus.Core.ScaffoldLocation.ETypeHint.File);
            var locations = allFilesRecursiveScaffold.GetLocations();
            var toRemove = new Opus.Core.Array<CopyFile>();
            foreach (var location in locations)
            {
                foreach (var copyFile in this.copyFiles)
                {
                    if (copyFile.SourceFile.AbsoluteLocation == location)
                    {
                        toRemove.Add(copyFile);
                    }
                }
            }

            foreach (var file in toRemove)
            {
                this.copyFiles.Remove(file);
            }
        }

        private System.Collections.Generic.Dictionary<Opus.Core.Location, Opus.Core.UpdateOptionCollectionDelegateArray> DeferredUpdates
        {
            get;
            set;
        }

        #region IModuleCollection implementation

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
            Opus.Core.ModuleCollection collection = new Opus.Core.ModuleCollection();
            foreach (CopyFile file in this.copyFiles)
            {
                collection.Add(file as Opus.Core.IModule);
            }
            return collection;
        }
        
        #endregion
    }
}

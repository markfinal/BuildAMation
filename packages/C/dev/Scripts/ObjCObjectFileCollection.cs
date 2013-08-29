// <copyright file="ObjCObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C.ObjC
{
    /// <summary>
    /// ObjectiveC object file collection
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(IObjCCompilerTool))]
    public class ObjectFileCollection : ObjectFileCollectionBase
    {
        public void Add(ObjectFile objectFile)
        {
            this.list.Add(objectFile);
        }

        protected override System.Collections.Generic.List<Opus.Core.IModule> MakeChildModules(Opus.Core.StringArray pathList)
        {
            var objectFileList = new System.Collections.Generic.List<Opus.Core.IModule>();

            foreach (var path in pathList)
            {
                var objectFile = new ObjectFile();
                objectFile.ProxyPath.Assign(this.ProxyPath);
                objectFile.SourceFile.AbsolutePath = path;
                objectFileList.Add(objectFile);
            }

            return objectFileList;
        }

#if false
        // deprecated
        public void Include(object owner, params string[] pathSegments)
        {
            var package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            var packagePath = package.Identifier.Path;
            var proxyPath = (owner as Opus.Core.BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier.Location).CachedPath;
            }

            var filePaths = Opus.Core.File.GetFiles(packagePath, pathSegments);
            foreach (var path in filePaths)
            {
                var objectFile = new ObjectFile();
                objectFile.ProxyPath.Assign(this.ProxyPath);
                objectFile.SourceFile.AbsolutePath = path;
                this.list.Add(objectFile);
            }
        }

        // deprecated
        public void Exclude(object owner, params string[] pathSegments)
        {
            var package = Opus.Core.PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Opus.Core.Exception("Unable to locate package '{0}'", owner.GetType().Namespace);
            }

            var packagePath = package.Identifier.Path;
            var proxyPath = (owner as Opus.Core.BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier.Location).CachedPath;
            }

            var filePaths = Opus.Core.File.GetFiles(packagePath, pathSegments);
            var toRemove = new System.Collections.Generic.List<ObjectFile>();
            foreach (var path in filePaths)
            {
                foreach (ObjectFile file in this.list)
                {
                    if (file.SourceFile.AbsolutePath == path)
                    {
                        toRemove.Add(file);
                    }
                }
            }

            foreach (var file in toRemove)
            {
                this.list.Remove(file);
            }
        }
#endif
    }
}

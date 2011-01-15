// <copyright file="CPlusPlusObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C.CPlusPlus
{
    /// <summary>
    /// C++ object file collection
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(Compiler),
                                   typeof(ExportCompilerOptionsDelegateAttribute),
                                   typeof(LocalCompilerOptionsDelegateAttribute),
                                   C.ClassNames.CPlusPlusCompilerToolOptions)]
    public class ObjectFileCollection : ObjectFileCollectionBase
    {
        public void Add(ObjectFile objectFile)
        {
            this.list.Add(objectFile);
        }

        public void Add(params string[] fileParts)
        {
            this.list.Add(new ObjectFile(fileParts));
        }

        public void AddUsingWildcards(params string[] fileParts)
        {
            Opus.Core.PackageInformation package = Opus.Core.PackageUtilities.GetOwningPackage(this);
            if (null == package)
            {
                throw new Opus.Core.Exception(System.String.Format("Unable to locate package for object '{0}'; package name used was '{1}'", this.GetType().FullName, this.GetType().Namespace), false);
            }

            Opus.Core.File path = new Opus.Core.File(fileParts);
            string[] files = System.IO.Directory.GetFiles(package.Directory, path.RelativePath, System.IO.SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string endPart = file.Remove(0, package.Directory.Length).TrimStart(new char[] { System.IO.Path.DirectorySeparatorChar });
                string[] split = endPart.Split(new char[] { System.IO.Path.DirectorySeparatorChar });
                this.list.Add(new ObjectFile(split));
            }
        }
    }
}
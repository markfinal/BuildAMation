// <copyright file="ObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C object file collection
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(Compiler),
                                   typeof(ExportCompilerOptionsDelegateAttribute),
                                   typeof(LocalCompilerOptionsDelegateAttribute),
                                   ClassNames.CCompilerToolOptions)]
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

            bool runningOnMono = System.Type.GetType("Mono.Runtime") != null;
            if (runningOnMono)
            {
                // workaround for this bug http://www.mail-archive.com/mono-bugs@lists.ximian.com/msg71506.html

                string baseDir = package.Directory;
                int i = 0;
                for (; i < fileParts.Length; ++i)
                {
                    string baseDirTest = System.IO.Path.Combine(baseDir, fileParts[i]);
                    if (System.IO.Directory.Exists(baseDirTest))
                    {
                        baseDir = baseDirTest;
                    }
                    else
                    {
                        break;
                    }
                }

                if (i != fileParts.Length - 1)
                {
                    throw new Opus.Core.Exception(System.String.Format("Unable to locate path, starting with '{0}' and ending in '{1}'", baseDir, fileParts[i]));
                }

                Opus.Core.File path = new Opus.Core.File(fileParts[fileParts.Length - 1]);
                try
                {
                    string[] files = System.IO.Directory.GetFiles(baseDir, path.RelativePath, System.IO.SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        string endPart = file.Remove(0, package.Directory.Length).TrimStart(new char[] { System.IO.Path.DirectorySeparatorChar });
                        string[] split = endPart.Split(new char[] { System.IO.Path.DirectorySeparatorChar });
                        this.list.Add(new ObjectFile(split));
                    }
                }
                catch (System.ArgumentException exception)
                {
                    string message = exception.Message + " Base directory '" + baseDir + "'. Sub path '" + path.RelativePath + "'";
                    throw new Opus.Core.Exception(message);
                }
            }
            else
            {
                Opus.Core.File path = new Opus.Core.File(fileParts);
                try
                {
                    string[] files = System.IO.Directory.GetFiles(package.Directory, path.RelativePath, System.IO.SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        string endPart = file.Remove(0, package.Directory.Length).TrimStart(new char[] { System.IO.Path.DirectorySeparatorChar });
                        string[] split = endPart.Split(new char[] { System.IO.Path.DirectorySeparatorChar });
                        this.list.Add(new ObjectFile(split));
                    }
                }
                catch (System.ArgumentException exception)
                {
                    string message = exception.Message + " Base directory '" + package.Directory + "'. Sub path '" + path.RelativePath + "'";
                    throw new Opus.Core.Exception(message);
                }
            }
        }
    }
}